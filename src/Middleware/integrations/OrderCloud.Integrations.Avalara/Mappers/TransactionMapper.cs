using System;
using System.Collections.Generic;
using System.Linq;
using Avalara.AvaTax.RestClient;
using Headstart.Common.Models;
using OrderCloud.SDK;

namespace OrderCloud.Integrations.Avalara.Mappers
{
    public static class TransactionMapper
    {
        public static CreateTransactionModel ToAvalaraTransactionModel(this HSOrderWorksheet order, string companyCode, DocumentType docType, List<HSOrderPromotion> promosOnOrder)
        {
            var buyerLocationID = order.Order.BillingAddress.ID;

            var standardLineItems = order.LineItems.Where(li => li.Product.xp.ProductType == ProductType.Standard)?.ToList();

            var standardShipEstimates = order.ShipEstimateResponse?.ShipEstimates;

            var shippingLines = standardShipEstimates.Select(shipment =>
            {
                var (shipFrom, shipTo) = shipment.GetAddresses(order.LineItems);
                return shipment.ToLineItemModel(shipFrom, shipTo);
            });

            var productLines = standardLineItems.Select(lineItem =>
                 lineItem.ToLineItemModel(lineItem.ShipFromAddress, lineItem.ShippingAddress));

            return new CreateTransactionModel()
            {
                companyCode = companyCode,
                type = docType,
                customerCode = buyerLocationID,
                date = DateTime.Now,
                discount = GetOrderOnlyTotalDiscount(promosOnOrder),
                lines = productLines.Concat(shippingLines).ToList(),
                purchaseOrderNo = order.Order.ID,
            };
        }

        private static LineItemModel ToLineItemModel(this LineItem lineItem, Address shipFrom, Address shipTo)
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
                addresses = ToAddressesModel(shipFrom, shipTo),
            };

            return line;
        }

        private static LineItemModel ToLineItemModel(this HSShipEstimate shipEstimate, Address shipFrom, Address shipTo)
        {
            var method = shipEstimate.GetSelectedShippingMethod();
            return new LineItemModel()
            {
                amount = method.Cost,
                taxCode = "FR",
                itemCode = method?.GetServiceName(),
                customerUsageType = null,
                number = shipEstimate.ID,
                addresses = ToAddressesModel(shipFrom, shipTo),
            };
        }

        private static decimal GetOrderOnlyTotalDiscount(List<HSOrderPromotion> promosOnOrder)
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
                country = address.Country,
            };
        }
    }
}
