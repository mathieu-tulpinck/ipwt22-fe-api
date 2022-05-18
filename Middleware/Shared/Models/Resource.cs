using Middleware.Shared.Enums;

namespace Middleware.Shared.Models
{
    public class Resource
    {
        public Guid Uuid { get; set; }
        public Source Source { get; set; }
        public EntityType EntityType { get; set; }
        public int SourceEntityId { get; set; }
        public int EntityVersion { get; set; }

        public override string ToString()
        {
            return $"{this.Uuid}, {this.Source}, {this.EntityType}, {this.SourceEntityId}, {this.EntityVersion}";
        }
    }
}