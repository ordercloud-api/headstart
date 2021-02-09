using Cosmonaut.Attributes;
using Newtonsoft.Json;
using ordercloud.integrations.library;
using System.Collections.Generic;
using Newtonsoft.Json.Converters;
using ordercloud.integrations.library.Cosmos;
using Headstart.Models;

namespace Headstart.Common.Models
{
    [SwaggerModel]
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
        LineItemDetail
    }

    [SwaggerModel]
    public class ReportFilters
    {
        public List<string> BuyerID { get; set; }
        public List<string> Country { get; set; }
        public List<string> State { get; set; }
        public List<string> SubmittedOrderStatus { get; set; }
        public List<string> OrderType { get; set; }
        //Only properties that are nested and being used as filters need to be setup here with their relative path and their full path.
        public static readonly Dictionary<string, string> NestedLocations = new Dictionary<string, string>
        {
            { "SubmittedOrderStatus", "xp.SubmittedOrderStatus" },
            { "OrderType", "xp.OrderType" },
            { "Country", "xp.ShippingAddress.Country" }
        };
    }

    [SwaggerModel]
    public class ReportAdHocFilters
    {
        public string LowDate { get; set; }
        public string HighDate { get; set; }
        public string LowTime { get; set; }
        public string HighTime { get; set; }
    }
}
