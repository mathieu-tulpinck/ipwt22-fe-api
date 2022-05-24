using Middleware.Shared.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel.DataAnnotations;

namespace Middleware.Shared.Models
{
    public class EventDto
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public int Owner { get; set; }
        [Required]
        public bool Status { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;
        [Required]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime Start { get; set; }
        [Required]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime End { get; set; }

        public override string ToString()
        {
            return $"{this.Id}, {this.Owner}, {this.Status.ToString()}, {this.Name}, {this.Start.ToString()}, {this.End.ToString()}";
        }
    }
}