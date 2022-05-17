using Middleware.Shared.Enums;

namespace Middleware.Shared.Models
{
    public class ResourceDto
    {
        public Guid Uuid { get; set; }
        public SourceType Source { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public int SourceEntityId { get; set; }
        public int EntityVersion { get; set; }
    }
}