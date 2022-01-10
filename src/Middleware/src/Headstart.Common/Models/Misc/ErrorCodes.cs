using System.Collections.Generic;
using Headstart.Models.Exceptions;
using ordercloud.integrations.library;
using OrderCloud.Catalyst;

namespace Headstart.Models
{
    public static class ErrorCodes
    {
        public static IDictionary<string, ErrorCode> All { get; } = new Dictionary<string, ErrorCode>
        {
            { "Checkout.MissingShippingSelection", new  ErrorCode<MissingShippingSelectionError>("MissingShippingSelection", 404, "Cannot proceed until all shipping selections have been made.") },
            { "Checkout.InvalidShipFromAddress", new ErrorCode<InvalidShipFromAddressIDError>("InvalidShipFromAddress", 400, "This ShipFromAddressID does not match any products in the order") },
            { "Checkout.MissingProductDimensions", new ErrorCode<MissingProductDimensionsError>("MissingProductDimensions", 400, "Product dimensions are missing for a product") },
            { "ZohoIntegrationError", new ErrorCode<ZohoIntegrationError>("ZohoIntegrationError", 400, "An error occurred in the Zoho Integration process")}
        };

        public static class Checkout
        {
            /// <summary>Cannot proceed until all shipping selections have been made.</summary>
            public static ErrorCode<MissingShippingSelectionError> MissingShippingSelection => All["Checkout.MissingShippingSelection"] as ErrorCode<MissingShippingSelectionError>;
            /// <summary>This ShipFromAddressID does not exist on the order.</summary>
            public static ErrorCode<InvalidShipFromAddressIDError> InvalidShipFromAddress => All["Checkout.InvalidShipFromAddress"] as ErrorCode<InvalidShipFromAddressIDError>;
            /// <summary>Product dimensions are not set for a product on this order.</summary>
            public static ErrorCode<MissingProductDimensionsError> MissingProductDimensions => All["Checkout.MissingProductDimensions"] as ErrorCode<MissingProductDimensionsError>;
        }
    }
}
