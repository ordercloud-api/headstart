using System;
using ordercloud.integrations.library;
using ordercloud.integrations.easypost;
using Headstart.Common.Models.Headstart;
using Headstart.Common.Models.Headstart.Extended;

namespace Headstart.Common.Repositories.Models
{
	public class OrderWithShipments : CosmosObject
	{
		public string PartitionKey { get; set; } = string.Empty;

		public string OrderId { get; set; } = string.Empty;

		public DateTimeOffset? DateSubmitted { get; set; }

		public SubmittedOrderStatus SubmittedOrderStatus { get; set; }

		public ShippingStatus ShippingStatus { get; set; }

		public string LineItemId { get; set; } = string.Empty;

		public string SupplierId { get; set; } = string.Empty;

		public string SupplierName { get; set; } = string.Empty;

		public string ShipRateId { get; set; } = string.Empty;

		public decimal? SupplierShippingCost { get; set; }

		public decimal? BuyerShippingCost { get; set; }

		public decimal? BuyerShippingTax { get; set; }

		public decimal? BuyerShippingTotal { get; set; }

		public decimal? ShippingCostDifference { get; set; }

		public string ProductId { get; set; } = string.Empty;

		public string ProductName { get; set; } = string.Empty;

		public int Quantity { get; set; }

		public int QuantityShipped { get; set; }

		public decimal LineTotal { get; set; }

		public string ShipToCompanyName { get; set; } = string.Empty;

		public string ShipToStreet1 { get; set; } = string.Empty;

		public string ShipToStreet2 { get; set; } = string.Empty;

		public string ShipToCity { get; set; } = string.Empty;

		public string ShipToState { get; set; } = string.Empty;

		public string ShipToZip { get; set; } = string.Empty;

		public string ShipToCountry { get; set; } = string.Empty;

		public decimal? ShipWeight { get; set; }

		public decimal? ShipWidth { get; set; }

		public decimal? ShipHeight { get; set; }

		public decimal? ShipLength { get; set; }

		public SizeTier SizeTier { get; set; }

		public string FromUserFirstName { get; set; } = string.Empty;

		public string FromUserLastName { get; set; } = string.Empty;

		public string LocationId { get; set; } = string.Empty;

		public string BillingNumber { get; set; } = string.Empty;

		public string BrandID { get; set; } = string.Empty;

		public string EstimateCarrier { get; set; } = string.Empty;

		public string EstimateCarrierAccountID { get; set; } = string.Empty;

		public string EstimateMethod { get; set; } = string.Empty;

		public int? EstimateTransitDays { get; set; }

		public string ShipmentId { get; set; } = string.Empty;

		public DateTimeOffset? DateShipped { get; set; }

		public string TrackingNumber { get; set; } = string.Empty;

		public string Service { get; set; } = string.Empty;
	}
}