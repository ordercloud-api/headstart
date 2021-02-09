using ordercloud.integrations.library;

namespace Headstart.Common.Models
{
    [SwaggerModel]
    public class ReportTypeResource
    {
        public ReportTypeEnum ID { get; set; }
        public string Name { get; set; }
        public string ReportCategory { get; set; }
        public bool AvailableToSuppliers { get; set; }
        public string Value { get; set; }
        public string[] AdHocFilters { get; set; }

        //Explicitly setting available report types
        public static ReportTypeResource[] ReportTypes = {
            new ReportTypeResource
            {
                ID = ReportTypeEnum.BuyerLocation,
                Name = "Buyer Location Report",
                ReportCategory = "Buyer",
                AvailableToSuppliers = true,
                Value = "BuyerLocation",
                AdHocFilters = null
            },
            new ReportTypeResource {
                ID = ReportTypeEnum.SalesOrderDetail,
                Name = "Sales Order Detail Report",
                ReportCategory = "Order",
                AvailableToSuppliers = false,
                Value = "SalesOrderDetail",
                AdHocFilters = new string[] {"DateLow", "TimeLow", "DateHigh", "TimeHigh"}
            },
            new ReportTypeResource
            {
                ID = ReportTypeEnum.PurchaseOrderDetail,
                Name = "Purchase Order Detail Report",
                ReportCategory = "Order",
                AvailableToSuppliers = true,
                Value = "PurchaseOrderDetail",
                AdHocFilters = new string[] {"DateLow", "TimeLow", "DateHigh", "TimeHigh"}
            },
            new ReportTypeResource
            {
                ID = ReportTypeEnum.LineItemDetail,
                Name = "Line Item Detail Report",
                ReportCategory = "Order",
                AvailableToSuppliers = true,
                Value = "LineItemDetail",
                AdHocFilters = new string[] {"DateLow", "TimeLow", "DateHigh", "TimeHigh"}
            }
        };
    }
}
