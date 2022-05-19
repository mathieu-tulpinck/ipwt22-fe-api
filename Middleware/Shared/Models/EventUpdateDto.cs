using Middleware.Shared.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel.DataAnnotations;

namespace Middleware.Shared.Models
{
    // Use terms of xsd
    public class EventUpdateDto
    {
        public int Id { get; set; }
        [Required]
        public int Owner { get; set; }
        public bool? Status { get; set; } = null;
        [MaxLength(50)]
        public string? Name { get; set; } = null;
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime? Start { get; set; } = null;
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime? End { get; set; } = null;

        public override string ToString()
        {
            return $"{this.Id}, {this.Owner}, {this.Status.ToString()}, {this.Name}, {this.Start.ToString()}, {this.End.ToString()}";
        }
    }
}