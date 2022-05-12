using Headstart.Models.Extended;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ordercloud.integrations.exchangerates;
using ordercloud.integrations.library;
using OrderCloud.SDK;
using System.Collections.Generic;
using Headstart.Common.Exceptions;
using Headstart.Common.Services.ShippingIntegration.Models;
using Headstart.Models.Headstart;
using System;

namespace Headstart.Models
{
    
    public class HSOrder : Order<OrderXp, HSUser, HSAddressBuyer>
    {
    }

    
    public class OrderXp
    {
        public string ExternalTaxTransactionID { get; set; }
        public List<string> ShipFromAddressIDs { get; set; }
        public List<string> SupplierIDs { get; set; }
        public bool NeedsAttention { get; set; }
        public bool StopShipSync { get; set; }
        public OrderType? OrderType { get; set; } // "Quote" or "Standard"
        public QuoteOrderInfo QuoteOrderInfo { get; set; }
        public ClaimsSummary Returns { get; set; }
        public ClaimsSummary Cancelations { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public CurrencySymbol? Currency { get; set; } = null;
        public SubmittedOrderStatus SubmittedOrderStatus { get; set; }
        public string ApprovalNeeded { get; set; }
        public ShippingStatus ShippingStatus { get; set; }
        public ClaimStatus ClaimStatus { get; set; }
        public string PaymentMethod { get; set; }
        public HSAddressBuyer ShippingAddress { get; set; }
        public List<ShipMethodSupplierView> SelectedShipMethodsSupplierView { get; set; }
        public bool? IsResubmitting { get; set; }
        public bool? HasSellerProducts { get; set; }
        public QuoteStatus QuoteStatus { get; set; }
        public string QuoteSellerContactEmail { get; set; }
        public string QuoteBuyerContactEmail { get; set; }
        public DateTimeOffset? QuoteSubmittedDate { get; set; }
        public string QuoteSupplierID { get; set; }

    }

    
    public class ShipMethodSupplierView 
    {
        public int EstimatedTransitDays { get; set; }
        public string Name { get; set; } // e.g. "Fedex PRIORITY_OVERNIGHT"
        public string ShipFromAddressID { get; set; }
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
        public HSOrder Order { get; set; }
        public IList<LineItem> LineItems { get; set; }
        public IList<OrderPromotion> Promotions { get; set; }
        public IList<Payment> Payments { get; set; }
        public IList<OrderApproval> Approvals { get; set; }
    }

    
    public class ClaimsSummary
    {
        public bool HasClaims { get; set; }
        public bool HasUnresolvedClaims { get; set; }
        public List<ClaimResolutionStatuses> Resolutions { get; set; }
    }

    
    public class ClaimResolutionStatuses
    {
        public string LineItemID { get; set; }
        public string RMANumber { get; set; }
        public bool IsResolved { get; set; }
    }

    
    public class HSSupplierOrderData
    {
        public HSOrderLineItemData SupplierOrder { get; set; }
        public HSOrderLineItemData BuyerOrder { get; set; }
        public IList<OrderPromotion> OrderPromotions { get; set; }
        public HSShipEstimate ShipMethod { get; set; }
    }

    public class HSOrderLineItemData
    {
        public HSOrder Order { get; set; }
        public List<HSLineItem> LineItems { get; set; }
        public List<LineItemMiscReportFields> LineItemsWithMiscFields { get; set; }
        public List<LineItemsWithPurchaseOrderFields> LineItemsWithPurchaseOrderFields { get; set; }
    }

    public class LineItemMiscReportFields
    {
        public string ID { get; set; }
        public decimal? Tax { get; set; }
        public bool LineTaxAvailable { get; set; }
        public string BrandName { get; set; }
        public string SupplierName { get; set; }
    }

    public class LineItemsWithPurchaseOrderFields
    {
        public string ID { get; set; }
        public string OrderID { get; set; }
        public decimal Total { get; set; }
        public decimal Subtotal { get; set; }
        public decimal? UnitPrice { get; set; }
        public string SupplierID { get; set; }
    }

    public class HSOrderSubmitPayload
    {
        public HSOrderSubmitPayloadResponse Response { get; set; }
    }
    
    public class HSOrderSubmitPayloadResponse
    {
        public HSOrder Body { get; set; }
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