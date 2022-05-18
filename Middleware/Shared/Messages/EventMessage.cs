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
        public SourceType Source { get; set; }
        [MaxLength(30)]
        [XmlElement("EntityType")]
        public string EntityType { get; set; } = String.Empty;
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
 

            public EventMessage(ResourceDto resourceDto, EventDto eventDto, CrudMethod crudMethod, ResourceDto organiserResourceDto)
            {
                UUID_nr = resourceDto.Uuid.ToString();
                Source = resourceDto.Source;
                EntityType = resourceDto.EntityType;
                SourceEntityId = resourceDto.SourceEntityId;
                EntityVersion = resourceDto.EntityVersion;
                Method = crudMethod;

                OrganiserUUID = organiserResourceDto.Uuid.ToString();
                IsActive = eventDto.Status;
                Title = eventDto.Name;
                StartDateUTC = eventDto.Start;
                EndDateUTC = eventDto.End;
            }
    }
}