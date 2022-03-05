using OrderCloud.SDK;
using Headstart.Models.Extended;
using System.Collections.Generic;

namespace Headstart.Models.Headstart
{

    public class HSLineItem : LineItem<LineItemXp, HSLineItemProduct, LineItemVariant, HSAddressBuyer, HSAddressSupplier> { }

    public class HSPartialLineItem : PartialLineItem<LineItemXp, HSLineItemProduct, LineItemVariant, HSAddressBuyer, HSAddressSupplier> { }

    public class LineItemXp
    {
        /// <summary>
        /// LineItem.LineTotal value if it was calculated by applying order-level promotions proportionally instead of evenly.
        /// </summary>
        public decimal LineTotalWithProportionalDiscounts { get; set; }

        public Dictionary<LineItemStatus, int> StatusByQuantity { get; set; }

        public List<LineItemClaim> Returns { get; set; } = new List<LineItemClaim>();

        public List<LineItemClaim> Cancelations { get; set; } = new List<LineItemClaim>();

        public string ImageUrl { get; set; } = string.Empty;

        // chili specific fields
        public string PrintArtworkURL { get; set; } = string.Empty;

        public string ConfigurationID { get; set; } = string.Empty;

        public string DocumentID { get; set; } = string.Empty;

        // needed for Line Item Detail Report
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