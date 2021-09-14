using Headstart.Common.Models;
using Headstart.Common.Services.ShippingIntegration.Models;
using OrderCloud.SDK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Headstart.Jobs.Helpers
{
    public static class OrderWithShipmentsMapper
    {
        public static OrderWithShipments Map(HSOrderWorksheet orderWorksheet, Shipment shipment, ShipmentItem shipmentItem)
        {
            var lineItem = orderWorksheet.LineItems.FirstOrDefault(li => li.ID == shipmentItem.LineItemID);

            var shipEstimate = orderWorksheet.ShipEstimateResponse.ShipEstimates
                .FirstOrDefault(estimate => estimate.ShipEstimateItems
                .Any(item => item.LineItemID == shipmentItem.LineItemID));

            var shipMethod = shipEstimate != null ? shipEstimate.ShipMethods.FirstOrDefault(method => method.ID == shipEstimate.SelectedShipMethodID) : null;

            var shippingTax = orderWorksheet.OrderCalculateResponse != null && orderWorksheet.OrderCalculateResponse.xp != null && orderWorksheet.OrderCalculateResponse.xp.TaxResponse.lines != null && shipEstimate != null && orderWorksheet.OrderCalculateResponse.xp.TaxResponse.lines.FirstOrDefault(line => line.lineNumber == shipEstimate.SelectedShipMethodID) != null
                ? orderWorksheet.OrderCalculateResponse.xp.TaxResponse.lines.FirstOrDefault(line => line.lineNumber == shipEstimate.SelectedShipMethodID).tax 
                : null;

            var supplierName = ((IDictionary<string, object>)shipmentItem.Product.xp.Facets).ContainsKey("supplier") ? shipmentItem.Product.xp.Facets?.supplier[0] : null;

            var orderWithShipments = new OrderWithShipments()
            {
                PartitionKey = "PartitionValue",
                OrderID = orderWorksheet.Order.ID,
                DateSubmitted = orderWorksheet.Order.DateSubmitted,
                SubmittedOrderStatus = orderWorksheet.Order.xp.SubmittedOrderStatus,
                ShippingStatus = orderWorksheet.Order.xp.ShippingStatus,
                LineItemID = lineItem.ID,
                SupplierID = lineItem.SupplierID,
                SupplierName = supplierName,
                ShipRateID = shipEstimate != null ? shipEstimate.SelectedShipMethodID : null,
                SupplierShippingCost = shipment.Cost,
                BuyerShippingCost = shipMethod != null ? Math.Round(shipMethod.Cost, 2): (decimal?)null,
                BuyerShippingTax = shippingTax != null ? shippingTax : null,
                BuyerShippingTotal = shipMethod != null ? Math.Round(shipMethod.Cost, 2) + (shippingTax ?? 0M) : (decimal?)null,
                ShippingCostDifference = shipMethod != null ? Math.Round(shipMethod.Cost, 2) - (shipment.Cost ?? 0M) : (decimal?)null,
                ProductID = lineItem.ProductID,
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
                LocationID = orderWorksheet.Order.BillingAddress.xp.LocationID,
                BillingNumber = orderWorksheet.Order.BillingAddress.xp.BillingNumber,
                BrandID = orderWorksheet.Order.FromCompanyID,
                EstimateCarrier = shipMethod != null ? shipMethod.xp.Carrier : null,
                EstimateCarrierAccountID = shipMethod != null ? shipMethod.xp.CarrierAccountID : null,
                EstimateMethod = shipMethod != null ? shipMethod.Name : null,
                EstimateTransitDays = shipMethod != null ? shipMethod.EstimatedTransitDays : (int?)null,
                ShipmentID = shipment.ID,
                DateShipped = shipment.DateShipped,
                TrackingNumber = shipment.TrackingNumber,
                Service = ((IDictionary<string, object>)shipmentItem.xp).ContainsKey("ShipMethod") ? shipmentItem.xp.ShipMethod : null
            };

            return orderWithShipments;
        }
    }
}
