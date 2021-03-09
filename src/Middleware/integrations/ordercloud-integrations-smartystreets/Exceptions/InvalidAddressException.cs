using System;
using System.Collections.Generic;
using System.Text;
using ordercloud.integrations.library;
using OrderCloud.Catalyst;

namespace ordercloud.integrations.smartystreets
{
	public class InvalidAddressException : CatalystBaseException
	{
		public InvalidAddressException(AddressValidation validation) : base("InvalidAddress", 400, "Address not valid", validation) { }
	}
}


