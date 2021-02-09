using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Headstart.Common.Models
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum LogLevel { Progress, Error, Warn, Success }
}
