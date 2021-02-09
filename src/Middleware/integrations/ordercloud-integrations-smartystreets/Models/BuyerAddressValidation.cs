using OrderCloud.SDK;
using System;
using System.Collections.Generic;
using System.Text;

namespace ordercloud.integrations.smartystreets
{
	public class BuyerAddressValidation
	{
		public BuyerAddressValidation(BuyerAddress raw)
		{
			RawAddress = raw;
		}

		public BuyerAddress RawAddress { get; set; }
		public BuyerAddress ValidAddress { get; set; }
		public bool ValidAddressFound => ValidAddress != null;
		public List<BuyerAddress> SuggestedAddresses { get; set; } = new List<BuyerAddress>() { };
		// https://smartystreets.com/docs/cloud/us-street-api#footnotes
		public string GapBetweenRawAndValid { get; set; }
	}
}
