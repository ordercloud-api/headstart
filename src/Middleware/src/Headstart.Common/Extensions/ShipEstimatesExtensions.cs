using System.Linq;
using OrderCloud.SDK;
using System.Threading.Tasks;
using System.Collections.Generic;
using Headstart.Common.Constants;
using Headstart.Common.Models.Headstart;
using ordercloud.integrations.exchangerates;

namespace Headstart.Common.Extensions
{
	public static class ShipEstimatesExtensions
	{
		public static IList<HsShipEstimate> CheckForEmptyRates(this IList<HsShipEstimate> estimates, decimal noRatesCost, int noRatesTransitDays)
		{
			// If there are no rates for a set of line items then return a mocked response so user can check out
			// this order will additionally get marked as needing attention
			foreach (var shipEstimate in estimates)
			{
				if (!shipEstimate.ShipMethods.Any())
				{
					shipEstimate.ShipMethods = new List<HsShipMethod>()
					{
						new HsShipMethod
						{
							ID = ShippingConstants.NoRatesId,
							Name = @"No shipping rates",
							Cost = noRatesCost,
							EstimatedTransitDays = noRatesTransitDays,
							xp = new ShipMethodXp
							{
								OriginalCost = noRatesCost
							}
						}
					};
				}
			}
			return estimates;
		}

		public static async Task<IList<HsShipEstimate>> ApplyShippingLogic(this IList<HsShipEstimate> shipEstimates, HsOrderWorksheet orderWorksheet, IOrderCloudClient _oc, int freeShippingTransitDays)
		{
			var updatedEstimates = new List<HsShipEstimate>();
			var supplierIds = orderWorksheet.LineItems.Select(li => li.SupplierID);
			var suppliers = await _oc.Suppliers.ListAsync<HsSupplier>(filters: $@"ID={string.Join("|", supplierIds)}");

			foreach (var shipEstimate in shipEstimates)
			{
				var supplierId = orderWorksheet.LineItems.FirstOrDefault(li => li.ID == shipEstimate.ShipEstimateItems.FirstOrDefault()?.LineItemID)?.SupplierID;
				var supplier = suppliers.Items.FirstOrDefault(s => s.ID == supplierId);
				var supplierLineItems = orderWorksheet.GetBuyerLineItemsBySupplierId(supplier?.Id);
				var supplierSubTotal = supplierLineItems?.Select(li => li.LineSubtotal).Sum();

				// TODO: Still waiting on decision makers to decide if we want Shipping Cost Schedules in HeadStart
				//var validCostBreaks = supplier?.xp?.ShippingCostSchedule?.CostBreaks?.Where(costBreak => costBreak.OrderSubTotal < supplierSubTotal);
				// Update Free Shipping Rates
				if (shipEstimate.ID.StartsWith(ShippingConstants.FreeShippingId))
				{
					foreach (var method in shipEstimate.ShipMethods)
					{
						method.ID = shipEstimate.ID;
						method.Cost = 0;
						method.EstimatedTransitDays = freeShippingTransitDays;
						method.xp.FreeShippingApplied = true;
					}
				}

				// TODO: Still waiting on decision makers to decide if we want
				// Shipping Cost Schedules in HeadStart
				//foreach (var method in shipEstimate.ShipMethods)
				//{
				//    // If valid Cost Breaks exist, apply them
				//    if (validCostBreaks != null)
				//    {
				//        // Apply the Supplier Shipping Cost Schedule to Ground Rates only
				//        if (method.Name.Contains("GROUND"))
				//        {
				//            validCostBreaks = validCostBreaks.Where(cb => cb.ValidCountries.Contains(orderWorksheet?.Order?.FromUser?.xp?.Country));
				//            if (validCostBreaks != null && validCostBreaks.Count() > 0)
				//            {
				//                var sortedValidCostBreaks = validCostBreaks.OrderBy(costBreak => costBreak.OrderSubTotal);
				//                method.xp.ShippingCostScheduleOrderSubTotalThreshold = sortedValidCostBreaks.LastOrDefault().OrderSubTotal;
				//                method.xp.ShippingCostScheduleApplied = true;
				//                method.Cost = sortedValidCostBreaks.LastOrDefault().Cost;
				//                if (method.Cost == 0) // If cost = 0, then shipping is considered free
				//                {
				//                    method.xp.FreeShippingApplied = true;
				//                }
				//            }
				//        }
				//    }
				//}
				updatedEstimates.Add(shipEstimate);
			}

			// Filter out any rates that are _not_ Fedex Ground, Fedex 2 day, and Fedex Standard Overnight
			updatedEstimates = shipEstimates.Select(estimate => FilterDownFedexShippingRates(estimate)).ToList();
			return updatedEstimates;
		}

		public static async Task<IList<HsShipEstimate>> ConvertCurrency(this IList<HsShipEstimate> shipEstimates, CurrencySymbol shipperCurrency, CurrencySymbol buyerCurrency, IExchangeRatesCommand _exchangeRates)
		{
			// If the Buyer's currency is USD, do not convert rates.
			if (buyerCurrency == CurrencySymbol.USD) 
			{ 
				return shipEstimates; 
			}

			var rates = (await _exchangeRates.Get(buyerCurrency)).Rates;
			var conversionRate = rates.Find(r => r.Currency == shipperCurrency).Rate;
			return shipEstimates.Select(estimate =>
			{
				estimate.ShipMethods = estimate.ShipMethods.Select(method =>
				{
					method.xp.OriginalCurrency = shipperCurrency;
					method.xp.OrderCurrency = buyerCurrency;
					method.xp.ExchangeRate = conversionRate;
					if (conversionRate != null)
					{
						method.Cost /= (decimal) conversionRate;
					}
					return method;
				}).ToList();
				return estimate;
			}).ToList();
		}

		#region Helper Methods
		private static HsShipEstimate FilterDownFedexShippingRates(HsShipEstimate estimate)
		{
			estimate.ShipMethods = estimate.ShipMethods.Where(method => (method.ID != null && method.ID.Contains("@FREE_SHIPPING")) || method?.ID == ShippingConstants.NoRatesId || method?.xp?.Carrier == @"USPS" || method.Name == @"FEDEX_GROUND" || method.Name == @"FEDEX_2_DAY" || method.Name == @"STANDARD_OVERNIGHT").ToList();
			return estimate;
		}
		#endregion
	}
}