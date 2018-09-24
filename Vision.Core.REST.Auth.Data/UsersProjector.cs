using Common.Core.EFCore.AirportVision.Models;
using Common.Standard.REST.Interfaces.DataModel;
using Vision.Core.REST.Auth.DTO;
using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Common.Standard.REST.DTO;

namespace Vision.Core.REST.Auth.Data
{
    public class UsersProjector : IProjector<Users, UsersDTO>
    {
        public List<string> RelationshipsToSkip => throw new NotImplementedException();

        private readonly Expression<Func<Users, UsersDTO>> toDto;
        private readonly Expression<Func<UsersDTO, Users>> toDb;

        public UsersProjector()
        {
            this.toDto = d =>
                new UsersDTO
                {
                    ID = d.UserId,
                    Key = d.UserName,
                    Description = d.UserDescription,
                    Name1 = d.Name1,
                    Name2 = d.Name2,
                    Active = d.Active ?? false,
                    Links = GetLinks(d)
                };

            this.toDb = d => 
                new Users
                {
                    UserId = d.ID,
                    UserName = d.Key,
                    UserDescription = d.Description,
                    Name1 = d.Name1,
                    Name2 = d.Name2,
                    Active = d.Active
                };
        }

        private ILinkDTOCollection<ILinkDTO> GetLinks(Users d)
        {
            var dict = new LinkDTODictionary()
            {
                ["options"] = new LinkDTO
                {
                    LinkedEntities = null
                },
                ["apiroles"] = new LinkDTO
                {
                    LinkedEntities = d.UserApiRoles.Select(s => new ApiRoleDTO { ID = s.ApiRole.Id, Key = s.ApiRole.Name, Description = s.ApiRole.Description })
                },
                ["sites"] = new LinkDTO
                {
                    LinkedEntities = d.UserSites.Select(s => new SiteProjector().Project(s.Site))
                }
            };

            return dict;
        }
        public IQueryable<Users> Project(IQueryable<UsersDTO> entity)
        {
            return entity.Select(this.toDb);
        }

        public IQueryable<UsersDTO> Project(IQueryable<Users> entity)
        {
            return entity.Select(this.toDto);
        }

        public Users Project(UsersDTO entity)
        {
            return this.toDb.Compile().Invoke(entity);
        }

        public UsersDTO Project(Users entity)
        {
            return this.toDto.Compile().Invoke(entity);
        }
    }
}
