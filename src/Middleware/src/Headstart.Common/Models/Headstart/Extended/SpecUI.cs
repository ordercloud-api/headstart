using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Headstart.Models.Extended
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ControlType
    {
        Text,
        DropDown,
        Checkbox,
        Range,
    }

    public class SpecUI
    {
        public ControlType ControlType { get; set; } = ControlType.Text;
    }
}
