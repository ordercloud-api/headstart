using Headstart.Common.Services.ShippingIntegration.Models;
using Headstart.Models.Extended;
using Headstart.Models.Headstart;
using System;
using OrderCloud.SDK;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using ordercloud.integrations.exchangerates;

namespace Headstart.Models
{

    public class HSOrder : Order<OrderXp, HSUser, HSAddressBuyer> { }

    public class OrderXp
    {
        public string ExternalTaxTransactionID { get; set; } = string.Empty;

        public List<string> ShipFromAddressIDs { get; set; } = new List<string>();

        public List<string> SupplierIDs { get; set; } = new List<string>();

        public bool NeedsAttention { get; set; }

        public bool StopShipSync { get; set; }

        public OrderType? OrderType { get; set; } // "Quote" or "Standard"

        public QuoteOrderInfo QuoteOrderInfo { get; set; } = new QuoteOrderInfo();

        public ClaimsSummary Returns { get; set; } = new ClaimsSummary();

        public ClaimsSummary Cancelations { get; set; } = new ClaimsSummary();

        [JsonConverter(typeof(StringEnumConverter))]
        public CurrencySymbol? Currency { get; set; }

        public SubmittedOrderStatus SubmittedOrderStatus { get; set; }

        public string ApprovalNeeded { get; set; } = string.Empty;

        public ShippingStatus ShippingStatus { get; set; }

        public ClaimStatus ClaimStatus { get; set; }

        public string PaymentMethod { get; set; } = string.Empty;

        public HSAddressBuyer ShippingAddress { get; set; } = new HSAddressBuyer();

        public List<ShipMethodSupplierView> SelectedShipMethodsSupplierView { get; set; } = new List<ShipMethodSupplierView>();
        public bool? IsResubmitting { get; set; }

        public bool? HasSellerProducts { get; set; }

        public QuoteStatus QuoteStatus { get; set; }

        public string QuoteSellerContactEmail { get; set; } = string.Empty;

        public string QuoteBuyerContactEmail { get; set; } = string.Empty;

        public DateTimeOffset? QuoteSubmittedDate { get; set; }

        public string QuoteSupplierID { get; set; } = string.Empty;
    }


    public class ShipMethodSupplierView
    {
        public int EstimatedTransitDays { get; set; }

        public string Name { get; set; } = string.Empty; // e.g. "Fedex PRIORITY_OVERNIGHT"

        public string ShipFromAddressID { get; set; } = string.Empty;
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
        public HSOrder Order { get; set; } = new HSOrder();

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
        public string LineItemID { get; set; } = string.Empty;

        public string RMANumber { get; set; } = string.Empty;

        public bool IsResolved { get; set; }
    }


    public class HSSupplierOrderData
    {
        public HSOrderLineItemData SupplierOrder { get; set; } = new HSOrderLineItemData();

        public HSOrderLineItemData BuyerOrder { get; set; } = new HSOrderLineItemData();

        public IList<OrderPromotion> OrderPromotions { get; set; } = new List<OrderPromotion>();

        public HSShipEstimate ShipMethod { get; set; } = new HSShipEstimate();
    }

    public class HSOrderLineItemData
    {
        public HSOrder Order { get; set; } = new HSOrder();

        public List<HSLineItem> LineItems { get; set; } = new List<HSLineItem>();

        public List<LineItemMiscReportFields> LineItemsWithMiscFields { get; set; } = new List<LineItemMiscReportFields>();

        public List<LineItemsWithPurchaseOrderFields> LineItemsWithPurchaseOrderFields { get; set; } = new List<LineItemsWithPurchaseOrderFields>();
    }

    public class LineItemMiscReportFields
    {
        public string ID { get; set; } = string.Empty;

        public decimal? Tax { get; set; }

        public bool LineTaxAvailable { get; set; }

        public string BrandName { get; set; } = string.Empty;

        public string SupplierName { get; set; } = string.Empty;
    }

    public class LineItemsWithPurchaseOrderFields
    {
        public string ID { get; set; } = string.Empty;

        public string OrderID { get; set; } = string.Empty;

        public decimal Total { get; set; }

        public decimal Subtotal { get; set; }

        public decimal? UnitPrice { get; set; }

        public string SupplierID { get; set; } = string.Empty;
    }

    public class HSOrderSubmitPayload
    {
        public HSOrderSubmitPayloadResponse Response { get; set; } = new HSOrderSubmitPayloadResponse();
    }

    public class HSOrderSubmitPayloadResponse
    {
        public HSOrder Body { get; set; } = new HSOrder();
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