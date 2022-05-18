using Middleware.Shared.Enums;

namespace Middleware.Shared.Models
{
    public class Resource
    {
        public Guid Uuid { get; set; }
        public SourceType Source { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public int SourceEntityId { get; set; }
        public int EntityVersion { get; set; }

        public override string ToString()
        {
            return $"{this.Uuid}, {this.Source}, {this.EntityType}, {this.SourceEntityId}, {this.EntityVersion}";
        }
    }
}