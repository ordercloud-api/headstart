using Headstart.Common.Services.ShippingIntegration.Models;
using Headstart.Models;
using Headstart.Models.Headstart;
using System.Collections.Generic;
using System.Linq;

namespace Headstart.Common.Extensions
{
    public static class Extensions
    {
        public static bool HasItem<t>(this IList<t> itemList)
        {
            if (itemList == null || itemList.Count == 0)
            { return false; }

            return true;
        }
        public static bool HasItem<t>(this IReadOnlyList<t> itemList)
        {
            if (itemList == null || itemList.Count == 0)
            { return false; }

            return true;
        }
        public static bool HasItem<t>(this List<t> itemList)
        {
            if (itemList == null || itemList.Count == 0)
            { return false; }

            return true;
        }

        public static HSShipEstimate GetMatchingShipEstimate(this HSOrderWorksheet buyerWorksheet, string shipFromAddressID)
        {
            return buyerWorksheet?.ShipEstimateResponse?.ShipEstimates?.FirstOrDefault(e => e.xp.ShipFromAddressID == shipFromAddressID);
        }

        public static IEnumerable<HSLineItem> GetBuyerLineItemsBySupplierID(this HSOrderWorksheet buyerWorksheet, string supplierID)
        {
            return buyerWorksheet?.LineItems?.Where(li => li.SupplierID == supplierID).Select(li => li);
        }

        public static IEnumerable<HSLineItem> GetLineItemsByProductType(this HSOrderWorksheet buyerWorksheet, ProductType type)
        {
            return buyerWorksheet?.LineItems.Where(li => li.Product.xp.ProductType == type);
        }
    }
}
