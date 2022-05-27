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
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = String.Empty;
        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = String.Empty;
        [Required]
        [MaxLength(254)]
        public string Email { get; set; } = String.Empty;

    }
}