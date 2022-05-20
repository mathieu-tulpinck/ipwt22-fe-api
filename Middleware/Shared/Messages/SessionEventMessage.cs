using Middleware.Shared.Enums;
using Middleware.Shared.Models;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Middleware.Shared.Messages
{
    [Serializable]
    [XmlType("SessionEvent")]
    [XmlRoot("SessionEvent")]
    public class SessionEventMessage
    {
        [MinLength(32)]
        [XmlElement("UUID_Nr")]
        public string UUID_Nr { get; set; } = String.Empty;
        [XmlElement("SourceEntityId")]
        public string SourceEntityId { get; set; } = String.Empty;
        [XmlElement("EntityType")]
        public EntityType EntityType { get; set; }
        [XmlElement("EntityVersion")]
        public int EntityVersion { get ; set; }
        [XmlElement("Source")]
        public Source Source { get; set; }
        [MaxLength(30)]
        [XmlElement("Method")]
        public CrudMethod Method { get; set; }
        [MaxLength(30)]
        [XmlElement("Title")]
        public string Title { get; set; } = String.Empty;
        [XmlElement("StartDateUTC", DataType = "dateTime")]
        public DateTime StartDateUTC { get; set; }
        [XmlElement("EndDateUTC", DataType = "dateTime")]
        public DateTime EndDateUTC { get; set; }
        [MinLength(32)]
        [XmlElement("OrganiserUUID")]
        public string OrganiserUUID { get; set; } = String.Empty;
        [XmlElement("IsActive")]    
        public bool IsActive { get; set; }

            public SessionEventMessage() {}
            
            public SessionEventMessage(Resource resource, EventDto eventDto, CrudMethod crudMethod, Guid organiserUuid)
            {
                UUID_Nr = resource.Uuid.ToString();
                SourceEntityId = resource.SourceEntityId.ToString();
                EntityType = resource.EntityType;
                EntityVersion = resource.EntityVersion;
                Source = resource.Source;
                Method = crudMethod;
                Title = eventDto.Name;
                StartDateUTC = eventDto.Start;
                EndDateUTC = eventDto.End;
                OrganiserUUID = organiserUuid.ToString();
                IsActive = eventDto.Status;
            }
    }
}