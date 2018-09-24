using Common.Core.EFCore.AirportVision.Models;
using Common.Standard.REST.Interfaces.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Vision.Core.REST.Auth.DTO;

namespace Vision.Core.REST.Auth.Data
{
    public class UserOptionsProjector : IProjector<UserOptions, UserOptionsDTO>
    {
        public List<string> RelationshipsToSkip => throw new NotImplementedException();

        private readonly Expression<Func<UserOptions, UserOptionsDTO>> toDto;
        private readonly Expression<Func<UserOptionsDTO, UserOptions>> toDb;


        public UserOptionsProjector()
        {
            this.toDto = d =>
                new UserOptionsDTO
                {
                    ID = new[] { d.Login, d.OptionName },
                    Key = $"{d.Login}-{d.OptionName}",
                    UserName = d.Login,
                    OptionName = d.OptionName,
                    Value = d.Value
                };

            this.toDb = d =>
                new UserOptions
                {
                    Login = d.UserName,
                    OptionName = d.OptionName,
                    Value = d.Value
                };
        }

        public IQueryable<UserOptions> Project(IQueryable<UserOptionsDTO> entity)
        {
            return entity.Select(this.toDb);
        }

        public IQueryable<UserOptionsDTO> Project(IQueryable<UserOptions> entity)
        {
            return entity.Select(this.toDto);
        }

        public UserOptions Project(UserOptionsDTO entity)
        {
            return this.toDb.Compile().Invoke(entity);
        }

        public UserOptionsDTO Project(UserOptions entity)
        {
            return this.toDto.Compile().Invoke(entity);
        }
    }
}
