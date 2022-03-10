using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Headstart.Common.Models.Headstart.Extended
{
	public class SpecUI
	{
		public ControlType ControlType { get; set; } = ControlType.Text;
	}

	[JsonConverter(typeof(StringEnumConverter))]
	public enum ControlType
	{
		Text, 
		DropDown, 
		Checkbox, 
		Range
	}
}