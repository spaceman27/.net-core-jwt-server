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
    public class UserOptionsDTO : IAPIModel<string[]>
    {
        public string[] ID { get; set; }
        public string Key { get; set; }
        public string UserName { get; set; }
        public string OptionName { get; set; }
        public string Value { get; set; }
        [JsonProperty(ItemConverterType = typeof(LinkDTOConverter))]
        public ILinkDTOCollection<ILinkDTO> Links { get; set; } = new LinkDTODictionary();
    }
}
