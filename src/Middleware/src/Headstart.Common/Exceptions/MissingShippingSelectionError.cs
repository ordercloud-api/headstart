using System.Collections.Generic;

namespace Headstart.Common.Exceptions
{
	public class MissingShippingSelectionError
	{
		public IEnumerable<string> ShipFromAddressIDsRequiringAttention { get; set; } = new List<string>();

		public MissingShippingSelectionError(IEnumerable<string> ids)
		{
			ShipFromAddressIDsRequiringAttention = ids;
		}
	}
}