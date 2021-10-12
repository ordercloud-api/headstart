using Headstart.Common.Services.ShippingIntegration.Models;
using Headstart.Models.Headstart;
using OrderCloud.Catalyst;
using OrderCloud.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Headstart.Common.Services
{
	public interface IDiscountDistributionService
	{
		/// <summary>
		/// Promotion discounts are distributed evenly accross line items. Proportional distribution is required for accurate partial returns.
		/// </summary>
		Task SetLineItemProportionalDiscount(HSOrderWorksheet order, List<OrderPromotion> promotions);
	}

	/// <summary>
	/// Promotion discounts are distributed evenly accross line items. Proportional distribution is required for accurate partial returns.
	/// </summary>
	public class DiscountDistributionService : IDiscountDistributionService
	{
		private readonly IOrderCloudClient _oc;

		public DiscountDistributionService(IOrderCloudClient oc)
		{
			_oc = oc;
		}

		/// <summary>
		/// Promotion discounts are distributed evenly accross line items. Proportional distribution is required for accurate partial returns.
		/// To solve this, set lineItem.xp.LineTotalWithProportionalDiscounts on all LineItems.
		/// </summary>
		public async Task SetLineItemProportionalDiscount(HSOrderWorksheet order, List<OrderPromotion> promotions)
		{
			List<string> allLineItemIDsWithDiscounts = promotions
				.Where(promo => promo.LineItemLevel)
				.Select(promo => promo.LineItemID)
				.Distinct().ToList(); // X001, X002...

			List<string> allLineItemLevelPromoCodes = promotions
				.Where(promo => promo.LineItemLevel)
				.Select(promo => promo.Code)
				.Distinct().ToList(); // 25OFF, SAVE15...

			Dictionary<string, decimal> totalWeightedLineItemDiscounts = new Dictionary<string, decimal>();

			foreach (string lineItemID in allLineItemIDsWithDiscounts)
			{
				// Initialize discounts at 0.00
				totalWeightedLineItemDiscounts.Add(lineItemID, 0M);
			}

			// Calculate discounts one promo code at a time
			foreach (string promoCode in allLineItemLevelPromoCodes)
			{
				CalculateDiscountByPromoCode(promoCode, order, promotions, totalWeightedLineItemDiscounts);
			}

			foreach (string lineItemID in allLineItemIDsWithDiscounts)
			{
				var lineItemToUpdate = order.LineItems.FirstOrDefault(lineItem => lineItem.ID == lineItemID);
				lineItemToUpdate.LineTotal = lineItemToUpdate.LineSubtotal - totalWeightedLineItemDiscounts[lineItemID];
			}

			await Throttler.RunAsync(order.LineItems, 100, 8, async li => {
				var patch = new HSPartialLineItem() { xp = new LineItemXp() { LineTotalWithProportionalDiscounts = li.LineTotal }};
				await _oc.LineItems.PatchAsync(OrderDirection.All, order.Order.ID, li.ID, patch);
			});
		}

		private static void CalculateDiscountByPromoCode(string promoCode, HSOrderWorksheet order, List<OrderPromotion> promosOnOrder, Dictionary<string, decimal> totalWeightedLineItemDiscounts)
		{
			// Total discounted from this code for all line items
			decimal totalDiscountedByOrderCloud = promosOnOrder
				.Where(promo => promo.Code == promoCode)
				.Select(promo => promo.Amount)
				.Sum();

			// Line items discounted with this code
			List<string> eligibleLineItemIDs = promosOnOrder
				.Where(promo => promo.Code == promoCode)
				.Select(promo => promo.LineItemID)
				.ToList();

			// Subtotal of all of these line items before applying promo code
			decimal eligibleLineItemSubtotal = order.LineItems
				.Where(lineItem => eligibleLineItemIDs.Contains(lineItem.ID))
				.Select(lineItem => lineItem.LineSubtotal)
				.Sum();

			Dictionary<string, decimal> weightedLineItemDiscountsByPromoCode = new Dictionary<string, decimal>();

			HandleWeightedDiscounting(eligibleLineItemIDs, weightedLineItemDiscountsByPromoCode, order, eligibleLineItemSubtotal, totalDiscountedByOrderCloud);

			foreach (string lineItemID in eligibleLineItemIDs)
			{
				totalWeightedLineItemDiscounts[lineItemID] += weightedLineItemDiscountsByPromoCode[lineItemID];
			}
		}

		private static void HandleWeightedDiscounting(List<string> eligibleLineItemIDs, Dictionary<string, decimal> weightedLineItemDiscountsByPromoCode, HSOrderWorksheet order, decimal eligibleLineItemSubtotal, decimal totalDiscountedByOrderCloud)
		{
			foreach (string lineItemID in eligibleLineItemIDs)
			{
				weightedLineItemDiscountsByPromoCode.Add(lineItemID, 0M); // Initialize discount for this promo code at 0.00
				LineItem lineItem = order.LineItems.FirstOrDefault(lineItem => lineItem.ID == lineItemID);

				// Determine amount of promo code discount to apply to this line item and round
				decimal lineItemRateOfSubtotal = lineItem.LineSubtotal / eligibleLineItemSubtotal;
				decimal weightedDiscount = Math.Round(lineItemRateOfSubtotal * totalDiscountedByOrderCloud, 2);
				weightedLineItemDiscountsByPromoCode[lineItemID] += weightedDiscount;
			}

			decimal totalWeightedDiscountApplied = weightedLineItemDiscountsByPromoCode.Sum(discount => discount.Value);
			if (totalDiscountedByOrderCloud != totalWeightedDiscountApplied)
			{
				// If a small discrepancy occurs due to rounding, resolve it by adding/subtracting the difference to the item that was discounted the most
				decimal difference = totalDiscountedByOrderCloud - totalWeightedDiscountApplied;
				string lineItemIDToApplyDiscountDifference = weightedLineItemDiscountsByPromoCode.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
				weightedLineItemDiscountsByPromoCode[lineItemIDToApplyDiscountDifference] += difference;
			}
		}
	}
}
