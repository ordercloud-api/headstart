using System;

namespace Headstart.Common.Models.Misc
{
    public class Error
    {
        public string ErrorMessage { get; set; } = string.Empty;

        public string StackTrace { get; set; } = string.Empty;
    }

    public class Shipment
    {
        public string OrderID { get; set; } = string.Empty;

        public string LineItemID { get; set; } = string.Empty;

        public string QuantityShipped { get; set; } = string.Empty;

        public string ShipmentID { get; set; } = string.Empty;

        public string BuyerID { get; set; } = string.Empty;

        public string Shipper { get; set; } = string.Empty;

        public DateTime? DateShipped { get; set; }

        public DateTime? DateDelivered { get; set; }

        public string TrackingNumber { get; set; } = string.Empty;

        public string Cost { get; set; } = string.Empty;

        public string FromAddressID { get; set; } = string.Empty;

        public string ToAddressID { get; set; } = string.Empty;

        public string Account { get; set; } = string.Empty;

        public string Service { get; set; } = string.Empty;

        public string ShipmentComment { get; set; } = string.Empty;

        public string ShipmentLineItemComment { get; set; } = string.Empty;
    }
}