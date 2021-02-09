using System.Collections.Generic;

namespace Headstart.Models.Exceptions
{
	public class MissingShippingSelectionError
	{
		public MissingShippingSelectionError(IEnumerable<string> ids)
		{
			ShipFromAddressIDsRequiringAttention = ids;
		}

		public IEnumerable<string> ShipFromAddressIDsRequiringAttention;
	}
}
