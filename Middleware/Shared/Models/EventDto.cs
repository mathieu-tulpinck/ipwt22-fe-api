using Middleware.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace Middleware.Shared.Models
{
    // Use terms of xsd
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
        public DateTime Start { get; set; }
        [Required]
        public DateTime End { get; set; }
        [Required]
        public CrudMethod Method { get; set; }

        public override string ToString()
        {
            return $"{this.Id}, {this.Owner}, {this.Status.ToString()}, {this.Name}, {this.Start.ToString()}, {this.End.ToString()}, {this.Method.ToString()}";
        }
    }
}