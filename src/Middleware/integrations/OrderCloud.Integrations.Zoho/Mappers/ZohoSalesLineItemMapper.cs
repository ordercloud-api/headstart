using System;
using Headstart.Models.Headstart;
using OrderCloud.Integrations.Zoho.Models;

namespace OrderCloud.Integrations.Zoho.Mappers
{
    public static class ZohoSalesLineItemMapper
    {
        public static ZohoLineItem Map(HSLineItem item)
        {
            return new ZohoLineItem()
            {
                item_type = "sales_and_purchases",
                name = $"{item.Variant?.Name ?? item.Product.Name} {item.Variant?.xp?.SpecCombo ?? item.SKU()}".Trim(),
                rate = item.UnitPrice.HasValue ? Math.Round(decimal.ToDouble(item.UnitPrice.Value), 2) : 0,
                quantity = 1,
                description = $"{item.Variant?.Name ?? item.Product.Name} {item.Variant?.xp?.SpecCombo ?? item.SKU()}".Trim(),
                sku = item.SKU(),
                unit = item.Product.xp?.UnitOfMeasure?.Unit,
                avatax_tax_code = item.Product.xp?.Tax.Code,
            };
        }

        public static ZohoLineItem Map(ZohoLineItem zItem, HSLineItem item)
        {
            zItem.item_type = "sales_and_purchases";
            zItem.name = $"{item.Variant?.Name ?? item.Product.Name} {item.Variant?.xp?.SpecCombo ?? item.SKU()}".Trim();
            zItem.description = $"{item.Variant?.Name ?? item.Product.Name} {item.Variant?.xp?.SpecCombo ?? item.SKU()}".Trim();
            zItem.purchase_description = $"{item.Variant?.Name ?? item.Product.Name} {item.Variant?.xp?.SpecCombo ?? item.SKU()}".Trim();
            zItem.rate = item.UnitPrice.HasValue ? Math.Round(decimal.ToDouble(item.UnitPrice.Value), 2) : 0;
            zItem.unit = item.Product.xp?.UnitOfMeasure?.Unit;
            zItem.avatax_tax_code = item.Product.xp?.Tax.Code;
            return zItem;
        }
    }
}
