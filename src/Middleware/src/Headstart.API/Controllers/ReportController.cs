using System.Collections.Generic;
using System.Threading.Tasks;
using Headstart.API.Commands;
using Headstart.Common.Models;
using Microsoft.AspNetCore.Mvc;
using OrderCloud.Catalyst;
using OrderCloud.Integrations.Reporting.Models;
using OrderCloud.Integrations.RMAs.Models;
using OrderCloud.SDK;

namespace Headstart.Common.Controllers
{
    /// <summary>
    ///  For generating and downloading reports in the Admin application.
    /// </summary>
    [Route("reports")]
    public class ReportController : CatalystController
    {
        private readonly IHSReportCommand reportCommand;
        private readonly IDownloadReportCommand downloadReportCommand;

        public ReportController(IHSReportCommand reportCommand, IDownloadReportCommand downloadReportCommand)
        {
            this.reportCommand = reportCommand;
            this.downloadReportCommand = downloadReportCommand;
        }

        [HttpGet, Route("fetchAllReportTypes"), OrderCloudUserAuth("HSReportReader", "HSReportAdmin")]
        public ListPage<ReportTypeResource> FetchAllReportTypes()
        {
            return reportCommand.FetchAllReportTypes(UserContext);
        }

        [HttpGet, Route("BuyerLocation/preview/{templateID}"), OrderCloudUserAuth("HSReportReader", "HSReportAdmin")]
        public async Task<List<Address>> BuyerLocation(string templateID)
        {
            return await reportCommand.BuyerLocation(templateID, UserContext);
        }

        [HttpPost, Route("BuyerLocation/download/{templateID}"), OrderCloudUserAuth("HSReportReader", "HSReportAdmin")]
        public async Task<string> DownloadBuyerLocation([FromBody] ReportTemplate reportTemplate, string templateID)
        {
            var reportData = await reportCommand.BuyerLocation(templateID, UserContext);
            return await downloadReportCommand.ExportToExcel(ReportTypeEnum.BuyerLocation, reportTemplate.Headers, reportData);
        }

        [HttpGet, Route("ProductDetail/preview/{templateID}"), OrderCloudUserAuth("HSReportAdmin", "HSReportReader")]
        public async Task<List<ProductDetailData>> ProductDetail(string templateID, ListArgs<ReportAdHocFilters> args)
        {
            return await reportCommand.ProductDetail(templateID, args, UserContext);
        }

        [HttpPost, Route("ProductDetail/download/{templateID}"), OrderCloudUserAuth("HSReportAdmin", "HSReportReader")]
        public async Task<string> ProductDetail([FromBody] ReportTemplate reportTemplate, string templateID, ListArgs<ReportAdHocFilters> args)
        {
            var reportData = await reportCommand.ProductDetail(templateID, args, UserContext);
            return await downloadReportCommand.ExportToExcel(ReportTypeEnum.ProductDetail, reportTemplate.Headers, reportData);
        }

        [HttpGet, Route("SalesOrderDetail/preview/{templateID}"), OrderCloudUserAuth("HSReportAdmin")]
        public async Task<List<OrderDetailData>> SalesOrderDetail(string templateID, ListArgs<ReportAdHocFilters> args)
        {
            return await reportCommand.SalesOrderDetail(templateID, args, UserContext);
        }

        [HttpPost, Route("SalesOrderDetail/download/{templateID}"), OrderCloudUserAuth("HSReportAdmin")]
        public async Task<string> DownloadSalesOrderDetail([FromBody] ReportTemplate reportTemplate, string templateID, ListArgs<ReportAdHocFilters> args)
        {
            var reportData = await reportCommand.SalesOrderDetail(templateID, args, UserContext);
            return await downloadReportCommand.ExportToExcel(ReportTypeEnum.SalesOrderDetail, reportTemplate.Headers, reportData);
        }

        [HttpGet, Route("PurchaseOrderDetail/preview/{templateID}"), OrderCloudUserAuth("HSReportReader", "HSReportAdmin")]
        public async Task<List<OrderDetailData>> PurchaseOrderDetail(string templateID, ListArgs<ReportAdHocFilters> args)
        {
            return await reportCommand.PurchaseOrderDetail(templateID, args, UserContext);
        }

        [HttpPost, Route("PurchaseOrderDetail/download/{templateID}"), OrderCloudUserAuth("HSReportReader", "HSReportAdmin")]
        public async Task<string> DownloadPurchaseOrderDetail([FromBody] ReportTemplate reportTemplate, string templateID, ListArgs<ReportAdHocFilters> args)
        {
            var reportData = await reportCommand.PurchaseOrderDetail(templateID, args, UserContext);
            return await downloadReportCommand.ExportToExcel(ReportTypeEnum.PurchaseOrderDetail, reportTemplate.Headers, reportData);
        }

        [HttpGet, Route("buyer/lineitemdetail/{viewContext}/{userID}/{locationID}"), OrderCloudUserAuth(ApiRole.MeAdmin)]
        public async Task<string> DownloadBuyerLineItemDetail(BuyerReportViewContext viewContext, string userID, string locationID, ListArgs<HSOrder> args)
        {
            var reportData = await reportCommand.BuyerLineItemDetail(args, viewContext, userID, locationID, UserContext);
            var reportHeaders = ReportHeaderPaths.BuyerLineDetailReport;
            return await downloadReportCommand.ExportToExcel(ReportTypeEnum.LineItemDetail, reportHeaders, reportData);
        }

