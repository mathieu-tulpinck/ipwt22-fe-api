using Middleware.Shared.Enums;

namespace Middleware.Shared.Models
{
    public class ResourceCreateDto
    {
        public SourceType Source { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public int SourceEntityId { get; set; }
        public int EntityVersion { get; set; }

        public ResourceCreateDto(SourceType source, string entityType, int sourceEntityId, int entityVersion)
        {
            Source = source;
            EntityType = entityType;
            SourceEntityId = sourceEntityId;
            EntityVersion = entityVersion;
        }
    }
}