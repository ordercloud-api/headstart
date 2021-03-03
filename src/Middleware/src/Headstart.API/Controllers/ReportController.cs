using Headstart.Models;
using Microsoft.AspNetCore.Mvc;
using OrderCloud.SDK;
using System.Threading.Tasks;
using System.Collections.Generic;
using ordercloud.integrations.library;
using Headstart.Models.Attributes;
using Headstart.Common.Models;
using Headstart.Models.Misc;
using Headstart.API.Controllers;
using Headstart.API.Commands;
using Headstart.Models.Headstart;
using OrderCloud.Catalyst;

namespace Headstart.Common.Controllers
{
    [DocComments("\"Headstart Reports\" for generating and downloading reports in the Admin application")]
    [HSSection.Headstart(ListOrder = 11)]
    [Route("reports")]
    public class ReportController : HeadstartController
    {
        private readonly IHSReportCommand _reportDataCommand;
        private readonly DownloadReportCommand _downloadReportCommand;

        public ReportController(IHSReportCommand reportDataCommand, AppSettings settings, DownloadReportCommand downloadReportCommand) : base(settings)
        {
            _reportDataCommand = reportDataCommand;
            _downloadReportCommand = downloadReportCommand;
        }

        public class ReportRequestBody
        {
            public string[] Headers { get; set; }
        }

        [HttpGet, Route("fetchAllReportTypes"), OrderCloudIntegrationsAuth]
        public ListPage<ReportTypeResource> FetchAllReportTypes()
        {
            RequireOneOf(CustomRole.HSReportReader, CustomRole.HSReportAdmin);
            return _reportDataCommand.FetchAllReportTypes(Context);
        }

        [HttpGet, Route("BuyerLocation/preview/{templateID}"), OrderCloudIntegrationsAuth]
        public async Task<List<HSAddressBuyer>> BuyerLocation(string templateID)
        {
            RequireOneOf(CustomRole.HSReportReader, CustomRole.HSReportAdmin);
            return await _reportDataCommand.BuyerLocation(templateID, Context);
        }

        [HttpPost, Route("BuyerLocation/download/{templateID}"), OrderCloudIntegrationsAuth]
        public async Task<string> DownloadBuyerLocation([FromBody] ReportTemplate reportTemplate, string templateID)
        {
            RequireOneOf(CustomRole.HSReportReader, CustomRole.HSReportAdmin);
            var reportData = await _reportDataCommand.BuyerLocation(templateID, Context);
            return await _downloadReportCommand.ExportToExcel(ReportTypeEnum.BuyerLocation, reportTemplate, reportData);
        }

        [HttpGet, Route("SalesOrderDetail/preview/{templateID}"), OrderCloudIntegrationsAuth]
        public async Task<List<HSOrder>> SalesOrderDetail(string templateID, ListArgs<ReportAdHocFilters> args)
        {
            RequireOneOf(CustomRole.HSReportAdmin);
            return await _reportDataCommand.SalesOrderDetail(templateID, args, Context);
        }

        [HttpPost, Route("SalesOrderDetail/download/{templateID}"), OrderCloudIntegrationsAuth]
        public async Task<string> DownloadSalesOrderDetail([FromBody] ReportTemplate reportTemplate, string templateID, ListArgs<ReportAdHocFilters> args)
        {
            RequireOneOf(CustomRole.HSReportAdmin);
            var reportData = await _reportDataCommand.SalesOrderDetail(templateID, args, Context);
            return await _downloadReportCommand.ExportToExcel(ReportTypeEnum.SalesOrderDetail, reportTemplate, reportData);
        }

        [HttpGet, Route("PurchaseOrderDetail/preview/{templateID}"), OrderCloudIntegrationsAuth]
        public async Task<List<HSOrder>> PurchaseOrderDetail(string templateID, ListArgs<ReportAdHocFilters> args)
        {
            RequireOneOf(CustomRole.HSReportReader, CustomRole.HSReportAdmin);
            return await _reportDataCommand.PurchaseOrderDetail(templateID, args, Context);
        }

