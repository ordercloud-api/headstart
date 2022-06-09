using System;
using System.Collections.Generic;
using System.Linq;
using Headstart.Common.Extensions;
using Headstart.Common.Models;
using OrderCloud.Integrations.Zoho.Models;
using OrderCloud.SDK;

namespace OrderCloud.Integrations.Zoho.Mappers
{
    public static class ZohoSalesOrderMapper
    {
        public static ZohoSalesOrder Map(HSOrderWorksheet worksheet, List<ZohoLineItem> items, ZohoContact contact, IList<OrderPromotion> promotions, ZohoSalesOrder zOrder)
        {
            zOrder.reference_number = worksheet.Order.ID;
            zOrder.salesorder_number = worksheet.Order.ID;
            zOrder.date = worksheet.Order.DateSubmitted?.ToString("yyyy-MM-dd");
            zOrder.is_discount_before_tax = true;
            zOrder.discount = decimal.ToDouble(promotions.Sum(p => p.Amount));
            zOrder.discount_type = "entity_level";
            zOrder.line_items = worksheet.LineItems.Select(item => new ZohoLineItem
            {
                item_id = items.First(i => i.sku == item.SKU()).item_id,
                quantity = item.Quantity,
                rate = Math.Round((double)(item.UnitPrice ?? 0), 2),
                avatax_tax_code = item.Product.xp.Tax.Code,

                // discount = decimal.ToDouble(promotions.Where(p => p.LineItemLevel == true && p.LineItemID == line_item.ID).Sum(p => p.Amount)),
            }).ToList();
            zOrder.tax_total = decimal.ToDouble(worksheet.Order.TaxCost);
            zOrder.customer_name = contact.contact_name;
            zOrder.sub_total = decimal.ToDouble(worksheet.Order.Subtotal);
            zOrder.total = decimal.ToDouble(worksheet.Order.Total);
            zOrder.customer_id = contact.contact_id;
            zOrder.currency_code = contact.currency_code;
            zOrder.currency_symbol = contact.currency_symbol;
            zOrder.notes = promotions.Any()
                ? $"Promotions applied: {promotions.DistinctBy(p => p.Code).Select(p => p.Code).JoinString(" - ", p => p)}"
                : null;

            // adding shipping as a line item
            foreach (var shipment in worksheet.ShipEstimateResponse.ShipEstimates)
            {
                var method = shipment.ShipMethods.FirstOrDefault(s => s.ID == shipment.SelectedShipMethodID);
                zOrder.line_items.Add(new ZohoLineItem
                {
                    item_id = items.First(i => i.sku == method?.ShippingSku()).item_id,
                    quantity = 1,
                    rate = Math.Round((double)(method?.Cost ?? 0), 2),
                    avatax_tax_code = "FR",
                });
            }

            return zOrder;
        }

        public static ZohoSalesOrder Map(HSOrderWorksheet worksheet, List<ZohoLineItem> items, ZohoContact contact, IList<OrderPromotion> promotions)
        {
            return Map(worksheet, items, contact, promotions, new ZohoSalesOrder());
        }
    }
}
