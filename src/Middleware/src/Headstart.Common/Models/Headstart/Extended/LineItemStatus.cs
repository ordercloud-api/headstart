using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Headstart.Common.Models.Headstart.Extended
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum LineItemStatus
	{
		Complete,
		Submitted,
		Open,
		BackOrdered,
		Canceled,
		CancelRequested,
		CancelDenied,
		Returned,
		ReturnRequested,
		ReturnDenied
	}
}