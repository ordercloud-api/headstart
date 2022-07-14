using System.Collections.Generic;
using System.Linq;
using Headstart.Common.Models;
using OrderCloud.SDK;

namespace OrderCloud.Integrations.Avalara.Mappers
{
    public static class ShipEstimateMapper
    {
        public static HSShipMethod GetSelectedShippingMethod(this HSShipEstimate shipEstimate)
        {
            return shipEstimate.ShipMethods.First(method => method.ID == shipEstimate.SelectedShipMethodID);
        }

        public static (Address, Address) GetAddresses(this ShipEstimate estimates, IList<HSLineItem> allLines)
        {
            var firstItemInShipmentID = estimates.ShipEstimateItems.FirstOrDefault().LineItemID;
            var lineItem = allLines.First(item => item.ID == firstItemInShipmentID);
            return (lineItem.ShipFromAddress, lineItem.ShippingAddress);
        }
    }
}
