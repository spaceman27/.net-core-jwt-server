using Common.Core.EFCore.AirportVision.Models;
using Common.Standard.REST.Base.DataModel;
using Common.Standard.REST.Interfaces.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vision.Core.REST.Auth.DTO;

namespace Vision.Core.REST.Auth.Data
{
    public class UserOptionsRepository : EFGenericRepository<UserOptionsDTO, UserOptions>
    {
        public UserOptionsRepository(IProjector<UserOptions, UserOptionsDTO> projector) : base(projector)
        {
        }

    }
}
