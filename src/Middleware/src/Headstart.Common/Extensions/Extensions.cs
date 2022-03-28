using System.Linq;
using System.Collections.Generic;
using Headstart.Common.Models.Headstart;

namespace Headstart.Common.Extensions
{
	public static class Extensions
	{
		public static bool HasItem<t>(this IList<t> itemList)
		{
			if (itemList == null || itemList.Count == 0)
			{ 
				return false; 
			}
			return true;
		}

		public static bool HasItem<t>(this IReadOnlyList<t> itemList)
		{
			if (itemList == null || itemList.Count == 0)
			{ 
				return false; 
			}
			return true;
		}

		public static bool HasItem<t>(this List<t> itemList)
		{
			if (itemList == null || itemList.Count == 0)
			{ 
				return false; 
			}
			return true;
		}

		public static HsShipEstimate GetMatchingShipEstimate(this HsOrderWorksheet buyerWorksheet, string shipFromAddressID)
		{
			return buyerWorksheet?.ShipEstimateResponse?.ShipEstimates?.FirstOrDefault(e => e.xp.ShipFromAddressID == shipFromAddressID);
		}

		public static IEnumerable<HsLineItem> GetBuyerLineItemsBySupplierId(this HsOrderWorksheet buyerWorksheet, string supplierId)
		{
			return (IEnumerable<HsLineItem>) buyerWorksheet?.LineItems?.Where(li => li.SupplierID == supplierId);
		}

		public static IEnumerable<HsLineItem> GetLineItemsByProductType(this HsOrderWorksheet buyerWorksheet, ProductType type)
		{
			return (IEnumerable<HsLineItem>) buyerWorksheet?.LineItems.Where(li => li.Product.xp.ProductType == type);
		}
	}
}