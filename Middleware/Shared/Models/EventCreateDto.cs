namespace Middleware.Shared.Models
{
    // Use terms of xsd
    public class EventCreateDto 
    {
        public int Id { get; set; }
        public int Owner { get; set; }
        public int Status { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Method { get; set; } = string.Empty;
    }
}