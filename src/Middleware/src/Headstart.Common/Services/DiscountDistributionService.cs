﻿using Headstart.Common.Services.ShippingIntegration.Models;
using Headstart.Models.Headstart;
using OrderCloud.Catalyst;
using OrderCloud.SDK;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SitecoreExtensions = Sitecore.Foundation.SitecoreExtensions.Extensions;

namespace Headstart.Common.Services
{
	public interface IDiscountDistributionService
	{
		/// <summary>
		/// Promotion discounts are distributed evenly accross line items. Proportional distribution is required for accurate partial returns.
		/// </summary>
		Task SetLineItemProportionalDiscount(HSOrderWorksheet order, List<OrderPromotion> promotions);
	}

	public class DiscountDistributionService : IDiscountDistributionService
	{
		private readonly IOrderCloudClient _oc;
		private readonly AppSettings _settings;

		/// <summary>
		/// The IOC based constructor method for the DiscountDistributionService class object with Dependency Injection
		/// </summary>
		/// <param name="oc"></param>
		/// <param name="settings"></param>
		public DiscountDistributionService(IOrderCloudClient oc, AppSettings settings)
		{
			try
			{
				_settings = settings;
				_oc = oc;
			}
			catch (Exception ex)
			{
				LoggingNotifications.LogApiResponseMessages(_settings.LogSettings, SitecoreExtensions.Helpers.GetMethodName(), "",
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace, ex);
			}
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

		/// <summary>
		/// Private re-usable CalculateDiscountByPromoCode static method
		/// </summary>
		/// <param name="promoCode"></param>
		/// <param name="order"></param>
		/// <param name="promosOnOrder"></param>
		/// <param name="totalWeightedLineItemDiscounts"></param>
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

		/// <summary>
		/// Private re-usable HandleWeightedDiscounting static method
		/// </summary>
		/// <param name="eligibleLineItemIDs"></param>
		/// <param name="weightedLineItemDiscountsByPromoCode"></param>
		/// <param name="order"></param>
		/// <param name="eligibleLineItemSubtotal"></param>
		/// <param name="totalDiscountedByOrderCloud"></param>
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