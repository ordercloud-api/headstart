using System;
using System.Collections.Generic;
using System.Text;
using ordercloud.integrations.library;

namespace ordercloud.integrations.smartystreets
{
	public class InvalidAddressException : OrderCloudIntegrationException
	{
		public InvalidAddressException(AddressValidation validation) : base("InvalidAddress", "Address not valid", validation) { }
	}
}


