using Common.Standard.REST.Interfaces.APIModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vision.Core.REST.Auth.DTO
{
    public class CredentialModel : ICredentialModel
    {
        public string UserName { get; set; }
        public string Site { get; set; }
        public string Password { get; set; }
    }
}
