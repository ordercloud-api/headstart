using OrderCloud.SDK;
using Headstart.Models;
using OrderCloud.Catalyst;
using Headstart.API.Commands;
using System.Threading.Tasks;
using Headstart.Common.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Headstart.Models.Headstart;
using Headstart.Common.Repositories.Models;

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

        /// <summary>
        /// The IOC based constructor method for the ReportController class object with Dependency Injection
        /// </summary>
        /// <param name="reportDataCommand"></param>
        /// <param name="settings"></param>
        /// <param name="downloadReportCommand"></param>
        public ReportController(IHSReportCommand reportDataCommand, AppSettings settings, DownloadReportCommand downloadReportCommand)
        {
            _reportDataCommand = reportDataCommand;
            _downloadReportCommand = downloadReportCommand;
        }

        public class ReportRequestBody
        {
            public string[] Headers { get; set; }
        }

        /// <summary>
        /// Gets the ListPage of ReportTypeResource objects (GET method)
        /// </summary>
        /// <returns>The ListPage of ReportTypeResource objects</returns>
        [HttpGet, Route("fetchAllReportTypes"), OrderCloudUserAuth("HSReportReader", "HSReportAdmin")]
        public ListPage<ReportTypeResource> FetchAllReportTypes()
        {
            return _reportDataCommand.FetchAllReportTypes(UserContext);
        }

        /// <summary>
        /// Gets the list of Addresses for a Buyer Location (GET method)
        /// </summary>
        /// <param name="templateID"></param>
        /// <returns>The list of addresses for a Buyer Location (GET method)</returns>
        [HttpGet, Route("BuyerLocation/preview/{templateID}"), OrderCloudUserAuth("HSReportReader", "HSReportAdmin")]
        public async Task<List<Address>> BuyerLocation(string templateID)
        {
            return await _reportDataCommand.BuyerLocation(templateID, UserContext);
        }

        /// <summary>
        /// Post action for the download of the BuyerLocation request (POST method)
        /// </summary>
        /// <param name="reportTemplate"></param>
        /// <param name="templateID"></param>
        /// <returns>The response from the post action for the download of the BuyerLocation request</returns>
        [HttpPost, Route("BuyerLocation/download/{templateID}"), OrderCloudUserAuth("HSReportReader", "HSReportAdmin")]
        public async Task<string> DownloadBuyerLocation([FromBody] ReportTemplate reportTemplate, string templateID)
        {
            var reportData = await _reportDataCommand.BuyerLocation(templateID, UserContext);
            return await _downloadReportCommand.ExportToExcel(ReportTypeEnum.BuyerLocation, reportTemplate.Headers, reportData);

        }

        /// <summary>
        /// Gets the list of ProductDetailData objects (GET method)
        /// </summary>
        /// <param name="templateID"></param>
        /// <param name="args"></param>
        /// <returns>The list of ProductDetailData objects</returns>
        [HttpGet, Route("ProductDetail/preview/{templateID}"), OrderCloudUserAuth("HSReportAdmin", "HSReportReader")]
        public async Task<List<ProductDetailData>> ProductDetail(string templateID, ListArgs<ReportAdHocFilters> args)
        {
            return await _reportDataCommand.ProductDetail(templateID, args, UserContext);
        }

        /// <summary>
        /// Post action for the download of the ProductDetail request (POST method)
        /// </summary>
        /// <param name="reportTemplate"></param>
        /// <param name="templateID"></param>
        /// <param name="args"></param>
        /// <returns>The response from the post action for the download of the ProductDetail request</returns>
        [HttpPost, Route("ProductDetail/download/{templateID}"), OrderCloudUserAuth("HSReportAdmin", "HSReportReader")]
        public async Task<string> ProductDetail([FromBody] ReportTemplate reportTemplate, string templateID, ListArgs<ReportAdHocFilters> args)
        {
            var reportData = await _reportDataCommand.ProductDetail(templateID, args, UserContext);
            return await _downloadReportCommand.ExportToExcel(ReportTypeEnum.ProductDetail, reportTemplate.Headers, reportData);
        }

        /// <summary>
        /// Gets the list of OrderDetailData objects (GET method)
        /// </summary>
        /// <param name="templateID"></param>
        /// <param name="args"></param>
        /// <returns>The list of OrderDetailData objects</returns>
        [HttpGet, Route("SalesOrderDetail/preview/{templateID}"), OrderCloudUserAuth("HSReportAdmin")]
        public async Task<List<OrderDetailData>> SalesOrderDetail(string templateID, ListArgs<ReportAdHocFilters> args)
        {
            return await _reportDataCommand.SalesOrderDetail(templateID, args, UserContext);
        }

        /// <summary>
        /// Post action for the download of the SalesOrderDetail request (POST method)
        /// </summary>
        /// <param name="reportTemplate"></param>
        /// <param name="templateID"></param>
        /// <param name="args"></param>
        /// <returns>The response from the post action for the download of the SalesOrderDetail request</returns>
        [HttpPost, Route("SalesOrderDetail/download/{templateID}"), OrderCloudUserAuth("HSReportAdmin")]
        public async Task<string> DownloadSalesOrderDetail([FromBody] ReportTemplate reportTemplate, string templateID, ListArgs<ReportAdHocFilters> args)
        {
            var reportData = await _reportDataCommand.SalesOrderDetail(templateID, args, UserContext);
            return await _downloadReportCommand.ExportToExcel(ReportTypeEnum.SalesOrderDetail, reportTemplate.Headers, reportData);
        }

        /// <summary>
        /// Gets the list of OrderDetailData objects (GET method)
        /// </summary>
        /// <param name="templateID"></param>
        /// <param name="args"></param>
        /// <returns>The list of OrderDetailData objects</returns>
        [HttpGet, Route("PurchaseOrderDetail/preview/{templateID}"), OrderCloudUserAuth("HSReportReader", "HSReportAdmin")]
        public async Task<List<OrderDetailData>> PurchaseOrderDetail(string templateID, ListArgs<ReportAdHocFilters> args)
        {
            return await _reportDataCommand.PurchaseOrderDetail(templateID, args, UserContext);
        }

        /// <summary>
        /// Post action for the download of the PurchaseOrderDetail request (POST method)
        /// </summary>
        /// <param name="reportTemplate"></param>
        /// <param name="templateID"></param>
        /// <param name="args"></param>
        /// <returns>The response from the post action for the download of the PurchaseOrderDetail request</returns>
        [HttpPost, Route("PurchaseOrderDetail/download/{templateID}"), OrderCloudUserAuth("HSReportReader", "HSReportAdmin")]
        public async Task<string> DownloadPurchaseOrderDetail([FromBody] ReportTemplate reportTemplate, string templateID, ListArgs<ReportAdHocFilters> args)
        {
            var reportData = await _reportDataCommand.PurchaseOrderDetail(templateID, args, UserContext);
            return await _downloadReportCommand.ExportToExcel(ReportTypeEnum.PurchaseOrderDetail, reportTemplate.Headers, reportData);
        }

        /// <summary>
        /// Post action for the download of the BuyerLineItemDetail request (POST method)
        /// </summary>
        /// <param name="viewContext"></param>
        /// <param name="userID"></param>
        /// <param name="locationID"></param>
        /// <param name="args"></param>
        /// <returns>The response from the post action for the download of the BuyerLineItemDetail request</returns>
        [HttpGet, Route("buyer/lineitemdetail/{viewContext}/{userID}/{locationID}"), OrderCloudUserAuth(ApiRole.MeAdmin)]
        public async Task<string> DownloadBuyerLineItemDetail(BuyerReportViewContext viewContext, string userID, string locationID, ListArgs<HSOrder> args)
        {
            var reportData = await _reportDataCommand.BuyerLineItemDetail(args, viewContext, userID, locationID, UserContext);
            var reportHeaders = ReportHeaderPaths.BuyerLineDetailReport;
            return await _downloadReportCommand.ExportToExcel(ReportTypeEnum.LineItemDetail, reportHeaders, reportData);
        }

        /// <summary>
        /// Gets the list of HSLineItemOrder objects (GET method)
        /// </summary>
        /// <param name="templateID"></param>
        /// <param name="args"></param>
        /// <returns>The list of HSLineItemOrder objects</returns>
        [HttpGet, Route("LineItemDetail/preview/{templateID}"), OrderCloudUserAuth("HSReportReader", "HSReportAdmin")]
        public async Task<List<HSLineItemOrder>> LineItemDetail(string templateID, ListArgs<ReportAdHocFilters> args)
        {
            return await _reportDataCommand.LineItemDetail(templateID, args, UserContext);
        }

        /// <summary>
        /// Post action for the download of the LineItemDetail request (POST method)
        /// </summary>
        /// <param name="reportTemplate"></param>
        /// <param name="templateID"></param>
        /// <param name="args"></param>
        /// <returns>The response from the post action for the download of the LineItemDetail request</returns>
        [HttpPost, Route("LineItemDetail/download/{templateID}"), OrderCloudUserAuth("HSReportReader", "HSReportAdmin")]
        public async Task<string> DownloadLineItemDetail([FromBody] ReportTemplate reportTemplate, string templateID, ListArgs<ReportAdHocFilters> args)
        {
            var reportData = await _reportDataCommand.LineItemDetail(templateID, args, UserContext);
            return await _downloadReportCommand.ExportToExcel(ReportTypeEnum.LineItemDetail, reportTemplate.Headers, reportData);
        }

        /// <summary>
        /// Gets the list of RMAWithRMALineItem objects (GET method)
        /// </summary>
        /// <param name="templateID"></param>
        /// <param name="args"></param>
        /// <returns>The list of RMAWithRMALineItem objects</returns>
        [HttpGet, Route("RMADetail/preview/{templateID}"), OrderCloudUserAuth("HSReportReader", "HSReportAdmin")]
        public async Task<List<RMAWithRMALineItem>> RMADetail(string templateID, ListArgs<ReportAdHocFilters> args)
        {
            return await _reportDataCommand.RMADetail(templateID, args, UserContext);
        }

        /// <summary>
        /// Post action for the download of the RMADetail request (POST method)
        /// </summary>
        /// <param name="reportTemplate"></param>
        /// <param name="templateID"></param>
        /// <param name="args"></param>
        /// <returns>The response from the post action for the download of the RMADetail request</returns>
        [HttpPost, Route("RMADetail/download/{templateID}"), OrderCloudUserAuth("HSReportReader", "HSReportAdmin")]
        public async Task<string> DownloadRMADetail([FromBody] ReportTemplate reportTemplate, string templateID, ListArgs<ReportAdHocFilters> args)
        {
            var reportData = await _reportDataCommand.RMADetail(templateID, args, UserContext);
            return await _downloadReportCommand.ExportToExcel(ReportTypeEnum.RMADetail, reportTemplate.Headers, reportData);
        }

        /// <summary>
        /// Gets the list of ShipmentDetail objects (GET method)
        /// </summary>
        /// <param name="templateID"></param>
        /// <param name="args"></param>
        /// <returns>The list of ShipmentDetail objects</returns>
        [HttpGet, Route("ShipmentDetail/preview/{templateID}"), OrderCloudUserAuth("HSReportReader", "HSReportAdmin")]
        public async Task<List<OrderWithShipments>> ShipmentDetail(string templateID, ListArgs<ReportAdHocFilters> args)
        {
            return await _reportDataCommand.ShipmentDetail(templateID, args, UserContext);
        }

        /// <summary>
        /// Post action for the download of the ShipmentDetail request (POST method)
        /// </summary>
        /// <param name="reportTemplate"></param>
        /// <param name="templateID"></param>
        /// <param name="args"></param>
        /// <returns>The response from the post action for the download of the ShipmentDetail request</returns>
        [HttpPost, Route("ShipmentDetail/download/{templateID}"), OrderCloudUserAuth("HSReportReader", "HSReportAdmin")]
        public async Task<string> DownloadShipmentDetail([FromBody] ReportTemplate reportTemplate, string templateID, ListArgs<ReportAdHocFilters> args)
        {
            var reportData = await _reportDataCommand.ShipmentDetail(templateID, args, UserContext);
            return await _downloadReportCommand.ExportToExcel(ReportTypeEnum.ShipmentDetail, reportTemplate.Headers, reportData);
        }

        /// <summary>
        /// Get action for the download of the SharedAccessSignature request (GET method)
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>The response from the get action for the download of the SharedAccessSignature request</returns>
        [HttpGet, Route("download-shared-access/{fileName}"), OrderCloudUserAuth(ApiRole.MeAdmin)]
        public async Task<string> GetSharedAccessSignature(string fileName)
        {
            return await _downloadReportCommand.GetSharedAccessSignature(fileName);
        }

        /// <summary>
        /// Gets the list of ReportTemplatesByReportType objects (GET method)
        /// </summary>
        /// <param name="reportType"></param>
        /// <returns>The list of ReportTemplatesByReportType objects</returns>
        [HttpGet, Route("{reportType}/listtemplates"), OrderCloudUserAuth("HSReportReader", "HSReportAdmin")]
        public async Task<List<ReportTemplate>> ListReportTemplatesByReportType(ReportTypeEnum reportType)
        {
            return await _reportDataCommand.ListReportTemplatesByReportType(reportType, UserContext);
        }

        /// <summary>
        /// Post action for the ReportTemplate request (POST method)
        /// </summary>
        /// <param name="reportType"></param>
        /// <param name="reportTemplate"></param>
        /// <returns>The response from the post action for the ReportTemplate request</returns>
        [HttpPost, Route("{reportType}"), OrderCloudUserAuth("AdminUserAdmin", "HSReportAdmin")]
        public async Task<ReportTemplate> PostReportTemplate(ReportTypeEnum reportType, [FromBody] ReportTemplate reportTemplate)
        {
            return await _reportDataCommand.PostReportTemplate(reportTemplate, UserContext);
        }

        /// <summary>
        /// Gets the ReportTemplate object (GET method)
        /// </summary>
        /// <param name="id"></param>
        /// <returns>The ReportTemplate object</returns>
        [HttpGet, Route("{id}"), OrderCloudUserAuth("AdminUserAdmin", "HSReportAdmin")]
        public async Task<ReportTemplate> GetReportTemplate(string id)
        {
            return await _reportDataCommand.GetReportTemplate(id, UserContext);
        }

        /// <summary>
        /// Update action for the ReportTemplate request (PUT method)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="reportTemplate"></param>
        /// <returns>The response from the update action for the ReportTemplate request</returns>
        [HttpPut, Route("{id}"), OrderCloudUserAuth("AdminUserAdmin", "HSReportAdmin")]
        public async Task<ReportTemplate> UpdateReportTemplate(string id, [FromBody] ReportTemplate reportTemplate)
        {
            return await _reportDataCommand.UpdateReportTemplate(id, reportTemplate, UserContext);
        }

        /// <summary>
        /// Removes/Deletes the ReportTemplate action (DELETE method)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete, Route("{id}"), OrderCloudUserAuth("AdminUserAdmin", "HSReportAdmin")]
        public async Task DeleteReportTemplate(string id)
        {
            await _reportDataCommand.DeleteReportTemplate(id);
        }

        /// <summary>
        /// Gets the list of HSBuyer objects (GET method)
        /// </summary>
        /// <returns>The list of HSBuyer objects</returns>
        [HttpGet, Route("filters/buyers"), OrderCloudUserAuth("HSReportReader", "HSReportAdmin")]
        public async Task<List<HSBuyer>> GetBuyerFilterValues()
        {
            return await _reportDataCommand.GetBuyerFilterValues(UserContext);
        }
    }
}