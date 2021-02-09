using System;
using ordercloud.integrations.library;

namespace ordercloud.integrations.freightpop
{
    public class ShipmentDetails
    {
        public string ShipmentId { get; set; }
        public string QuoteId { get; set; }
        public Carrier Carrier { get; set; }
        public ShipmentRate Rate { get; set; }
        public PaymentTerm PaymentTerm { get; set; }
        public AccountDetails ThirdPartyAccountInfo { get; set; }
        public DateTime ShipDate { get; set; }
        public OrderAddress ShipperAddress { get; set; }
        public OrderAddress ReturnAddress { get; set; }
        public OrderAddress ConsigneeAddress { get; set; }
        public string ITN { get; set; }
        public string Reference1 { get; set; }
        public string Reference2 { get; set; }
        public string Reference3 { get; set; }
        public string Reference4 { get; set; }
        public ShipmentItem[] Items { get; set; }
        public ProductDetail[] ProductDetails { get; set; }
        public AdditionalDetails AdditionalDetails { get; set; }
        public string PickupNumber { get; set; }
        public PickupDetails PickupDetails { get; set; }
        public Document[] Documents { get; set; }
        public string[] TrackingNumbers { get; set; }
        public string[] TrackingURLs { get; set; }
    }

	[SwaggerModel]
    public class ShipmentRate
    {
        public string Id { get; set; }
        public string Currency { get; set; }
        public decimal DiscountedCost { get; set; }
        public decimal ListCost { get; set; }
        public string Service { get; set; }
        public string TransitDays { get; set; }
    }

    public class PickupDetails
    {
        public DateTime ReadyDateTime { get; set; }
        public string[] Emails { get; set; }
        public DateTime CutoffDateTime { get; set; }
    }

    public class Document
    {
        public string Type { get; set; }
        public string Format { get; set; }
        public string Url { get; set; }
        public string Number { get; set; }
    }

    public enum GetShipmentBy
    {
        QuoteId = 1,
        ShipmentId = 2,
        PRO = 3,
        OrderNo = 4,
        Date = 5
    }
}