        [HttpGet, Route("LineItemDetail/preview/{templateID}"), OrderCloudUserAuth("HSReportReader", "HSReportAdmin")]
        public async Task<List<HSLineItemOrder>> LineItemDetail(string templateID, ListArgs<ReportAdHocFilters> args)
        {
            return await reportCommand.LineItemDetail(templateID, args, UserContext);
        }

        [HttpPost, Route("LineItemDetail/download/{templateID}"), OrderCloudUserAuth("HSReportReader", "HSReportAdmin")]
        public async Task<string> DownloadLineItemDetail([FromBody] ReportTemplate reportTemplate, string templateID, ListArgs<ReportAdHocFilters> args)
        {
            var reportData = await reportCommand.LineItemDetail(templateID, args, UserContext);
            return await downloadReportCommand.ExportToExcel(ReportTypeEnum.LineItemDetail, reportTemplate.Headers, reportData);
        }

        [HttpGet, Route("RMADetail/preview/{templateID}"), OrderCloudUserAuth("HSReportReader", "HSReportAdmin")]
        public async Task<List<RMAWithRMALineItem>> RMADetail(string templateID, ListArgs<ReportAdHocFilters> args)
        {
            return await reportCommand.RMADetail(templateID, args, UserContext);
        }

        [HttpPost, Route("RMADetail/download/{templateID}"), OrderCloudUserAuth("HSReportReader", "HSReportAdmin")]
        public async Task<string> DownloadRMADetail([FromBody] ReportTemplate reportTemplate, string templateID, ListArgs<ReportAdHocFilters> args)
        {
            var reportData = await reportCommand.RMADetail(templateID, args, UserContext);
            return await downloadReportCommand.ExportToExcel(ReportTypeEnum.RMADetail, reportTemplate.Headers, reportData);
        }

        [HttpGet, Route("ShipmentDetail/preview/{templateID}"), OrderCloudUserAuth("HSReportReader", "HSReportAdmin")]
        public async Task<List<OrderWithShipments>> ShipmentDetail(string templateID, ListArgs<ReportAdHocFilters> args)
        {
            return await reportCommand.ShipmentDetail(templateID, args, UserContext);
        }

        [HttpPost, Route("ShipmentDetail/download/{templateID}"), OrderCloudUserAuth("HSReportReader", "HSReportAdmin")]
        public async Task<string> DownloadShipmentDetail([FromBody] ReportTemplate reportTemplate, string templateID, ListArgs<ReportAdHocFilters> args)
        {
            var reportData = await reportCommand.ShipmentDetail(templateID, args, UserContext);
            return await downloadReportCommand.ExportToExcel(ReportTypeEnum.ShipmentDetail, reportTemplate.Headers, reportData);
        }

        [HttpGet, Route("download-shared-access/{fileName}"), OrderCloudUserAuth(ApiRole.MeAdmin)]
        public async Task<string> GetSharedAccessSignature(string fileName)
        {
            return await downloadReportCommand.GetSharedAccessSignature(fileName);
        }

        [HttpGet, Route("{reportType}/listtemplates"), OrderCloudUserAuth("HSReportReader", "HSReportAdmin")]
        public async Task<List<ReportTemplate>> ListReportTemplatesByReportType(ReportTypeEnum reportType)
        {
            return await reportCommand.ListReportTemplatesByReportType(reportType, UserContext);
        }

        [HttpPost, Route("{reportType}"), OrderCloudUserAuth("AdminUserAdmin", "HSReportAdmin")]
        public async Task<ReportTemplate> PostReportTemplate(ReportTypeEnum reportType, [FromBody] ReportTemplate reportTemplate)
        {
            return await reportCommand.PostReportTemplate(reportTemplate, UserContext);
        }

        [HttpGet, Route("{id}"), OrderCloudUserAuth("AdminUserAdmin", "HSReportAdmin")]
        public async Task<ReportTemplate> GetReportTemplate(string id)
        {
            return await reportCommand.GetReportTemplate(id, UserContext);
        }

        [HttpPut, Route("{id}"), OrderCloudUserAuth("AdminUserAdmin", "HSReportAdmin")]
        public async Task<ReportTemplate> UpdateReportTemplate(string id, [FromBody] ReportTemplate reportTemplate)
        {
            return await reportCommand.UpdateReportTemplate(id, reportTemplate, UserContext);
        }

        [HttpDelete, Route("{id}"), OrderCloudUserAuth("AdminUserAdmin", "HSReportAdmin")]
        public async Task DeleteReportTemplate(string id)
        {
            await reportCommand.DeleteReportTemplate(id);
        }

        [HttpGet, Route("filters/buyers"), OrderCloudUserAuth("HSReportReader", "HSReportAdmin")]
        public async Task<List<HSBuyer>> GetBuyerFilterValues()
        {
            return await reportCommand.GetBuyerFilterValues(UserContext);
        }

        public class ReportRequestBody
        {
            public string[] Headers { get; set; }
        }
    }
}
