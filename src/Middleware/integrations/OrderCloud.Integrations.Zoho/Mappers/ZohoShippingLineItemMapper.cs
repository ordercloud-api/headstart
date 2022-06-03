using System;
using Headstart.Common.Models;
using OrderCloud.Integrations.Zoho.Models;

namespace OrderCloud.Integrations.Zoho.Mappers
{
    public static class ZohoShippingLineItemMapper
    {
        public static ZohoLineItem Map(ZohoLineItem item, HSShipMethod method)
        {
            item.item_id = item.item_id;
            item.item_type = "sales_and_purchases";
            item.name = $"Shipping: {method.Name}";
            item.rate = Math.Round(decimal.ToDouble(method.Cost), 2);
            item.description = $"{method.Name} - {method.EstimatedTransitDays} days transit";
            item.sku = item.sku;
            item.quantity = 1;
            item.unit = "each";
            item.purchase_description = $"{method.Name} - {method.EstimatedTransitDays} days transit";
            item.avatax_tax_code = "FR";

            return item;
        }

        public static ZohoLineItem Map(HSShipMethod method)
        {
            var item = new ZohoLineItem()
            {
                item_type = "sales_and_purchases",
                sku = method.ShippingSku(),
                rate = Math.Round(decimal.ToDouble(method.Cost), 2),
                description = $"{method.Name} - {method.EstimatedTransitDays} days transit",
                name = $"Shipping: {method.Name}",
                quantity = 1,
                unit = "each",
                purchase_description = $"{method.Name} - {method.EstimatedTransitDays} days transit",
                avatax_tax_code = "FR",
            };

            return item;
        }
    }
}
