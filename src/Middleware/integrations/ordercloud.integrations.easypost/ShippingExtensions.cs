using System;
using System.Collections.Generic;
using System.Text;
using ordercloud.integrations.library;
using OrderCloud.SDK;

namespace ordercloud.integrations.easypost
{
    public static class ShippingExtensions
    {
        public static double ValueOrDefault(this decimal? value, double defaultValue)
        {
            return value.IsNullOrZero() ? defaultValue : (double)value;
        }

        public static double ShipWeightOrDefault(this LineItem li, double defaultValue)
        {
            if (li.Variant?.ShipWeight != null)
                return (double)li.Variant.ShipWeight;
            return li.Product.ShipWeight.IsNullOrZero() ? defaultValue : (double)li.Product.ShipWeight;
        }

        public static double ShipLengthOrDefault(this LineItem li, double defaultValue)
        {
            if (li.Variant?.ShipLength != null)
                return (double)li.Variant.ShipLength;
            return li.Product.ShipLength.IsNullOrZero() ? defaultValue : (double)li.Product.ShipLength;
        }
        public static double ShipHeightOrDefault(this LineItem li, double defaultValue)
        {
            if (li.Variant?.ShipHeight != null)
                return (double)li.Variant.ShipHeight;
            return li.Product.ShipHeight.IsNullOrZero() ? defaultValue : (double)li.Product.ShipHeight;
        }
        public static double ShipWidthOrDefault(this LineItem li, double defaultValue)
        {
            if (li.Variant?.ShipWidth != null)
                return (double)li.Variant.ShipWidth;
            return li.Product.ShipWidth.IsNullOrZero() ? defaultValue : (double)li.Product.ShipWidth;
        }
    }
}
