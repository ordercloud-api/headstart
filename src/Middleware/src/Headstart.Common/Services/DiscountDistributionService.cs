using System;
using System.Collections.Generic;
using System.Linq;
using Headstart.Common.Models;
using OrderCloud.SDK;

namespace Headstart.Common.Services
{
    public interface IDiscountDistributionService
    {
        /// <summary>
        /// Promotion discounts are distributed evenly across line items. Proportional distribution are required for accurate partial returns.
        /// </summary>
        List<HSLineItem> GetLineItemsWithProportionalPromoDiscountApplied(HSOrderWorksheet worksheet);
    }

    /// <summary>
    /// Promotion discounts are distributed evenly across line items. Proportional distribution are required for accurate partial returns.
    /// </summary>
    public class DiscountDistributionService : IDiscountDistributionService
    {
        /// <summary>
        /// OrderCloud applies promotional discounts evenly across line items however proportional distribution is required for accurate partial returns.
        /// To solve this, calculate and return the line items with what the actual line item total should be with proportional discounts applied.
        /// </summary>
        public List<HSLineItem> GetLineItemsWithProportionalPromoDiscountApplied(HSOrderWorksheet worksheet)
        {
            var promotions = worksheet.OrderPromotions.ToList();
            if (!promotions.Any(promo => promo.LineItemLevel))
            {
                return worksheet.LineItems.ToList();
            }

            List<string> allLineItemLevelPromoCodes = promotions
                .Where(promo => promo.LineItemLevel)
                .Select(promo => promo.Code)
                .Distinct().ToList(); // 25OFF, SAVE15...

            Dictionary<string, decimal> totalWeightedLineItemDiscounts = new Dictionary<string, decimal>();
            foreach (HSLineItem lineItem in worksheet.LineItems)
            {
                totalWeightedLineItemDiscounts.Add(lineItem.ID, 0M); // initialize all at 0.00
            }

            // Calculate discounts one promo code at a time
            foreach (string promoCode in allLineItemLevelPromoCodes)
            {
                CalculateDiscountByPromoCode(promoCode, worksheet, promotions, totalWeightedLineItemDiscounts);
            }

            foreach (HSLineItem lineItem in worksheet.LineItems)
            {
                if (totalWeightedLineItemDiscounts.TryGetValue(lineItem.ID, out var weightedPromoDiscount))
                {
                    lineItem.LineTotal = lineItem.LineSubtotal - weightedPromoDiscount;
                }
            }

            return worksheet.LineItems.ToList();
        }

        private static void CalculateDiscountByPromoCode(string promoCode, HSOrderWorksheet order, List<HSOrderPromotion> promosOnOrder, Dictionary<string, decimal> totalWeightedLineItemDiscounts)
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
