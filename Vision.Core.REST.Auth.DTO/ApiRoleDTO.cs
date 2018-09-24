using Common.Standard.REST.DTO;
using Common.Standard.REST.Interfaces.APIModel;
using Common.Standard.REST.Interfaces.DataModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Vision.Core.REST.Auth.DTO
{
    public class ApiRoleDTO : IAPIModel<int>
    {
        
        public int ID { get; set; }
        public string Key { get; set; }
        public string Description { get; set; }
        [JsonProperty(ItemConverterType = typeof(LinkDTOConverter))]
        public ILinkDTOCollection<ILinkDTO> Links { get; set; } = new LinkDTODictionary();
    }
}
