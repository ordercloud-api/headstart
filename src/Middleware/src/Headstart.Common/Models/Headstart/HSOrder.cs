using System;
using OrderCloud.SDK;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using Headstart.Common.Models.Base;
using ordercloud.integrations.exchangerates;
using Headstart.Common.Models.Headstart.Extended;

namespace Headstart.Common.Models.Headstart
{
	public class HsOrder : Order<OrderXp, HsUser, HsAddressBuyer>
	{
	}

	public class OrderXp
	{
		public string ExternalTaxTransactionId { get; set; } = string.Empty;

		public List<string> ShipFromAddressIds { get; set; } = new List<string>();

		public List<string> SupplierIds { get; set; } = new List<string>();

		public bool NeedsAttention { get; set; }

		public bool StopShipSync { get; set; }

		public OrderType? OrderType { get; set; } // "Quote" or "Standard"

		public QuoteOrderInfo QuoteOrderInfo { get; set; } = new QuoteOrderInfo();

		public ClaimsSummary Returns { get; set; } = new ClaimsSummary();

		public ClaimsSummary Cancellations { get; set; } = new ClaimsSummary();

		[JsonConverter(typeof(StringEnumConverter))]
		public CurrencySymbol? Currency { get; set; }

		public SubmittedOrderStatus SubmittedOrderStatus { get; set; } = new SubmittedOrderStatus();

		public string ApprovalNeeded { get; set; } = string.Empty;

		public ShippingStatus ShippingStatus { get; set; } = new ShippingStatus();

		public ClaimStatus ClaimStatus { get; set; } = new ClaimStatus();

		public string PaymentMethod { get; set; } = string.Empty;

		public HsAddressBuyer ShippingAddress { get; set; } = new HsAddressBuyer();

		public List<ShipMethodSupplierView> SelectedShipMethodsSupplierView { get; set; } = new List<ShipMethodSupplierView>();

		public bool? IsResubmitting { get; set; }

		public bool? HasSellerProducts { get; set; }

		public QuoteStatus QuoteStatus { get; set; }

		public string QuoteSellerContactEmail { get; set; } = string.Empty;

		public string QuoteBuyerContactEmail { get; set; } = string.Empty;

		public DateTimeOffset? QuoteSubmittedDate { get; set; }

		public string QuoteSupplierId { get; set; } = string.Empty;
	}


	public class ShipMethodSupplierView
	{
		public int EstimatedTransitDays { get; set; }

		public string Name { get; set; } = string.Empty; // e.g. "Fedex PRIORITY_OVERNIGHT"

		public string ShipFromAddressId { get; set; } = string.Empty;
		// Do not include buyer's cost. That is none of the supplier's beeswax 
	}

	[JsonConverter(typeof(StringEnumConverter))]
	public enum OrderType
	{
		Standard,
		Quote
	}

	public class OrderDetails
	{
		public HsOrder Order { get; set; } = new HsOrder();

		public IList<LineItem> LineItems { get; set; } = new List<LineItem>();

		public IList<OrderPromotion> Promotions { get; set; } = new List<OrderPromotion>();

		public IList<Payment> Payments { get; set; } = new List<Payment>();

		public IList<OrderApproval> Approvals { get; set; } = new List<OrderApproval>();
	}


	public class ClaimsSummary
	{
		public bool HasClaims { get; set; }

		public bool HasUnresolvedClaims { get; set; }

		public List<ClaimResolutionStatuses> Resolutions { get; set; } = new List<ClaimResolutionStatuses>();
	}


	public class ClaimResolutionStatuses
	{
		public string LineItemId { get; set; } = string.Empty;

		public string RMANumber { get; set; } = string.Empty;

		public bool IsResolved { get; set; }
	}


	public class HsSupplierOrderData
	{
		public HsOrderLineItemData SupplierOrder { get; set; } = new HsOrderLineItemData();

		public HsOrderLineItemData BuyerOrder { get; set; } = new HsOrderLineItemData();

		public IList<OrderPromotion> OrderPromotions { get; set; } = new List<OrderPromotion>();

		public HsShipEstimate ShipMethod { get; set; } = new HsShipEstimate();
	}

	public class HsOrderLineItemData
	{
		public HsOrder Order { get; set; } = new HsOrder();

		public List<HsLineItem> LineItems { get; set; } = new List<HsLineItem>();

		public List<LineItemMiscReportFields> LineItemsWithMiscFields { get; set; } = new List<LineItemMiscReportFields>();

		public List<LineItemsWithPurchaseOrderFields> LineItemsWithPurchaseOrderFields { get; set; } = new List<LineItemsWithPurchaseOrderFields>();
	}

	public class LineItemMiscReportFields : HsBaseObject
	{
		public decimal? Tax { get; set; }

		public bool LineTaxAvailable { get; set; }

		public string BrandName { get; set; } = string.Empty;

		public string SupplierName { get; set; } = string.Empty;
	}

	public class LineItemsWithPurchaseOrderFields : HsBaseObject
	{
		public string OrderId { get; set; } = string.Empty;

		public decimal Total { get; set; }

		public decimal Subtotal { get; set; }

		public decimal? UnitPrice { get; set; }

		public string SupplierId { get; set; } = string.Empty;
	}

	public class HsOrderSubmitPayload
	{
		public HsOrderSubmitPayloadResponse Response { get; set; } = new HsOrderSubmitPayloadResponse();
	}

	public class HsOrderSubmitPayloadResponse
	{
		public HsOrder Body { get; set; } = new HsOrder();
	}

	[JsonConverter(typeof(StringEnumConverter))]
	public enum SubmittedOrderStatus
	{
		Open,
		Completed,
		Canceled
	}

	[JsonConverter(typeof(StringEnumConverter))]
	public enum QuoteStatus
	{
		NeedsBuyerReview,
		NeedsSellerReview
	}
}