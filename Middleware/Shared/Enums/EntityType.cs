using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Middleware.Shared.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum EntityType
    {
        ORGANISER,
        EVENT,
    }
}