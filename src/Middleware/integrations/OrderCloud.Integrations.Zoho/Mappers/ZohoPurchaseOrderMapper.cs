using System;
using System.Collections.Generic;
using System.Linq;
using Headstart.Models.Headstart;
using OrderCloud.Integrations.Zoho.Models;
using OrderCloud.SDK;

namespace OrderCloud.Integrations.Zoho.Mappers
{
    public static class ZohoPurchaseOrderMapper
    {
        public static ZohoPurchaseOrder Map(
            ZohoSalesOrder salesorder,
            Order order,
            List<ZohoLineItem> items,
            List<HSLineItem> lineitems,
            ZohoAddress delivery_address,
            ZohoContact vendor,
            ZohoPurchaseOrder po)
        {
            po.line_items = items.Select(p => new ZohoLineItem()
            {
                // account_id = p.purchase_account_id,
                item_id = p.item_id,
                description = p.description,
                rate = Math.Round(decimal.ToDouble(lineitems.First(l => l.SKU() == p.sku).UnitPrice.Value), 2),
                quantity = lineitems.FirstOrDefault(li => li.SKU() == p.sku)?.Quantity,
            }).ToList();
            po.salesorder_id = salesorder.salesorder_id;
            po.purchaseorder_number = order.ID;
            po.reference_number = salesorder.reference_number;
            po.sub_total = decimal.ToDouble(order.Subtotal);
            po.tax_total = decimal.ToDouble(order.TaxCost);
            po.total = decimal.ToDouble(order.Total);
            po.vendor_id = vendor.contact_id;
            po.delivery_customer_id = salesorder.customer_id;
            return po;
        }

        public static ZohoPurchaseOrder Map(
            ZohoSalesOrder salesorder,
            Order order,
            List<ZohoLineItem> items,
            List<HSLineItem> lineitems,
            ZohoAddress delivery_address,
            ZohoContact vendor)
        {
            return Map(salesorder, order, items, lineitems, delivery_address, vendor, new ZohoPurchaseOrder());
        }
    }
}
