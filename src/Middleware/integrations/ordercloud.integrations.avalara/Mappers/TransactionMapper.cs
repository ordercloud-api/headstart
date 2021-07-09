using Avalara.AvaTax.RestClient;
using OrderCloud.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ordercloud.integrations.avalara
{
	public static class TransactionMapper
	{
		public static CreateTransactionModel ToAvalaraTransactionModel(this OrderWorksheet order, string companyCode, DocumentType docType, List<OrderPromotion> promosOnOrder)
		{
			var buyerLocationID = order.Order.BillingAddress.ID;

			var standardLineItems = order.LineItems.Where(li => li.Product.xp.ProductType == "Standard")?.ToList();

			if (promosOnOrder.Any(promo => promo.LineItemLevel))
			{
				order.LineItems = HandleLineItemPromoDiscounting(order, promosOnOrder);
			}

			var standardShipEstimates = order.ShipEstimateResponse?.ShipEstimates;

			var shippingLines = standardShipEstimates.Select(shipment =>
			{
				var (shipFrom, shipTo) = shipment.GetAddresses(order.LineItems);
				var method = shipment.GetSelectedShippingMethod();
				return method.ToLineItemModel(shipFrom, shipTo);
			});

			var hasResaleCert = ((int?) order.Order.BillingAddress.xp?.AvalaraCertificateID != null);
			var exemptionNo = hasResaleCert ? buyerLocationID : null;

			var productLines = standardLineItems.Select(lineItem =>
				 lineItem.ToLineItemModel(lineItem.ShipFromAddress, lineItem.ShippingAddress, exemptionNo));

			return new CreateTransactionModel()
			{
				companyCode = companyCode,
				type = docType,
				customerCode = buyerLocationID,
				date = DateTime.Now,
				discount = GetOrderOnlyTotalDiscount(order, promosOnOrder),
				lines = productLines.Concat(shippingLines).ToList(),
				purchaseOrderNo = order.Order.ID
			};
		}

		private static IList<LineItem> HandleLineItemPromoDiscounting(OrderWorksheet order, List<OrderPromotion> promosOnOrder)
		{
			List<string> allLineItemIDsWithDiscounts = promosOnOrder
				.Where(promo => promo.LineItemLevel)
				.Select(promo => promo.LineItemID)
				.Distinct().ToList(); // X001, X002...

			List<string> allLineItemLevelPromoCodes = promosOnOrder
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
				CalculateDiscountByPromoCode(promoCode, order, promosOnOrder, totalWeightedLineItemDiscounts);
			}

			foreach (string lineItemID in allLineItemIDsWithDiscounts)
			{
				LineItem lineItemToUpdate = order.LineItems.FirstOrDefault(lineItem => lineItem.ID == lineItemID);
				lineItemToUpdate.LineTotal = lineItemToUpdate.LineSubtotal - totalWeightedLineItemDiscounts[lineItemID];
			}

			return order.LineItems;
		}

		private static void CalculateDiscountByPromoCode(string promoCode, OrderWorksheet order, List<OrderPromotion> promosOnOrder, Dictionary<string, decimal> totalWeightedLineItemDiscounts)
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

		private static void HandleWeightedDiscounting(List<string> eligibleLineItemIDs, Dictionary<string, decimal> weightedLineItemDiscountsByPromoCode, OrderWorksheet order, decimal eligibleLineItemSubtotal, decimal totalDiscountedByOrderCloud)
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

		private static LineItemModel ToLineItemModel(this LineItem lineItem, Address shipFrom, Address shipTo, string exemptionNo)
		{
			var line = new LineItemModel()
			{
				amount = lineItem.LineTotal, // Total after line-item level promotions have been applied
				quantity = lineItem.Quantity,
				taxCode = lineItem.Product.xp.Tax.Code,
				itemCode = lineItem.ProductID,
				discounted = true, // Assumption that all products are eligible for order-level promotions
				customerUsageType = null,
				number = lineItem.ID,
				addresses = ToAddressesModel(shipFrom, shipTo)
			};
			var isResaleProduct = (bool)lineItem.Product.xp?.IsResale;
			if (isResaleProduct && exemptionNo != null)
			{
				line.exemptionCode = exemptionNo;
			}
			return line;
		}

		private static LineItemModel ToLineItemModel(this ShipMethod method, Address shipFrom, Address shipTo)
		{
			return new LineItemModel()
			{
				amount = method.Cost,
				taxCode = "FR",
				itemCode = method.Name,
				customerUsageType = null,
				number = method.ID,
				addresses = ToAddressesModel(shipFrom, shipTo)
			};
		}

		private static decimal GetOrderOnlyTotalDiscount(OrderWorksheet order, List<OrderPromotion> promosOnOrder)
		{
			return promosOnOrder
				.Where(promo => promo.LineItemID == null && !promo.LineItemLevel)
				.Select(promo => promo.Amount).Sum();
		}

		private static AddressesModel ToAddressesModel(Address shipFrom, Address shipTo)
		{
			return new AddressesModel()
			{
				shipFrom = shipFrom.ToAddressLocationInfo(),
				shipTo = shipTo.ToAddressLocationInfo(),
			};
		}

		private static AddressLocationInfo ToAddressLocationInfo(this Address address)
		{
			return new AddressLocationInfo()
			{
				line1 = address.Street1,
				line2 = address.Street2,
				city = address.City,
				region = address.State,
				postalCode = address.Zip,
				country = address.Country
			};
		}
	}
}
