using System.Collections.Generic;
using Headstart.Models;
using OrderCloud.SDK;

namespace Headstart.Common.Models
{
    public class SuperHSShipment
    {
        public HSShipment Shipment { get; set; }

        public List<ShipmentItem> ShipmentItems { get; set; }
    }

    public class HSShipment : Shipment<ShipmentXp, HSAddressSupplier, HSAddressBuyer>
    {
    }

    public class ShipmentXp
    {
        public string Service { get; set; }

        public string Comment { get; set; }

        public string BuyerID { get; set; }
    }

    public class HSShipmentWithItems : Shipment
    {
        public List<HSShipmentItemWithLineItem> ShipmentItems { get; set; }
    }

    public class HSShipmentItemWithLineItem : ShipmentItem
    {
        public LineItem LineItem { get; set; }
    }
}
