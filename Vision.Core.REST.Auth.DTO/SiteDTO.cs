using Common.Standard.REST.DTO;
using Common.Standard.REST.Interfaces.APIModel;
using Common.Standard.REST.Interfaces.DataModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vision.Core.REST.Auth.DTO
{
    public class SiteDTO : IAPIModel<int>
    {
        public int ID { get; set; }
        public string Key { get; set; }
        [JsonProperty(ItemConverterType = typeof(LinkDTOConverter))]
        public ILinkDTOCollection<ILinkDTO> Links { get; set; } = new LinkDTODictionary();
    }
}
