using Middleware.Shared.Enums;
using Middleware.Shared.Models;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Middleware.Shared.Messages
{
    [Serializable]
    [XmlType("SessionEvent")]
    [XmlRoot("SessionEvent")]
    public class UserMessage
    {
        [MinLength(32)]
        [XmlElement("UUID_Nr")]
        public Guid UUID_Nr { get; set; }
        [XmlElement("Source")]
        public Source Source { get; set; }
        [MaxLength(30)]
        [XmlElement("EntityType")]
        public EntityType EntityType { get; set; }
        [XmlElement("SourceEntityId")]
        public int SourceEntityId { get; set; }
        [XmlElement("EntityVersion")]
        public int EntityVersion { get ; set; }
        [XmlElement("Method")]
        public CrudMethod Method { get; set; }
        

 

            public UserMessage(Resource resource, CrudMethod crudMethod)
            {
                UUID_Nr = resource.Uuid;
                Source = resource.Source;
                EntityType = resource.EntityType;
                SourceEntityId = resource.SourceEntityId;
                EntityVersion = resource.EntityVersion;
                Method = crudMethod;
            }
    }
}