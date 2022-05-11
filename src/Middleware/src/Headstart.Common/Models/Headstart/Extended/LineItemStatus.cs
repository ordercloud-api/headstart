using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Headstart.Models.Extended
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum LineItemStatus
    {
        Complete,
        Submitted,
        Open,
        Backordered,
        Canceled,
        CancelRequested,
        CancelDenied,
        Returned,
        ReturnRequested,
        ReturnDenied
    }
}
