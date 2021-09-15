using Headstart.Models;
using Microsoft.AspNetCore.Mvc;
using OrderCloud.SDK;
using System.Threading.Tasks;
using System.Collections.Generic;
using ordercloud.integrations.library;
using Headstart.Models.Attributes;
using Headstart.Common.Models;
using Headstart.Models.Misc;
using Headstart.API.Commands;
using Headstart.Models.Headstart;
using OrderCloud.Catalyst;

namespace Headstart.Common.Controllers
{
    /// <summary>
    ///  For generating and downloading reports in the Admin application
    /// </summary>
    [Route("reports")]
    public class ReportController : CatalystController
    {
        private readonly IHSReportCommand _reportDataCommand;
		private readonly DownloadReportCommand _downloadReportCommand;
        private const string HSReportReader = "HSReportReader";
        private const string HSReportAdmin = "HSReportAdmin";
		private const string HSShipmentAdmin = "HSShipmentAdmin";
        private const string AdminUserAdmin = "AdminUserAdmin";

        public ReportController(IHSReportCommand reportDataCommand, AppSettings settings, DownloadReportCommand downloadReportCommand)
        {
            _reportDataCommand = reportDataCommand;
            _downloadReportCommand = downloadReportCommand;
        }

        public class ReportRequestBody
        {
            public string[] Headers { get; set; }
        }

        [HttpGet, Route("fetchAllReportTypes"), OrderCloudUserAuth(HSReportReader, HSReportAdmin)]
        public ListPage<ReportTypeResource> FetchAllReportTypes()
        {
            return _reportDataCommand.FetchAllReportTypes(UserContext);
        }

        [HttpGet, Route("BuyerLocation/preview/{templateID}"), OrderCloudUserAuth(HSReportReader, HSReportAdmin)]
        public async Task<List<HSAddressBuyer>> BuyerLocation(string templateID)
        {
            return await _reportDataCommand.BuyerLocation(templateID, UserContext);
        }

        [HttpPost, Route("BuyerLocation/download/{templateID}"), OrderCloudUserAuth(HSReportReader, HSReportAdmin)]
        public async Task<string> DownloadBuyerLocation([FromBody] ReportTemplate reportTemplate, string templateID)
        {
            var reportData = await _reportDataCommand.BuyerLocation(templateID, UserContext);
            return await _downloadReportCommand.ExportToExcel(ReportTypeEnum.BuyerLocation, reportTemplate, reportData);
        }

        [HttpGet, Route("SalesOrderDetail/preview/{templateID}"), OrderCloudUserAuth(HSReportAdmin)]
        public async Task<List<HSOrder>> SalesOrderDetail(string templateID, ListArgs<ReportAdHocFilters> args)
        {
            return await _reportDataCommand.SalesOrderDetail(templateID, args, UserContext);
        }

        [HttpPost, Route("SalesOrderDetail/download/{templateID}"), OrderCloudUserAuth(HSReportAdmin)]
        public async Task<string> DownloadSalesOrderDetail([FromBody] ReportTemplate reportTemplate, string templateID, ListArgs<ReportAdHocFilters> args)
        {
            var reportData = await _reportDataCommand.SalesOrderDetail(templateID, args, UserContext);
            return await _downloadReportCommand.ExportToExcel(ReportTypeEnum.SalesOrderDetail, reportTemplate, reportData);
        }

        [HttpGet, Route("PurchaseOrderDetail/preview/{templateID}"), OrderCloudUserAuth(HSReportReader, HSReportAdmin)]
        public async Task<List<HSOrder>> PurchaseOrderDetail(string templateID, ListArgs<ReportAdHocFilters> args)
        {
            return await _reportDataCommand.PurchaseOrderDetail(templateID, args, UserContext);
        }

        [HttpPost, Route("PurchaseOrderDetail/download/{templateID}"), OrderCloudUserAuth(HSReportReader, HSReportAdmin)]
        public async Task<string> DownloadPurchaseOrderDetail([FromBody] ReportTemplate reportTemplate, string templateID, ListArgs<ReportAdHocFilters> args)
        {
            var reportData = await _reportDataCommand.PurchaseOrderDetail(templateID, args, UserContext);
            return await _downloadReportCommand.ExportToExcel(ReportTypeEnum.PurchaseOrderDetail, reportTemplate, reportData);
        }

        [HttpGet, Route("LineItemDetail/preview/{templateID}"), OrderCloudUserAuth(HSReportReader, HSReportAdmin)]
        public async Task<List<HSLineItemOrder>> LineItemDetail(string templateID, ListArgs<ReportAdHocFilters> args)
        {
            return await _reportDataCommand.LineItemDetail(templateID, args, UserContext);
        }

        [HttpPost, Route("LineItemDetail/download/{templateID}"), OrderCloudUserAuth(HSReportReader, HSReportAdmin)]
        public async Task<string> DownloadLineItemDetail([FromBody] ReportTemplate reportTemplate, string templateID, ListArgs<ReportAdHocFilters> args)
        {
            var reportData = await _reportDataCommand.LineItemDetail(templateID, args, UserContext);
            return await _downloadReportCommand.ExportToExcel(ReportTypeEnum.LineItemDetail, reportTemplate, reportData);
        }

        [HttpGet, Route("download-shared-access/{fileName}"), OrderCloudUserAuth(HSReportReader, HSReportAdmin, HSShipmentAdmin)]
        public string GetSharedAccessSignature(string fileName)
        {
            return _downloadReportCommand.GetSharedAccessSignature(fileName);
        }

        [HttpGet, Route("{reportType}/listtemplates"), OrderCloudUserAuth(HSReportReader, HSReportAdmin)]
        public async Task<List<ReportTemplate>> ListReportTemplatesByReportType(ReportTypeEnum reportType)
        {
            return await _reportDataCommand.ListReportTemplatesByReportType(reportType, UserContext);
        }

        [HttpPost, Route("{reportType}"), OrderCloudUserAuth(HSReportAdmin, AdminUserAdmin)]
        public async Task<ReportTemplate> PostReportTemplate(ReportTypeEnum reportType, [FromBody] ReportTemplate reportTemplate)
        {
            return await _reportDataCommand.PostReportTemplate(reportTemplate, UserContext);
        }

        [HttpGet, Route("{id}"), OrderCloudUserAuth(HSReportAdmin, AdminUserAdmin)]
        public async Task<ReportTemplate> GetReportTemplate(string id)
        {
            return await _reportDataCommand.GetReportTemplate(id, UserContext);
        }

        [HttpPut, Route("{id}"), OrderCloudUserAuth(HSReportAdmin, AdminUserAdmin)]
        public async Task<ReportTemplate> UpdateReportTemplate(string id, [FromBody] ReportTemplate reportTemplate)
        {
            return await _reportDataCommand.UpdateReportTemplate(id, reportTemplate, UserContext);
        }

        [HttpDelete, Route("{id}"), OrderCloudUserAuth(HSReportAdmin, AdminUserAdmin)]
        public async Task DeleteReportTemplate(string id)
        {
            await _reportDataCommand.DeleteReportTemplate(id);
        }
    }
}
