using System.Text.Json.Serialization;

namespace Headstart.Common.Models
{
	public class ReportTypeResource
	{
		public ReportTypeEnum Id { get; set; }

		public string Name { get; set; } = string.Empty;

		public string ReportCategory { get; set; } = string.Empty;

		public bool AvailableToSuppliers { get; set; }

		public string Value { get; set; } = string.Empty;

		public string[] AdHocFilters { get; set; }

		//Explicitly setting available report types
		public static ReportTypeResource[] ReportTypes = {
			new ReportTypeResource
			{
				Id = ReportTypeEnum.BuyerLocation,
				Name = @"Buyer Group Report",
				ReportCategory = @"Buyer",
				AvailableToSuppliers = true,
				Value = @"BuyerLocation",
				AdHocFilters = null
			},
			new ReportTypeResource {
				Id = ReportTypeEnum.SalesOrderDetail,
				Name = @"Sales Order Detail Report",
				ReportCategory = @"Order",
				AvailableToSuppliers = false,
				Value = @"SalesOrderDetail",
				AdHocFilters = new string[]
				{
					@"DateLow", 
					@"DateHigh", 
					@"TimeLow", 
					@"TimeHigh", 
					@"SupplierID", 
					@"BrandID"
				}
			},
			new ReportTypeResource
			{
				Id = ReportTypeEnum.PurchaseOrderDetail,
				Name = @"Purchase Order Detail Report",
				ReportCategory = @"Order",
				AvailableToSuppliers = true,
				Value = @"PurchaseOrderDetail",
				AdHocFilters = new string[]
				{
					@"DateLow", 
					@"DateHigh", 
					@"TimeLow", 
					@"TimeHigh", 
					@"SupplierID", 
					@"BrandID"
				}
			},
			new ReportTypeResource
			{
				Id = ReportTypeEnum.LineItemDetail,
				Name = @"Line Item Detail Report",
				ReportCategory = @"Order",
				AvailableToSuppliers = true,
				Value = @"LineItemDetail",
				AdHocFilters = new string[]
				{
					@"DateLow", 
					@"DateHigh", 
					@"TimeLow", 
					@"TimeHigh", 
					@"SupplierID", 
					@"BrandID"
				}
			},
			new ReportTypeResource
			{
				Id = ReportTypeEnum.ProductDetail,
				Name = @"Product Detail Report",
				ReportCategory = @"Product",
				AvailableToSuppliers = true,
				Value = @"ProductDetail",
				AdHocFilters = new string[]
				{
					@"SupplierID"
				}
			},
			new ReportTypeResource
			{
				Id = ReportTypeEnum.RMADetail,
				Name = @"RMA Detail Report",
				ReportCategory = @"RMA",
				AvailableToSuppliers = true,
				Value = @"RMADetail",
				AdHocFilters = new string[]
				{
					@"DateLow", 
					@"DateHigh", 
					@"TimeLow", 
					@"TimeHigh", 
					@"SupplierID"
				}
			},
			new ReportTypeResource
			{
				Id = ReportTypeEnum.ShipmentDetail,
				Name = @"Shipment Detail Report",
				ReportCategory = @"Order",
				AvailableToSuppliers = true,
				Value = @"ShipmentDetail",
				AdHocFilters = new string[]
				{
					@"DateLow", 
					@"DateHigh", 
					@"TimeLow", 
					@"TimeHigh", 
					@"SupplierID"
				}
			}
		};
	}

	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum BuyerReportViewContext
	{
		MyOrders,
		Approve,
		Location
	}
}