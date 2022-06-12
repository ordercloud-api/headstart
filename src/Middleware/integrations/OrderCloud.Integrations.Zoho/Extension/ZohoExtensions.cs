using System;
using Headstart.Common.Extensions;
using Headstart.Common.Models;

namespace OrderCloud.Integrations.Zoho
{
    public static class ZohoExtensions
    {
        public const string ShippingSuffix = "Shipping (41000)";

        public static string SKU(this HSLineItem item)
        {
            return item.Product == null ? string.Empty : $"{item.Product.ID}-{item.Variant?.ID}".TrimEnd("-");
        }

        public static string ShippingSku(this HSShipMethod method)
        {
            return $"{method?.Name} {ShippingSuffix}";
        }
    }
}
