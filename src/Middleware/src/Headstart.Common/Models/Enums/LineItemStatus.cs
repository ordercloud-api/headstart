using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Headstart.Common.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum LineItemStatus
    {
        Complete,
        Submitted,
        Open,
        Backordered,

        // TODO: keeping these unused status types for now to avoid breaking headstart too much
        // in the future would like to do away with line item status and derive from ordercloud objects
        Canceled,
        CancelRequested,
        CancelDenied,
        Returned,
        ReturnRequested,
        ReturnDenied,
    }
}
