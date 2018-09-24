using Common.Standard.REST.Base.Services;
using Common.Standard.REST.Interfaces.DataModel;
using Vision.Core.REST.Auth.DTO;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vision.Core.REST.Auth.Data;

namespace Vision.Core.REST.Auth.Service
{
    public class UsersService : APIModelService<UsersDTO, int>
    {
        private readonly IGenericRepository<UserOptionsDTO> optionsRepo;
        private readonly IGenericRepository<SiteDTO> siteRepo;

        public UsersService(IUnitOfWork unitOfWork, ILogger<UsersService> logger) : base(unitOfWork, logger)
        {
            this.optionsRepo = unitOfWork.GetRepository<UserOptionsDTO>();
            this.siteRepo = unitOfWork.GetRepository<SiteDTO>();
        }

        public override void AddCalculatedFields(UsersDTO model)
        {
            model.Links["options"].LinkedEntities = optionsRepo.Get(s => s.UserName.ToLower() == model.Key.ToLower());
            base.AddCalculatedFields(model);
        }

        protected override void InitRelationshipDBNames()
        {
            RelationshipDBNames = new Dictionary<string, string[]>
            {
                ["options"] = new string[0],
                ["apiroles"] = new String[] { "UserApiRoles.ApiRole" },
                ["sites"] = new String[] { "UserSites.Site" }
            };
        }

        public CredentialModel GetCurrentCredential(String user, int siteId)
        {
            string siteName = this.siteRepo.GetByID(siteId).Key;

            CredentialModel cred = new CredentialModel()
            {
                UserName = user,
                Site = siteName
            };

            return cred;
        }

        public List<String> GetSiteList()
        {
            var sites = this.siteRepo.Get();

            return sites.Select(s => s.Key).ToList();
        }

        public async Task<UsersDTO> GetByApiKey(string apiKey)
        {
            var user = await (this.repo as UsersRepository).GetUserByApiKey(apiKey);

            return user;
        }

        public async Task<SiteDTO> GetSiteByApiKey(string apiKey)
        {
            var site = await (this.siteRepo as SitesRepository).GetByApiKey(apiKey);

            return site;
        }
    }
}
