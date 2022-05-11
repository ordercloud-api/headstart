using Headstart.Models;
using Headstart.Models.Extended;
using ordercloud.integrations.easypost;
using ordercloud.integrations.library;
using System;

namespace Headstart.Common.Models
{
    public class OrderWithShipments : CosmosObject
    {
        public string PartitionKey { get; set; }
        public string OrderID { get; set; }
        public DateTimeOffset? DateSubmitted { get; set; }
        public SubmittedOrderStatus SubmittedOrderStatus { get; set; }
        public ShippingStatus ShippingStatus { get; set; }
        public string LineItemID { get; set; }
        public string SupplierID { get; set; }
        public string SupplierName { get; set; }
        public string ShipRateID { get; set; }
        public decimal? SupplierShippingCost { get; set; }
        public decimal? BuyerShippingCost { get; set; }
        public decimal? BuyerShippingTax { get; set; }
        public decimal? BuyerShippingTotal { get; set; }
        public decimal? ShippingCostDifference { get; set; }
        public string ProductID { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public int QuantityShipped { get; set; }
        public decimal LineTotal { get; set; }
        public string ShipToCompanyName { get; set; }
        public string ShipToStreet1 { get; set; }
        public string ShipToStreet2 { get; set; }
        public string ShipToCity { get; set; }
        public string ShipToState { get; set; }
        public string ShipToZip { get; set; }
        public string ShipToCountry { get; set; }
        public decimal? ShipWeight { get; set; }
        public decimal? ShipWidth { get; set; }
        public decimal? ShipHeight { get; set; }
        public decimal? ShipLength { get; set; }
        public SizeTier SizeTier { get; set; }
        public string FromUserFirstName { get; set; }
        public string FromUserLastName { get; set; }
        public string LocationID { get; set; }
        public string BillingNumber { get; set; }
        public string BrandID { get; set; }
        public string EstimateCarrier { get; set; }
        public string EstimateCarrierAccountID { get; set; }
        public string EstimateMethod { get; set; }
        public int? EstimateTransitDays { get; set; }
        public string ShipmentID { get; set; }
        public DateTimeOffset? DateShipped { get; set; }
        public string TrackingNumber { get; set; }
        public string Service { get; set; }
    }
}
