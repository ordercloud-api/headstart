using System;
using System.Collections.Generic;
using System.Text;
using ordercloud.integrations.library;

namespace ordercloud.integrations.smartystreets
{
	public class InvalidBuyerAddressException : OrderCloudIntegrationException
	{
		public InvalidBuyerAddressException(BuyerAddressValidation validation) : base("InvalidAddress", "Address not valid", validation) { }
	}
}
