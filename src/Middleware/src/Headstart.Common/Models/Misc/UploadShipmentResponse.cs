using System;
using System.Collections.Generic;

namespace Headstart.Common.Models.Misc
{
    public class Error
    {
        public string ErrorMessage { get; set; }
        public string StackTrace { get; set; }
    }

    public class Shipment
    {
        public string OrderID { get; set; }
        public string LineItemID { get; set; }
        public string QuantityShipped { get; set; }
        public string ShipmentID { get; set; }
        public string BuyerID { get; set; }
        public string Shipper { get; set; }
        public DateTime? DateShipped { get; set; }
        public DateTime? DateDelivered { get; set; }
        public string TrackingNumber { get; set; }
        public string Cost { get; set; }
        public string FromAddressID { get; set; }
        public string ToAddressID { get; set; }
        public string Account { get; set; }
        public string Service { get; set; }
        public string ShipmentComment { get; set; }
        public string ShipmentLineItemComment { get; set; }

    }
}
