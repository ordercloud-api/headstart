using System;
using System.Collections.Generic;
using System.Text;
using ordercloud.integrations.library;
using OrderCloud.Catalyst;

namespace ordercloud.integrations.smartystreets
{
	public class InvalidBuyerAddressException : CatalystBaseException
	{
		public InvalidBuyerAddressException(BuyerAddressValidation validation) : base("InvalidAddress", "Address not valid", validation, 400) { }
	}
}
