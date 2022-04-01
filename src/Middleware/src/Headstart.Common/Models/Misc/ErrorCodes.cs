using System.Net;
using OrderCloud.Catalyst;
using System.Collections.Generic;
using Headstart.Common.Exceptions;

namespace Headstart.Common.Models.Misc
{
	public static class ErrorCodes
	{
		public static IDictionary<string, ErrorCode> All { get; } = new Dictionary<string, ErrorCode>
		{
			{
				@"Checkout.MissingShippingSelection", new ErrorCode<MissingShippingSelectionError>(@"MissingShippingSelection", HttpStatusCode.BadRequest, @"Cannot proceed until all shipping selections have been made.")
			},
			{
				@"Checkout.InvalidShipFromAddress", new ErrorCode<InvalidShipFromAddressIdError>(@"InvalidShipFromAddress", HttpStatusCode.BadRequest, @"This ShipFromAddressID does not match any products in the order.")
			},
			{
				@"Checkout.MissingProductDimensions", new ErrorCode<MissingProductDimensionsError>(@"MissingProductDimensions", HttpStatusCode.BadRequest, @"Product dimensions are missing for a product.")
			},
			{
				@"ZohoIntegrationError", new ErrorCode<ZohoIntegrationError>(@"ZohoIntegrationError", HttpStatusCode.BadRequest, @"An error occurred in the Zoho Integration process.")
			}
		};

		public static class Checkout
		{
			/// <summary>Cannot proceed until all shipping selections have been made.</summary>
			public static ErrorCode<MissingShippingSelectionError> MissingShippingSelection => All[@"Checkout.MissingShippingSelection"] as ErrorCode<MissingShippingSelectionError>;
			/// <summary>This ShipFromAddressID does not exist on the order.</summary>
			public static ErrorCode<InvalidShipFromAddressIdError> InvalidShipFromAddress => All[@"Checkout.InvalidShipFromAddress"] as ErrorCode<InvalidShipFromAddressIdError>;
			/// <summary>Product dimensions are not set for a product on this order.</summary>
			public static ErrorCode<MissingProductDimensionsError> MissingProductDimensions => All[@"Checkout.MissingProductDimensions"] as ErrorCode<MissingProductDimensionsError>;
		}
	}
}