using Cosmonaut.Attributes;
using Newtonsoft.Json;
using ordercloud.integrations.library;
using System.Collections.Generic;
using Newtonsoft.Json.Converters;
using ordercloud.integrations.library.Cosmos;
using Headstart.Models;

namespace Headstart.Common.Models
{
    [CosmosCollection("reporttemplates")]
    public class ReportTemplate : CosmosObject
    {
        [CosmosInteropID]
        public string TemplateID { get; set; }
        [CosmosPartitionKey]
        public string SellerID { get; set; }
        public ReportTypeEnum ReportType { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> Headers { get; set; }
        public ReportFilters Filters { get; set; }
        public bool AvailableToSuppliers { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ReportTypeEnum
    {
        BuyerLocation,
        SalesOrderDetail,
        PurchaseOrderDetail,
        LineItemDetail,
        RMADetail,
        ShipmentDetail,
        ProductDetail
    }

    public class ReportFilters
    {
        public List<string> BuyerID { get; set; }
        public List<string> Country { get; set; }
        public List<string> State { get; set; }
        public List<string> SubmittedOrderStatus { get; set; }
        public List<string> OrderType { get; set; }
        public List<string> RMAType { get; set; }
        public List<string> RMAStatus { get; set; }
        public List<string> ShippingStatus { get; set; }
        public List<string> Status { get; set; }
        //Only properties that are nested and being used as filters need to be setup here with their relative path and their full path.
        public static readonly Dictionary<string, string> NestedLocations = new Dictionary<string, string>
        {
            { "SubmittedOrderStatus", "xp.SubmittedOrderStatus" },
            { "OrderType", "xp.OrderType" },
            { "Country", "xp.ShippingAddress.Country" },
            { "RMAStatus", "Status" },
            { "RMAType", "Type" },
        };
    }

    public class ReportAdHocFilters
    {
        public string LowDate { get; set; }
        public string HighDate { get; set; }
        public string LowTime { get; set; }
        public string HighTime { get; set; }
        public string SupplierID { get; set; }
    }

    public class ReportHeaderPaths
    {
        public static readonly List<string> BuyerLineDetailReport = new List<string>
        {
            "HSOrder.ID",
            "HSLineItem.Product.xp.Facets.supplier",
            "HSLineItem.SupplierID",
            "HSOrder.DateSubmitted",
            "HSOrder.DateCompleted",
            "HSOrder.xp.OrderType",
            "HSOrder.Total",
            "HSOrder.TaxCost",
            "HSOrder.ShippingCost",
            "HSOrder.PromotionDiscount",
            "HSOrder.Subtotal",
            "HSOrder.xp.Currency",
            "HSOrder.Status",
            "HSOrder.xp.SubmittedOrderStatus",
            "HSOrder.xp.ShippingStatus",
            "HSOrder.xp.PaymentMethod",
            "HSLineItem.ID",
            "HSLineItem.UnitPrice",
            "HSLineItem.LineTotal",
            "HSLineItem.LineSubtotal",
            "HSLineItem.ProductID",
            "HSLineItem.Product.Name",
            "HSLineItem.Product.xp.ProductType",
            "HSLineItem.Variant.ID",
            "HSLineItem.Variant.xp.SpecCombo",
            "HSLineItem.Quantity",
            "HSLineItem.Product.xp.Tax.Code",
            "HSLineItem.Product.xp.IsResale",
            "HSLineItem.Product.xp.UnitOfMeasure.Qty",
            "HSLineItem.Product.xp.UnitOfMeasure.Unit",
            "HSLineItem.Product.xp.HasVariants",
            "HSLineItem.xp.ShipMethod",
            "HSLineItem.ShippingAddress.FirstName",
            "HSLineItem.ShippingAddress.LastName",
            "HSLineItem.ShippingAddress.Street1",
            "HSLineItem.ShippingAddress.Street2",
            "HSLineItem.ShippingAddress.City",
            "HSLineItem.ShippingAddress.State",
            "HSLineItem.ShippingAddress.Zip",
            "HSLineItem.ShippingAddress.Country",
            "HSOrder.BillingAddress.Street1",
            "HSOrder.BillingAddress.Street2",
            "HSOrder.BillingAddress.City",
            "HSOrder.BillingAddress.State",
            "HSOrder.BillingAddress.Zip",
            "HSOrder.BillingAddress.Country",
            "HSLineItem.ShippingAddress.xp.BillingNumber",
            "HSLineItem.ShippingAddress.xp.LocationID",
            "HSLineItem.ShippingAddress.CompanyName",
            "HSOrder.FromUser.ID",
            "HSOrder.FromUser.Username",
            "HSOrder.FromUser.FirstName",
            "HSOrder.FromUser.LastName",
            "HSLineItem.ShippingAddress.xp.Email",
            "HSOrder.FromUser.Phone",
            "HSLineItem.xp.StatusByQuantity.Submitted",
            "HSLineItem.xp.StatusByQuantity.Backordered",
            "HSLineItem.xp.StatusByQuantity.CancelRequested",
            "HSLineItem.xp.StatusByQuantity.CancelDenied",
            "HSLineItem.xp.StatusByQuantity.Complete",
            "HSLineItem.xp.StatusByQuantity.ReturnRequested",
            "HSLineItem.xp.StatusByQuantity.ReturnDenied",
            "HSLineItem.xp.StatusByQuantity.Returned",
            "HSLineItem.xp.StatusByQuantity.Canceled",
            "HSLineItem.xp.StatusByQuantity.Open",
            "HSLineItem.PromotionDiscount",
        };
    }
}
