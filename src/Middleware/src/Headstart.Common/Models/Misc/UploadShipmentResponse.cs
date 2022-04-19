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
		public string OrderId { get; set; } = string.Empty;

		public string LineItemId { get; set; } = string.Empty;

		public string QuantityShipped { get; set; } = string.Empty;

		public string ShipmentId { get; set; } = string.Empty;

		public string BuyerId { get; set; } = string.Empty;

		public string Shipper { get; set; } = string.Empty;

		public DateTime? DateShipped { get; set; }

		public DateTime? DateDelivered { get; set; }

		public string TrackingNumber { get; set; } = string.Empty;

		public string Cost { get; set; } = string.Empty;

		public string FromAddressId { get; set; } = string.Empty;

		public string ToAddressId { get; set; } = string.Empty;

		public string Account { get; set; } = string.Empty;

		public string Service { get; set; } = string.Empty;

		public string ShipmentComment { get; set; } = string.Empty;

		public string ShipmentLineItemComment { get; set; } = string.Empty;
	}
}