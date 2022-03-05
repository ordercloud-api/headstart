using OrderCloud.SDK;
using Headstart.Models;
using System.Collections.Generic;

namespace Headstart.Common.Services.ShippingIntegration.Models
{

    public class SuperHSShipment
    {
        public HSShipment Shipment { get; set; }
        public List<ShipmentItem> ShipmentItems { get; set; }
    }

    // These are in the common namespace so that we can reference the FreightPop model
    public class HSShipment : Shipment<ShipmentXp, HSAddressSupplier, HSAddressBuyer> { }
    
    public class ShipmentXp
    {        
        public string Service { get; set; } = string.Empty;

        public string Comment { get; set; } = string.Empty;

        public string BuyerID { get; set; } = string.Empty;
    }


    public class HSShipmentWithItems : Shipment
    {
        public List<HSShipmentItemWithLineItem> ShipmentItems { get; set; } = new List<HSShipmentItemWithLineItem>();
    }


    public class HSShipmentItemWithLineItem : ShipmentItem
    {
        public LineItem LineItem { get; set; } = new LineItem();
    }
}