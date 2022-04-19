using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Headstart.Common.Models.Headstart.Extended
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum ShippingStatus
	{
		Shipped,
		PartiallyShipped,
		Canceled,
		Processing,
		BackOrdered
	}
}