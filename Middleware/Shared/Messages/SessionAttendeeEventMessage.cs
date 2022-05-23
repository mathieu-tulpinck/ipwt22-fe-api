using Middleware.Shared.Enums;
using Middleware.Shared.Models;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Middleware.Shared.Messages
{
    [Serializable]
    [XmlType("SessionAttendeeEvent")]
    [XmlRoot("SessionAttendeeEvent")]
    public class SessionAttendeeEventMessage
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
        [XmlElement("AttendeeUUID")]
        public string AttendeeUUID { get; set; } = String.Empty;
        [XmlElement("SessionUUID")]
        public string SessionUUID { get; set; } = String.Empty;
        [XmlElement("InvitationStatus")]
        public InvitationStatus InvitationStatus { get; set; }

        public SessionAttendeeEventMessage() {}
        
        public SessionAttendeeEventMessage(Resource resource, BookingDto bookingDto, CrudMethod crudMethod, Guid eventUuid)
        {
            UUID_Nr = resource.Uuid.ToString();
            SourceEntityId = resource.SourceEntityId.ToString();
            EntityType = resource.EntityType;
            EntityVersion = resource.EntityVersion;
            Source = resource.Source;
            Method = crudMethod;
            AttendeeUUID = resource.Uuid.ToString();
            SessionUUID = eventUuid.ToString();
            switch (bookingDto.BookingStatus) {
                case 0:
                    InvitationStatus = InvitationStatus.PENDING;
                break;
                case 1:
                    InvitationStatus = InvitationStatus.ACCEPTED;
                break;
                case 2:
                    InvitationStatus = InvitationStatus.DECLINED;
                break;
                default:
                    break;
            }
        }
    }
}