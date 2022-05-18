using Middleware.Shared.Enums;

namespace Middleware.Shared.Models
{
    public class ResourceDto
    {
        public Source Source { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public int SourceEntityId { get; set; }
        public int EntityVersion { get; set; }

        public ResourceDto(Source source, string entityType, int sourceEntityId, int entityVersion)
        {
            Source = source;
            EntityType = entityType;
            SourceEntityId = sourceEntityId;
            EntityVersion = entityVersion;
        }
    }
}