        [HttpPost, Route("PurchaseOrderDetail/download/{templateID}"), OrderCloudIntegrationsAuth]
        public async Task<string> DownloadPurchaseOrderDetail([FromBody] ReportTemplate reportTemplate, string templateID, ListArgs<ReportAdHocFilters> args)
        {
            RequireOneOf(CustomRole.HSReportReader, CustomRole.HSReportAdmin);
            var reportData = await _reportDataCommand.PurchaseOrderDetail(templateID, args, Context);
            return await _downloadReportCommand.ExportToExcel(ReportTypeEnum.PurchaseOrderDetail, reportTemplate, reportData);
        }

        [HttpGet, Route("LineItemDetail/preview/{templateID}"), OrderCloudIntegrationsAuth]
        public async Task<List<HSLineItemOrder>> LineItemDetail(string templateID, ListArgs<ReportAdHocFilters> args)
        {
            RequireOneOf(CustomRole.HSReportReader, CustomRole.HSReportAdmin);
            return await _reportDataCommand.LineItemDetail(templateID, args, Context);
        }

        [HttpPost, Route("LineItemDetail/download/{templateID}"), OrderCloudIntegrationsAuth]
        public async Task<string> DownloadLineItemDetail([FromBody] ReportTemplate reportTemplate, string templateID, ListArgs<ReportAdHocFilters> args)
        {
            RequireOneOf(CustomRole.HSReportReader, CustomRole.HSReportAdmin);
            var reportData = await _reportDataCommand.LineItemDetail(templateID, args, Context);
            return await _downloadReportCommand.ExportToExcel(ReportTypeEnum.LineItemDetail, reportTemplate, reportData);
        }

        [HttpGet, Route("download-shared-access/{fileName}"), OrderCloudIntegrationsAuth]
        public string GetSharedAccessSignature(string fileName)
        {
            RequireOneOf(CustomRole.HSReportReader, CustomRole.HSReportAdmin, CustomRole.HSShipmentAdmin);
            return _downloadReportCommand.GetSharedAccessSignature(fileName);
        }

        [HttpGet, Route("{reportType}/listtemplates"), OrderCloudIntegrationsAuth]
        public async Task<List<ReportTemplate>> ListReportTemplatesByReportType(ReportTypeEnum reportType)
        {
            RequireOneOf(CustomRole.HSReportReader, CustomRole.HSReportAdmin);
            return await _reportDataCommand.ListReportTemplatesByReportType(reportType, Context);
        }

        [HttpPost, Route("{reportType}"), OrderCloudIntegrationsAuth(ApiRole.AdminUserAdmin)]
        public async Task<ReportTemplate> PostReportTemplate(ReportTypeEnum reportType, [FromBody] ReportTemplate reportTemplate)
        {
            RequireOneOf(CustomRole.HSReportAdmin);
            return await _reportDataCommand.PostReportTemplate(reportTemplate, Context);
        }

        [HttpGet, Route("{id}"), OrderCloudIntegrationsAuth(ApiRole.AdminUserAdmin)]
        public async Task<ReportTemplate> GetReportTemplate(string id)
        {
            RequireOneOf(CustomRole.HSReportAdmin);
            return await _reportDataCommand.GetReportTemplate(id, Context);
        }

        [HttpPut, Route("{id}"), OrderCloudIntegrationsAuth(ApiRole.AdminUserAdmin)]
        public async Task<ReportTemplate> UpdateReportTemplate(string id, [FromBody] ReportTemplate reportTemplate)
        {
            RequireOneOf(CustomRole.HSReportAdmin);
            return await _reportDataCommand.UpdateReportTemplate(id, reportTemplate, Context);
        }

        [HttpDelete, Route("{id}"), OrderCloudIntegrationsAuth(ApiRole.AdminUserAdmin)]
        public async Task DeleteReportTemplate(string id)
        {
            RequireOneOf(CustomRole.HSReportAdmin);
            await _reportDataCommand.DeleteReportTemplate(id);
        }
    }
}
