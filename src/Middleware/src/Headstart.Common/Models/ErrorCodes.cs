using System.Collections.Generic;
using System.Net;
using Headstart.Models.Exceptions;
using OrderCloud.Catalyst;

namespace Headstart.Common.Models
{
    public static class ErrorCodes
    {
        public static IDictionary<string, ErrorCode> All { get; } = new Dictionary<string, ErrorCode>
        {
            { "Checkout.MissingShippingSelection", new ErrorCode<MissingShippingSelectionError>("MissingShippingSelection", HttpStatusCode.BadRequest, "Cannot proceed until all shipping selections have been made.") },
            { "Checkout.InvalidShipFromAddress", new ErrorCode<InvalidShipFromAddressIDError>("InvalidShipFromAddress", HttpStatusCode.BadRequest, "This ShipFromAddressID does not match any products in the order") },
            { "Checkout.MissingProductDimensions", new ErrorCode<MissingProductDimensionsError>("MissingProductDimensions", HttpStatusCode.BadRequest, "Product dimensions are missing for a product") },
            { "NotFound", new ErrorCode("Not Found", "Resource requested was not found", HttpStatusCode.NotFound) },
            { "Required", new ErrorCode("Required", "Field is required") },
            { "WriteFailure", new ErrorCode("Write Failure", "Failed to create record") },
            { "UnrecognizedType", new ErrorCode("UnrecognizedType", "Unrecognized type") },
            { "Blob.ConnectionString", new ErrorCode("InvalidConnectionString", "Invalid Connection String", HttpStatusCode.NotFound) },
            { "Blob.Container", new ErrorCode("InvalidContainerString", "Invalid Container", HttpStatusCode.NotFound) },
            { "Webhook.MissingHeader", new ErrorCode("MissingWebhookHeader", "Invalid Header", HttpStatusCode.Unauthorized) },
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

        public static partial class Auth
        {
            /// <summary>User does not have role(s) required to perform this action.</summary>
            public static readonly ErrorCode<InvalidHeaderError> InvalidHeader = All["Webhook.MissingHeader"] as ErrorCode<InvalidHeaderError>;
        }
    }
}
