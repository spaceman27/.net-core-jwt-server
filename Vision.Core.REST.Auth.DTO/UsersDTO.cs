using Common.Standard.REST.Interfaces.APIModel;
using Common.Standard.REST.Interfaces.DataModel;
using Common.Standard.REST.DTO;
using System;
using Newtonsoft.Json;

namespace Vision.Core.REST.Auth.DTO
{
    //TODO: Add any fields to the DTO that you want returned to the user.
    public class UsersDTO : IAPIModel<int>
    {
        public int ID { get; set; }
        public string Key { get; set; }
        public string Name1 { get; set; }
        public string Name2 { get; set; }
        public string Description { get; set; }
        public bool Active { get; set; }
        /*TODO:
            If any fields are variable (such as a field value list in daily values)
                use a FieldDTODictionary class to implement.
        [XmlAnyElement]
        public FieldDTODictionary Fields { get; set; } = new FieldDTODictionary();
        */
        [JsonProperty(ItemConverterType = typeof(LinkDTOConverter))]
        public ILinkDTOCollection<ILinkDTO> Links { get; set; } = new LinkDTODictionary();
    }
}
