using System;
using System.Linq;
using OrderCloud.SDK;
using OrderCloud.Catalyst;
using System.Threading.Tasks;
using System.Collections.Generic;
using Headstart.Common.Models.Headstart;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Foundation.SitecoreExtensions.MVC.Extensions;
using SitecoreExtensions = Sitecore.Foundation.SitecoreExtensions.Extensions;

namespace Headstart.Common.Services
{
	public interface IDiscountDistributionService
	{
		Task SetLineItemProportionalDiscount(HsOrderWorksheet order, List<OrderPromotion> promotions);
	}

	public class DiscountDistributionService : IDiscountDistributionService
	{
		private readonly IOrderCloudClient _oc;
		private readonly ConfigSettings _configSettings = ConfigSettings.Instance;

		public DiscountDistributionService(IOrderCloudClient oc)
		{
			try
			{
				_oc = oc;
			}
			catch (Exception ex)
			{
				LoggingNotifications.LogApiResponseMessages(_configSettings.AppLogFileKey, SitecoreExtensions.Helpers.GetMethodName(), "",
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace);
			}
		}

		/// <summary>
		/// Promotion discounts are distributed evenly accross line items. Proportional distribution is required for accurate partial returns.
		/// To solve this, set lineItem.xp.LineTotalWithProportionalDiscounts on all LineItems.
		/// </summary>
		public async Task SetLineItemProportionalDiscount(HsOrderWorksheet order, List<OrderPromotion> promotions)
		{
			try
			{
				var allLineItemIDsWithDiscounts = promotions.Where(promo => promo.LineItemLevel).Select(promo => promo.LineItemID)
					.Distinct().ToList(); // X001, X002...
				var allLineItemLevelPromoCodes = promotions.Where(promo => promo.LineItemLevel).Select(promo => promo.Code)
					.Distinct().ToList(); // 25OFF, SAVE15...
				var totalWeightedLineItemDiscounts = new Dictionary<string, decimal>();
				foreach (var lineItemID in allLineItemIDsWithDiscounts)
				{
					// Initialize discounts at 0.00
					totalWeightedLineItemDiscounts.Add(lineItemID, 0M);
				}

				// Calculate discounts one promo code at a time
				foreach (var promoCode in allLineItemLevelPromoCodes)
				{
					CalculateDiscountByPromoCode(promoCode, order, promotions, totalWeightedLineItemDiscounts);
				}

				foreach (var lineItemID in allLineItemIDsWithDiscounts)
				{
					var lineItemToUpdate = order.LineItems.FirstOrDefault(lineItem => lineItem.ID == lineItemID);
					lineItemToUpdate.LineTotal = lineItemToUpdate.LineSubtotal - totalWeightedLineItemDiscounts[lineItemID];
				}

				await Throttler.RunAsync(order.LineItems, 100, 8, async li =>
				{
					var patch = new HSPartialLineItem() { xp = new LineItemXp() { LineTotalWithProportionalDiscounts = li.LineTotal } };
					await _oc.LineItems.PatchAsync(OrderDirection.All, order.Order.ID, li.ID, patch);
				});
			}
			catch (Exception ex)
			{
				LoggingNotifications.LogApiResponseMessages(_configSettings.AppLogFileKey, SitecoreExtensions.Helpers.GetMethodName(), "",
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace);
			}
		}

		private void CalculateDiscountByPromoCode(string promoCode, HsOrderWorksheet order, List<OrderPromotion> promosOnOrder, Dictionary<string, decimal> totalWeightedLineItemDiscounts)
		{
			try
			{
				// Total discounted from this code for all line items
				var totalDiscountedByOrderCloud = promosOnOrder.Where(promo => promo.Code == promoCode).Select(promo => promo.Amount).Sum();
				// Line items discounted with this code
				var eligibleLineItemIDs = promosOnOrder.Where(promo => promo.Code == promoCode).Select(promo => promo.LineItemID).ToList();
				// Subtotal of all of these line items before applying promo code
				var eligibleLineItemSubtotal = order.LineItems.Where(lineItem => eligibleLineItemIDs.Contains(lineItem.ID)).Select(lineItem => lineItem.LineSubtotal).Sum();
				var weightedLineItemDiscountsByPromoCode = new Dictionary<string, decimal>();
				HandleWeightedDiscounting(eligibleLineItemIDs, weightedLineItemDiscountsByPromoCode, order, eligibleLineItemSubtotal, totalDiscountedByOrderCloud);

				foreach (var lineItemID in eligibleLineItemIDs)
				{
					totalWeightedLineItemDiscounts[lineItemID] += weightedLineItemDiscountsByPromoCode[lineItemID];
				}
			}
			catch (Exception ex)
			{
				LoggingNotifications.LogApiResponseMessages(_configSettings.AppLogFileKey, SitecoreExtensions.Helpers.GetMethodName(), "",
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace);
			}
		}

		private void HandleWeightedDiscounting(List<string> eligibleLineItemIDs, Dictionary<string, decimal> weightedLineItemDiscountsByPromoCode, HsOrderWorksheet order, decimal eligibleLineItemSubtotal, decimal totalDiscountedByOrderCloud)
		{
			try
			{
				foreach (var lineItemID in eligibleLineItemIDs)
				{
					weightedLineItemDiscountsByPromoCode.Add(lineItemID, 0M); // Initialize discount for this promo code at 0.00
					LineItem lineItem = order.LineItems.FirstOrDefault(lineItem => lineItem.ID == lineItemID);
					// Determine amount of promo code discount to apply to this line item and round
					var lineItemRateOfSubtotal = lineItem.LineSubtotal / eligibleLineItemSubtotal;
					var weightedDiscount = Math.Round(lineItemRateOfSubtotal * totalDiscountedByOrderCloud, 2);
					weightedLineItemDiscountsByPromoCode[lineItemID] += weightedDiscount;
				}

				var totalWeightedDiscountApplied = weightedLineItemDiscountsByPromoCode.Sum(discount => discount.Value);
				if (totalDiscountedByOrderCloud != totalWeightedDiscountApplied)
				{
					// If a small discrepancy occurs due to rounding, resolve it by adding/subtracting the difference to the item that was discounted the most
					var difference = totalDiscountedByOrderCloud - totalWeightedDiscountApplied;
					var lineItemIDToApplyDiscountDifference = weightedLineItemDiscountsByPromoCode.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
					weightedLineItemDiscountsByPromoCode[lineItemIDToApplyDiscountDifference] += difference;
				}
			}
			catch (Exception ex)
			{
				LoggingNotifications.LogApiResponseMessages(_configSettings.AppLogFileKey, SitecoreExtensions.Helpers.GetMethodName(), "",
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace);
			}
		}
	}
}