using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Middleware.Shared.Models
{
    // Use terms of xsd
    public class BookingDto
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public int EventId { get; set; }
        [Required]
        public int PersonId { get; set; }
        [Required]
        public int BookingSpaces { get; set; }
        [Required]
        public int BookingStatus { get; set; }
    }
}