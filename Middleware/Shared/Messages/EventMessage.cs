using Middleware.Shared.Enums;
using Middleware.Shared.Models;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Middleware.Shared.Messages
{
    [Serializable]
    [XmlType("SessionEvent")]
    [XmlRoot("SessionEvent")]
    public class EventMessage
    {
        [MinLength(32)]
        [XmlElement("UUID_nr")]
        public string UUID_nr { get; set; } = String.Empty;
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
        
        [MinLength(32)]
        [XmlElement("OrganiserUuid")]
        public string OrganiserUUID { get; set; } = String.Empty;
        [XmlElement("IsActive")]    
        public bool IsActive { get; set; }
        [MaxLength(30)]
        [XmlElement("Title")]
        public string Title { get; set; } = String.Empty;
        [XmlElement("StartDateUTC", DataType = "dateTime")]
        public DateTime StartDateUTC { get; set; }
        [XmlElement("EndDateUTC", DataType = "dateTime")]
        public DateTime EndDateUTC { get; set; }
 

            public EventMessage() {}
            
            public EventMessage(Resource resource, EventDto eventDto, CrudMethod crudMethod, Guid organiserUuid)
            {
                UUID_nr = resource.Uuid.ToString();
                Source = resource.Source;
                EntityType = resource.EntityType;
                SourceEntityId = resource.SourceEntityId;
                EntityVersion = resource.EntityVersion;
                Method = crudMethod;

                OrganiserUUID = organiserUuid.ToString();
                IsActive = eventDto.Status;
                Title = eventDto.Name;
                StartDateUTC = eventDto.Start;
                EndDateUTC = eventDto.End;
            }
    }
}