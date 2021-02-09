using Headstart.Models;
using ordercloud.integrations.library;
using OrderCloud.SDK;
using System.Collections.Generic;

namespace Headstart.Common.Services.ShippingIntegration.Models
{
    [SwaggerModel]
    public class SuperHSShipment
    {
        public HSShipment Shipment { get; set; }
        public List<ShipmentItem> ShipmentItems { get; set; }
    }

    // these are in the common namespace so that we can reference the FreightPop model
    [SwaggerModel]
    public class HSShipment : Shipment<ShipmentXp, HSAddressSupplier, HSAddressBuyer>
    {
    }

    [SwaggerModel]
    public class ShipmentXp
    {
        // storing full freightPopShipmentRate for potential reference later
        //public ShipmentRate FreightPopShipmentRate { get; set; }
        public string Service { get; set; }
        public string Comment { get; set; }
    }

    [SwaggerModel]
    public class HSShipmentWithItems : Shipment
    {
        public List<HSShipmentItemWithLineItem> ShipmentItems { get; set; }
    }

    [SwaggerModel]
    public class HSShipmentItemWithLineItem : ShipmentItem
    {
        public LineItem LineItem { get; set; }
    }
}
