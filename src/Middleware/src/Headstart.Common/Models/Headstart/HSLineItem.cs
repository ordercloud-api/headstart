using OrderCloud.SDK;
using System.Collections.Generic;
using Headstart.Common.Models.Headstart.Extended;

namespace Headstart.Common.Models.Headstart
{
	public class HsLineItem : LineItem<LineItemXp, HsLineItemProduct, LineItemVariant, HsAddressBuyer, HsAddressSupplier>
	{
	}

	public class HSPartialLineItem : PartialLineItem<LineItemXp, HsLineItemProduct, LineItemVariant, HsAddressBuyer, HsAddressSupplier>
	{
	}

	public class LineItemXp
	{
		/// <summary>
		/// LineItem.LineTotal value if it was calculated by applying order-level promotions proportionally instead of evenly.
		/// </summary>
		public decimal LineTotalWithProportionalDiscounts { get; set; }

		public Dictionary<LineItemStatus, int> StatusByQuantity { get; set; }

		public List<LineItemClaim> Returns { get; set; } = new List<LineItemClaim>();

		public List<LineItemClaim> Cancellations { get; set; } = new List<LineItemClaim>();

		public string ImageUrl { get; set; } = string.Empty;

		public string PrintArtworkUrl { get; set; } = string.Empty;

		public string ConfigurationId { get; set; } = string.Empty;

		public string DocumentId { get; set; } = string.Empty;

		// Needed for Line Item Detail Report
		public string ShipMethod { get; set; } = string.Empty;

		public string SupplierComments { get; set; } = string.Empty; // xp.Comments is already being used as ship comments for SEB
	}

	public class LineItemClaim
	{
		public string RMANumber { get; set; } = string.Empty;

		public int Quantity { get; set; }

		public string Reason { get; set; } = string.Empty;

		public string Comment { get; set; } = string.Empty;

		public bool IsResolved { get; set; }
	}
}