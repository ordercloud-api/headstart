using System;
using System.Linq;
using OrderCloud.SDK;
using System.Collections.Generic;
using Headstart.Common.Models.Headstart;
using Headstart.Common.Repositories.Models;

namespace Headstart.Jobs.Helpers
{
	public static class OrderWithShipmentsMapper
	{
		public static OrderWithShipments Map(HsOrderWorksheet orderWorksheet, Shipment shipment, ShipmentItem shipmentItem)
		{
			var lineItem = orderWorksheet.LineItems.FirstOrDefault(li => li.ID == shipmentItem.LineItemID);

			var shipEstimate = orderWorksheet.ShipEstimateResponse.ShipEstimates
				.FirstOrDefault(estimate => estimate.ShipEstimateItems
					.Any(item => item.LineItemID == shipmentItem.LineItemID));

			var shipMethod = shipEstimate != null ? shipEstimate.ShipMethods.FirstOrDefault(method => method.ID == shipEstimate.SelectedShipMethodID) : null;

			var shippingTax = shipEstimate != null && orderWorksheet?.OrderCalculateResponse?.xp?.TaxCalculation?.OrderLevelTaxes?.FirstOrDefault(line => line.ShipEstimateID == shipEstimate.ID) != null
				? orderWorksheet.OrderCalculateResponse.xp.TaxCalculation.OrderLevelTaxes.FirstOrDefault(line => line.ShipEstimateID == shipEstimate.ID).Tax 
				: 0;

			var supplierName = ((IDictionary<string, object>)shipmentItem.Product.xp.Facets).ContainsKey("supplier") ? shipmentItem.Product.xp.Facets?.supplier[0] : null;

			var orderWithShipments = new OrderWithShipments()
			{
				PartitionKey = "PartitionValue",
				OrderId = orderWorksheet.Order.ID,
				DateSubmitted = orderWorksheet.Order.DateSubmitted,
				SubmittedOrderStatus = orderWorksheet.Order.xp.SubmittedOrderStatus,
				ShippingStatus = orderWorksheet.Order.xp.ShippingStatus,
				LineItemId = lineItem.ID,
				SupplierId = lineItem.SupplierID,
				SupplierName = supplierName,
				ShipRateId = shipEstimate != null ? shipEstimate.SelectedShipMethodID : null,
				SupplierShippingCost = shipment.Cost,
				BuyerShippingCost = shipMethod != null ? Math.Round(shipMethod.Cost, 2): (decimal?)null,
				BuyerShippingTax = shippingTax,
				BuyerShippingTotal = shipMethod != null ? Math.Round(shipMethod.Cost, 2) + shippingTax : (decimal?)null,
				ShippingCostDifference = shipMethod != null ? Math.Round(shipMethod.Cost, 2) - (shipment.Cost ?? 0M) : (decimal?)null,
				ProductId = lineItem.ProductID,
				ProductName = lineItem.Product.Name,
				Quantity = lineItem.Quantity,
				QuantityShipped = shipmentItem.QuantityShipped,
				LineTotal = lineItem.LineTotal,
				ShipToCompanyName = orderWorksheet.Order.xp.ShippingAddress.CompanyName,
				ShipToStreet1 = orderWorksheet.Order.xp.ShippingAddress.Street1,
				ShipToStreet2 = orderWorksheet.Order.xp.ShippingAddress.Street2,
				ShipToCity = orderWorksheet.Order.xp.ShippingAddress.City,
				ShipToState = orderWorksheet.Order.xp.ShippingAddress.State,
				ShipToZip = orderWorksheet.Order.xp.ShippingAddress.Zip,
				ShipToCountry = orderWorksheet.Order.xp.ShippingAddress.Country,
				ShipWeight = lineItem.Product.ShipWeight,
				ShipWidth = lineItem.Product.ShipWidth,
				ShipHeight = lineItem.Product.ShipHeight,
				ShipLength = lineItem.Product.ShipLength,
				SizeTier = lineItem.Product.xp.SizeTier,
				FromUserFirstName = orderWorksheet.Order.FromUser.FirstName,
				FromUserLastName = orderWorksheet.Order.FromUser.LastName,
				LocationId = orderWorksheet.Order.BillingAddress.xp.LocationId,
				BillingNumber = orderWorksheet.Order.BillingAddress.xp.BillingNumber,
				BrandID = orderWorksheet.Order.FromCompanyID,
				EstimateCarrier = shipMethod != null ? shipMethod.xp.Carrier : null,
				EstimateCarrierAccountID = shipMethod != null ? shipMethod.xp.CarrierAccountId : null,
				EstimateMethod = shipMethod != null ? shipMethod.Name : null,
				EstimateTransitDays = shipMethod != null ? shipMethod.EstimatedTransitDays : (int?)null,
				ShipmentId = shipment.ID,
				DateShipped = shipment.DateShipped,
				TrackingNumber = shipment.TrackingNumber,
				Service = ((IDictionary<string, object>)shipmentItem.xp).ContainsKey("ShipMethod") ? shipmentItem.xp.ShipMethod : null
			};

			return orderWithShipments;
		}
	}
}
