using Middleware.Shared.Enums;

namespace Middleware.Shared.Models
{
    public class ResourceDto
    {
        public Source Source { get; set; }
        public EntityType EntityType { get; set; }
        public int SourceEntityId { get; set; }
        public int EntityVersion { get; set; }

        public ResourceDto(Source source, EntityType entityType, int sourceEntityId, int entityVersion)
        {
            Source = source;
            EntityType = entityType;
            SourceEntityId = sourceEntityId;
            EntityVersion = entityVersion;
        }
    }
}