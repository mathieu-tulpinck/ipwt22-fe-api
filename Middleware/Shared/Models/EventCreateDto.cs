using Middleware.Shared.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel.DataAnnotations;

namespace Middleware.Shared.Models
{
    public class EventCreateDto
    {
        public int Owner { get; set; }
        public bool Status { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}