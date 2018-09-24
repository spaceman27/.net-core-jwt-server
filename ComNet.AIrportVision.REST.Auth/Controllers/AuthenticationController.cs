using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Common.Standard.REST.Base.Controllers;
using Common.Standard.REST.Base.Services;
using Vision.Core.REST.Auth.DTO;
using Common.Standard.REST.Interfaces.Services;
using Common.Standard.REST.Interfaces.API;
using Common.Standard.REST.Base.DataModel;
using System.Threading.Tasks;
using Vision.Core.REST.Auth.Service;
using System.Security.Claims;
using Common.Standard.REST.DTO;

namespace ComNet.AirportVision.REST.Auth.Controllers
{
    [Produces("application/json")]
    [Route("v1/api/Authentication")]
    [Authorize]
    public class AuthenticationController : Controller
    {
        private ILoginHandler<UsersDTO> _loginHandler;
        private readonly IConfiguration config;
        private readonly UserSettings settings;
        public IAPIModelService<UsersDTO> Service { get; set; }
        public ILogger<AuthenticationController> Logger { get; }

        /// <summary>
        /// Constructor - uses dependency injection and calls the base constructor.
        /// </summary>
        /// <param name="svc">DI service.</param>
        /// <param name="logger">DI logger</param>
        public AuthenticationController(IAPIModelService<UsersDTO> service, ILoginHandler<UsersDTO> loginHandler, ILogger<AuthenticationController> logger, IConfiguration config, UserSettings settings)
        {
            _loginHandler = loginHandler;
            Logger = logger;
            this.config = config;
            this.settings = settings;
            this.Service = service;
        }

        /// <summary>
        /// Use this to send in a User Name and Password to receive a token good for a configurable
        ///     amount of time (see appsettings.json)
        /// </summary>
        /// <param name="credential">An object with the username and password</param>
        /// <param name="site"></param>
        /// <returns></returns>
        /// <response code="200">Login Successful</response>
        /// <response code="403">Login Failed</response>
        /// <response code="500">Server Exception</response>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(200, Type = typeof(ILoginToken<UsersDTO>))]
        [ProducesResponseType(403, Type = typeof(ErrorModel))]
        [ProducesResponseType(500, Type = typeof(ErrorModel))]
        public async Task<IActionResult> Login([FromBody]CredentialModel credential, [FromQuery]String site = null)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(credential.Site))
                    credential.Site = site;
                else if (!String.IsNullOrWhiteSpace(credential.Site) && !String.Equals(site ?? "", credential.Site ?? "", StringComparison.OrdinalIgnoreCase))
                    return new ObjectResult(new ErrorModel { Message = @"Site must be specified in either the credential or in an HTML query. If both are specified, then they must be equal." })
                    {
                        StatusCode = 403
                    };

                var token = await _loginHandler.Login(credential, this.config["RESTTokenSettings:Audience"], site);
                if (token != null)
                {
                    return Ok(token);
                }

                return new ObjectResult(new ErrorModel { Message = @"Invalid credentials or user does not have access to site." })
                {
                    StatusCode = 403
                };
            }
            catch (Exception ex)
            {
                ErrorModel error = new ErrorModel();
                this.Logger.LogError(ex, "ERROR {0} : {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), ex.Message);
                error.Message = ex.ToString();
                OkObjectResult result = new OkObjectResult(error);

                return result;
            }
        }

        /// <summary>
        /// Refreshes the login using the current authentication credentials.
        /// </summary>
        /// <returns></returns>
        [HttpPost("RefreshLogin")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ExcludeFromCodeCoverage]
        public async Task<IActionResult> RefreshLogin()
        {
            try
            {
                var userName = Request.HttpContext.User.Claims.FirstOrDefault(s => s.Type == ClaimTypes.NameIdentifier)?.Value;
                int siteId = int.Parse(Request.HttpContext.User.Claims.FirstOrDefault(c => c.Type.ToLower() == "site").Value);
                CredentialModel credential = (this.Service as UsersService).GetCurrentCredential(userName, siteId);

                var token = await _loginHandler.LoginByContext(this.config["RESTTokenSettings:Audience"], credential.Site);
                if (token != null)
                {
                    return Ok(token);
                }

                return new ObjectResult(new ErrorModel
                {
                    Message = $"User {credential.UserName} is not granted access to the API."
                })
                {
                    StatusCode = 403
                };
            }
            catch (Exception ex)
            {
                ErrorModel error = new ErrorModel();
                this.Logger.LogError(ex, "ERROR {0} : {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), ex.Message);
                error.Message = ex.ToString();
                OkObjectResult result = new OkObjectResult(error);

                return result;
            }
        }

        /// <summary>
        /// Returns a list of strings that represent Site codes.  Used for login purposes.
        /// </summary>
        /// <returns></returns>
        [HttpGet("SiteList")]
        [AllowAnonymous]
        public IActionResult SiteList()
        {
            try
            {
                return new OkObjectResult((this.Service as UsersService).GetSiteList());
            }
            catch (Exception ex)
            {
                ErrorModel error = new ErrorModel();
                this.Logger.LogError(ex, "ERROR {0} : {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), ex.Message);
                error.Message = ex.ToString();
                OkObjectResult result = new OkObjectResult(error);

                return result;
            }
        }

        /// <summary>
        /// Use this to login to the service if authenticating with Windows Auth.  The token is good
        ///     for a configurable amountof time.
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Login Successful</response>
        /// <response code="403">Login Failed</response>
        [HttpPost("windowslogin")]
        [ProducesResponseType(200, Type = typeof(ILoginToken<UsersDTO>))]
        [ExcludeFromCodeCoverage]
        public async Task<IActionResult> WindowsLogin([FromQuery]String site = null)
        {
            try
            {
                CredentialModel credential = new CredentialModel
                {
                    UserName = Request.HttpContext.User.Identity.Name,
                    Site = site
                };

                var token = await _loginHandler.LoginByContext(this.config["RESTTokenSettings:Audience"], site);

                if (token != null)
                {
                    return Ok(token);
                }

                return new ObjectResult(new ErrorModel
                {
                    Message = $"User {credential.UserName} is not granted access to the API."
                })
                {
                    StatusCode = 403
                };
            }
            catch (Exception ex)
            {
                ErrorModel error = new ErrorModel();
                this.Logger.LogError(ex, "ERROR {0} : {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), ex.Message);
                error.Message = ex.ToString();
                OkObjectResult result = new OkObjectResult(error);

                return result;
            }
        }

        [HttpPost("apiKeyLogin")]
        [AllowAnonymous]
        public async Task<IActionResult> ApiKeyLogin([FromBody]string apiKey)
        {
            try
            {
                var token = await this._loginHandler.LoginByApiKey(apiKey);

                if (token != null)
                {
                    return Ok(token);
                }

                return new ObjectResult(new ErrorModel
                {
                    Message = $"API Key {apiKey} is not granted access to the API."
                })
                {
                    StatusCode = 403
                };
            }
            catch (Exception ex)
            {
                ErrorModel error = new ErrorModel();
                this.Logger.LogWarning(ex, "WARN {0} : {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), ex.Message);
                error.Message = ex.Message;
                OkObjectResult result = new OkObjectResult(error);

                return result;
            }
        }
    }
}