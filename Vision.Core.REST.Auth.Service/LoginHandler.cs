using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Net;
using Vision.Core.REST.Auth.DTO;
using Common.Standard.REST.Interfaces.API;
using Common.Standard.REST.Interfaces.Services;
using Common.Standard.REST.Interfaces.Security;
using Core.REST.APV.DataLayer.DTO;
using Common.Standard.REST.Interfaces.APIModel;
using System.Net.Http;
using System.Threading.Tasks;
using Vision.Core.REST.Auth.Service;
using Common.Standard.REST.Base.Services;

namespace Vision.Core.REST.Auth.Service
{
    /// <summary>
    /// The login business logic to log into AirportVision.
    /// </summary>
    public class LoginHandler: ILoginHandler<UsersDTO>
    {
        private IAPIModelService<UsersDTO> _userService;
        private IConfiguration _config;
        private readonly IEncryption<string, string> encryption;
        private readonly HttpContext context;
        private bool skipPassword = false;

        /// <summary>
        /// Constructor. Uses DI
        /// </summary>
        /// <param name="userService">DI service to get user information.</param>
        /// <param name="config">DI config information.</param>
        public LoginHandler(IAPIModelService<UsersDTO> userService, IConfiguration config, IEncryption<string, string> encryption, IHttpContextAccessor contextAccessor)
        {
            _userService = userService;
            _config = config;
            this.encryption = encryption;
            this.context = contextAccessor.HttpContext;
        }
        
        private string Decrypt(string toDecrypt)
        {
            if (this.encryption == null) return toDecrypt;

            return this.encryption.Decrypt(toDecrypt);
        }

        public async Task<ILoginToken<UsersDTO>> Login(ICredentialModel loginCredentials, string audience, string site)
        {
            String userName = loginCredentials.UserName;
            String password = "";

            string secretKey = _config["RESTTokenSettings:Secret"];
            string issuer = _config["RESTTokenSettings:Issuer"];
            int minutesGoodFor = int.Parse(_config["RESTTokenSettings:MinutesGoodFor"]);

            userName = loginCredentials.UserName;
            password = this.Decrypt(loginCredentials.Password);


            this._userService.IncludeLink("apiroles");
            this._userService.IncludeLink("sites");
            var user = (this._userService.GetByKey(userName)).Result;

            ILoginToken<UsersDTO>  token = null;

            if (user == null)
                return token;

            var userSites = user?.Links["sites"].LinkedEntities.Cast<SiteDTO>();
            var userSite = userSites.FirstOrDefault(s => s.Key == site);


            var userOptions = user.Links["options"]?.LinkedEntities.Cast<UserOptionsDTO>();

            var valid = userOptions.Any(uo => uo.OptionName.ToLower() == "userpassword" && this.Decrypt(uo.Value) == password) || skipPassword;
            valid = valid && (userSite != null || String.IsNullOrWhiteSpace(site));

            if (valid)
            {
                var claims = new List<Claim>
                {
                   new Claim(JwtRegisteredClaimNames.Sub, user.Key),
                   new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                if (userSite != null)
                    claims.Add(new Claim("Site", userSite.ID.ToString()));
                else
                    userSites.ToList().ForEach(s => claims.Add(new Claim("Site", s.ID.ToString())));

                string hostName = "";
                string ipAddress = "";
                try
                {
                    var remIpAddress = this.context.Connection?.RemoteIpAddress;
                    ipAddress = remIpAddress?.IsIPv4MappedToIPv6 == true ? remIpAddress.MapToIPv4()?.ToString() : remIpAddress.ToString();
                    hostName = Dns.GetHostEntry(ipAddress)?.HostName;
                }
                catch { }

                claims.Add(new Claim(ClaimTypes.Role, "apv"));
                claims.Add(new Claim("IPAddress", ipAddress ?? ""));
                claims.Add(new Claim("hostName", hostName ?? ""));

                foreach (var role in user.Links["apiroles"]?.LinkedEntities.Cast<ApiRoleDTO>())
                {
                    claims.Add(new Claim(ClaimTypes.Role, role.Key.ToLower()));
                }

                string groupCode = userOptions.FirstOrDefault(uo => uo.OptionName.ToLower() == "usergroupcode").Value ?? "***";

                claims.Add(new Claim("GroupCode", groupCode));

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var expirationDate = DateTime.Now.AddMinutes(minutesGoodFor);

                var tok = new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    claims: claims,
                    expires: expirationDate,
                    signingCredentials: creds

                );

                token = new LoginToken()
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(tok),
                    Expiration = expirationDate,
                    User = user,
                    MinutesValid = minutesGoodFor
                };

                return token;
            }
            return null;
        }

        public async Task<ILoginToken<UsersDTO>> LoginByContext(string audience, string site)
        {
            var credentials = new CredentialModel();
            if (context != null)
            {
                var userName = context?.User?.Identity?.Name ?? context.User.Claims.FirstOrDefault(s => s.Type == ClaimTypes.NameIdentifier)?.Value;
                if (!String.IsNullOrWhiteSpace(userName) && context.User.Identity.IsAuthenticated)
                {
                    skipPassword = true;
                }
                else
                {
                    return null;
                }

                credentials.Site = site;
                credentials.UserName = userName;
            }
            else
                throw new APVRESTException("Unable to login. Context is invalid.");

            return await this.Login(credentials, audience, site);
        }

        public async Task<ILoginToken<UsersDTO>> LoginByApiKey(string apiKey)
        {
            var user = await (this._userService as UsersService).GetByApiKey(apiKey);
            var site = await (this._userService as UsersService).GetSiteByApiKey(apiKey);

            if (user == null || site == null)
                return null;

            var credentials = new CredentialModel()
            {
                UserName = user.Key,
                Site = site.Key
            };

            this.skipPassword = true;

            return await this.Login(credentials, "", credentials.Site);
        }
    }
}
