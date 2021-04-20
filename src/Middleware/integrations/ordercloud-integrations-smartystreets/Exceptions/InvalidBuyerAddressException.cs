using System;
using System.Collections.Generic;
using System.Text;
using ordercloud.integrations.library;
using OrderCloud.Catalyst;

namespace ordercloud.integrations.smartystreets
{
	public class InvalidBuyerAddressException : CatalystBaseException
	{
		public InvalidBuyerAddressException(BuyerAddressValidation validation) : base("InvalidAddress", 400, "Address not valid", validation) { }
	}
}
