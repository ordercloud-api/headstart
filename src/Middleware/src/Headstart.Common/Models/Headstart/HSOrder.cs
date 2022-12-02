using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OrderCloud.SDK;

namespace Headstart.Common.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OrderType
    {
        Standard,
        Quote,
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum SubmittedOrderStatus
    {
        Open,
        Completed,
        Canceled,
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum QuoteStatus
    {
        NeedsBuyerReview,
        NeedsSellerReview,
    }

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

        [JsonConverter(typeof(StringEnumConverter))]
        public CurrencyCode? Currency { get; set; } = null;

        public SubmittedOrderStatus SubmittedOrderStatus { get; set; }

        public string ApprovalNeeded { get; set; }

        public ShippingStatus ShippingStatus { get; set; }

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
    }

    public class OrderDetails
    {
        public HSOrder Order { get; set; }

        public IList<LineItem> LineItems { get; set; }

        public IList<OrderPromotion> Promotions { get; set; }

        public IList<Payment> Payments { get; set; }

        public IList<OrderApproval> Approvals { get; set; }

        public IList<OrderReturn> OrderReturns { get; set; }
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
}
