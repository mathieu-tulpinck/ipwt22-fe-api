namespace Middleware.Shared.Entities
{
    public class Event
    {
        public int Id { get; set; }
        public int Owner { get; set; }
        public int Status { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime start { get; set; }
        public DateTime end { get; set; }
    }
}