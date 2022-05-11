using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Headstart.Models.Extended
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ObjectStatus
    {
        Draft,
        Published
    }
}
