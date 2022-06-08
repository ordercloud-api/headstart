using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Headstart.Common.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum WorkItemAction
    {
        Ignore,
        Create,
        Update,
        Patch,
        Delete,
        Get,
        SyncShipments,
    }
}
