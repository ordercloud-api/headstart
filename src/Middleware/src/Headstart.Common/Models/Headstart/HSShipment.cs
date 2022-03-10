using OrderCloud.SDK;
using System.Collections.Generic;

namespace Headstart.Common.Models.Headstart
{
	public class SuperHsShipment
	{
		public HsShipment Shipment { get; set; } = new HsShipment();
		public List<ShipmentItem> ShipmentItems { get; set; } = new List<ShipmentItem>();
	}

	// These are in the common namespace so that we can reference the FreightPop model
	public class HsShipment : Shipment<ShipmentXp, HsAddressSupplier, HsAddressBuyer>
	{
	}
    
	public class ShipmentXp
	{        
		public string Service { get; set; } = string.Empty;

		public string Comment { get; set; } = string.Empty;

		public string BuyerId { get; set; } = string.Empty;
	}

	public class HsShipmentWithItems : Shipment
	{
		public List<HsShipmentItemWithLineItem> ShipmentItems { get; set; } = new List<HsShipmentItemWithLineItem>();
	}

	public class HsShipmentItemWithLineItem : ShipmentItem
	{
		public LineItem LineItem { get; set; } = new LineItem();
	}
}