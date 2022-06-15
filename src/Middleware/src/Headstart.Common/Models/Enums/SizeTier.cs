using System.Text.Json.Serialization;

namespace Headstart.Common.Models
{
    // measured in how many of the product fit in a 22x22x22 box
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SizeTier
    {
        // ships alone
        G,

        // 2-5
        A,

        // 5-15
        B,

        // 15-49
        C,

        // 50-99
        D,

        // 100-999
        E,

        // 1000+
        F,
    }
}
