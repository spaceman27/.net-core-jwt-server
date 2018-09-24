using Common.Core.EFCore.AirportVision.Models;
using Common.Standard.REST.Base.DataModel;
using Common.Standard.REST.Interfaces.DataModel;
using Vision.Core.REST.Auth.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using Common.Standard.REST.Base.Services;

namespace Vision.Core.REST.Auth.Data
{
    //TODO: modify the repository to correctly replace and update entities and replace links.
    public class UsersRepository : EFGenericRepository<UsersDTO, Users>
    {
        public UsersRepository(IProjector<Users, UsersDTO> projector) : base(projector)
        {
        }

        public async Task<UsersDTO> GetUserByApiKey(string apiKey)
        {
            Guid key = Guid.Parse(apiKey);
            var res = await this.Context.Set<UserApiKeys>().Include(s => s.Users).FirstOrDefaultAsync(s => s.ApiKey.Equals(key) && (bool)s.Enabled && DateTime.Now >= s.ActiveStart && DateTime.Now <= s.ActiveEnd);
            
            return res == null ? null : this.GetProjection(res.Users);
        }
    }
}
