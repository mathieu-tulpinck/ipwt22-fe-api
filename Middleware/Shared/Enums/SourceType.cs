using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Middleware.Shared.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SourceType
    {
        Planning,
        Crm,
        FrontEnd
    }
}