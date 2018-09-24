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
    public class SiteProjector : IProjector<Sites, SiteDTO>
    {
        public List<string> RelationshipsToSkip => throw new NotImplementedException();
        private readonly Expression<Func<Sites, SiteDTO>> toDto;
        private readonly Expression<Func<SiteDTO, Sites>> toDb;

        public SiteProjector()
        {
            this.toDto = d =>
                new SiteDTO
                {
                    ID = d.SiteId,
                    Key = d.IataCode
                };

            this.toDb = d =>
                new Sites
                {
                    SiteId = d.ID,
                    IataCode = d.Key
                };
        }

        public IQueryable<Sites> Project(IQueryable<SiteDTO> entity)
        {
            return entity.Select(this.toDb);
        }

        public IQueryable<SiteDTO> Project(IQueryable<Sites> entity)
        {
            return entity.Select(this.toDto);
        }

        public Sites Project(SiteDTO entity)
        {
            return this.toDb.Compile().Invoke(entity);
        }

        public SiteDTO Project(Sites entity)
        {
            return this.toDto.Compile().Invoke(entity);
        }
    }
}
