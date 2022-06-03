using System.Collections.Generic;
using System.Linq;
using Headstart.Common.Models;

namespace Headstart.Common.Extensions
{
    public static class Extensions
    {
        public static bool HasItem<T>(this IList<T> itemList)
        {
            if (itemList == null || itemList.Count == 0)
            {
                return false;
            }

            return true;
        }

        public static bool HasItem<T>(this IReadOnlyList<T> itemList)
        {
            if (itemList == null || itemList.Count == 0)
            {
                return false;
            }

            return true;
        }

        public static bool HasItem<T>(this List<T> itemList)
        {
            if (itemList == null || itemList.Count == 0)
            {
                return false;
            }

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
