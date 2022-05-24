using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Middleware.Shared.Models
{
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
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = String.Empty;
        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = String.Empty;
        [Required]
        [MaxLength(50)]
        public string Email { get; set; } = String.Empty;

        public override string ToString()
        {
            return $"{this.Id}, {this.EventId}, {this.PersonId}, {this.BookingSpaces}, {this.BookingStatus}, {this.FirstName}, {this.LastName}, {this.Email}";
        }
    }
}