using OrderCloud.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ordercloud.integrations.avalara
{
	public static class ShipEstimateMapper
	{
		public static ShipMethod GetSelectedShippingMethod(this ShipEstimate shipEstimate)
		{
			return shipEstimate.ShipMethods.First(method => method.ID == shipEstimate.SelectedShipMethodID);
		}

		public static (Address, Address) GetAddresses(this ShipEstimate estimates, IList<LineItem> allLines)
		{
			var firstItemInShipmentID = estimates.ShipEstimateItems.FirstOrDefault().LineItemID;
			var lineItem = allLines.First(item => item.ID == firstItemInShipmentID);
			return (lineItem.ShipFromAddress, lineItem.ShippingAddress);
		}
	}
}
