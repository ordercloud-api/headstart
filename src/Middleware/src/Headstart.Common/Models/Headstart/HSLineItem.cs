using ordercloud.integrations.library;
using OrderCloud.SDK;
using Headstart.Models.Extended;
using System.Collections.Generic;

namespace Headstart.Models.Headstart
{
    [SwaggerModel]
	public class HSLineItem : LineItem<LineItemXp, HSLineItemProduct, LineItemVariant, HSAddressBuyer, HSAddressSupplier> { }

    [SwaggerModel]
	public class LineItemXp
    {
        public Dictionary<LineItemStatus, int> StatusByQuantity { get; set; }
        public List<LineItemClaim> Returns { get; set; }
        public List<LineItemClaim> Cancelations { get; set; }
        public string ImageUrl { get; set; }

        // chili specific fields
        public string PrintArtworkURL { get; set; }
        public string ConfigurationID { get; set; }
        public string DocumentID { get; set; }

        // kit specific fields
        public string KitProductImageUrl { get; set; }
        public string KitProductID { get; set; }
        public string KitProductName { get; set; }

        // needed for Line Item Detail Report
        public string ShipMethod { get; set; }
        public string SupplierComments { get; set; } // xp.Comments is already being used as ship comments for SEB
    }

    [SwaggerModel]
    public class LineItemClaim
    {
        public string RMANumber { get; set; }
        public int Quantity { get; set; }
        public string Reason { get; set; }
        public string Comment { get; set; }
        public bool IsResolved { get; set; }
    }
}
