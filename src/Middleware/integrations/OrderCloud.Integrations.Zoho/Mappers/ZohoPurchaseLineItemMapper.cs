using System;
using Headstart.Common.Models;
using OrderCloud.Integrations.Zoho.Models;
using OrderCloud.SDK;

namespace OrderCloud.Integrations.Zoho.Mappers
{
    public static class ZohoPurchaseLineItemMapper
    {
        public static ZohoLineItem Map(HSLineItem item, Supplier supplier)
        {
            return Map(new ZohoLineItem(), item, supplier);
        }

        public static ZohoLineItem Map(ZohoLineItem zItem, HSLineItem item, Supplier supplier)
        {
            zItem.purchase_description = $"{item.Product.Name ?? item.Variant?.Name} {item.Variant?.xp?.SpecCombo ?? item.SKU()}".Trim();
            zItem.purchase_rate = item.UnitPrice.HasValue ? Math.Round(decimal.ToDouble(item.UnitPrice.Value), 2) : 0;
            zItem.manufacturer = supplier.Name;
            return zItem;
        }
    }
}
