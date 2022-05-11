using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Headstart.Common.Models;
using Headstart.Common.Repositories;
using Headstart.Common.Services.ShippingIntegration.Models;
using Headstart.Models;
using Headstart.Models.Headstart;
using ordercloud.integrations.library;
using OrderCloud.Catalyst;
using OrderCloud.SDK;

namespace Headstart.Jobs
{
    public class ReceiveRecentLineItemsJob : BaseReportJob
    {
        private readonly IOrderCloudClient _oc;
        private readonly ILineItemDetailDataRepo _lineItemDetailDataRepo;

        public ReceiveRecentLineItemsJob(IOrderCloudClient oc, ILineItemDetailDataRepo lineItemDetailDataRepo)
        {
            _oc = oc;
            _lineItemDetailDataRepo = lineItemDetailDataRepo;
        }

        protected override async Task<ResultCode> ProcessJobAsync(string message)
        {
            try
            {
                await UpsertLineItemDetail(message);
                return ResultCode.Success;
            }
            catch (Exception ex)
            {
                LogFailure($"{ex.Message} {ex?.InnerException?.Message} {ex.StackTrace}");
                return ResultCode.PermanentFailure;
            }
        }

        private async Task UpsertLineItemDetail(string orderID)
        {
            var orderWorksheet = await _oc.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.Incoming, orderID);

            var lineItems = await _oc.LineItems.ListAllAsync<HSLineItem>(OrderDirection.Incoming, orderID);

            var buyer = await _oc.Buyers.GetAsync<HSBuyer>(orderWorksheet.Order.FromCompanyID);

            var lineItemsWithMiscFields = await BuildLineItemsMiscFields(lineItems, orderWorksheet, buyer.Name);

            var lineItemsWithPurchaseOrders = await BuildLineItemsWithPurchaseOrders(orderID);

            var orderLineItemData = new HSOrderLineItemData()
            { 
                Order = orderWorksheet.Order,
                LineItems = lineItems,
                LineItemsWithMiscFields = lineItemsWithMiscFields,
                LineItemsWithPurchaseOrderFields = lineItemsWithPurchaseOrders
            };

            var queryable = _lineItemDetailDataRepo.GetQueryable().Where(order => order.PartitionKey == "PartitionValue");

            var requestOptions = BuildQueryRequestOptions();

            var cosmosLineItemOrder = new LineItemDetailData()
            { 
                PartitionKey = "PartitionValue",
                OrderID = orderID,
                Data = orderLineItemData
            };

            var listOptions = BuildListOptions(orderID);

            CosmosListPage<LineItemDetailData> currentLineItemListPage = await _lineItemDetailDataRepo.GetItemsAsync(queryable, requestOptions, listOptions);

            var cosmosID = "";
            if (currentLineItemListPage.Items.Count() == 1)
            {
                cosmosID = cosmosLineItemOrder.id = currentLineItemListPage.Items[0].id;
            }

            await _lineItemDetailDataRepo.UpsertItemAsync(cosmosID, cosmosLineItemOrder);
        }

        private async Task<List<LineItemsWithPurchaseOrderFields>> BuildLineItemsWithPurchaseOrders(string orderID)
        {
            //returns POs
            var orders = await _oc.Orders.ListAllAsync<HSOrder>(OrderDirection.Outgoing, filters: $"ID={orderID}-*");

            //loop through orders, get line items, pass those.
            List<LineItemsWithPurchaseOrderFields> orderLineItemBySupplierID = await GetLineItemsFromPurchaseOrdersAsync(orders);

            return orderLineItemBySupplierID;

        }

        private async Task<List<LineItemsWithPurchaseOrderFields>> GetLineItemsFromPurchaseOrdersAsync(List<HSOrder> orders)
        {
            var result = new List<LineItemsWithPurchaseOrderFields>() { };

            foreach (HSOrder order in orders)
            {
                List<HSLineItem> lineItemsBySupplier = await _oc.LineItems.ListAllAsync<HSLineItem>(OrderDirection.Outgoing, order.ID);

                if (lineItemsBySupplier.Count() <= 0)
                {
                    continue;
                }
                foreach (HSLineItem lineItem in lineItemsBySupplier)
                {

                    var lineItemWithPurchaseOrder = new LineItemsWithPurchaseOrderFields
                    {
                        ID = lineItem.ID,
                        OrderID = order.ID,
                        Subtotal = order.Subtotal,
                        Total = order.Total,
                        UnitPrice = lineItem.UnitPrice,
                        SupplierID = lineItem.SupplierID
                    };
                    result.Add(lineItemWithPurchaseOrder);
                }
            }
            return result;
        }

        private async Task<List<LineItemMiscReportFields>> BuildLineItemsMiscFields(List<HSLineItem> lineItems, HSOrderWorksheet orderWorksheet, string buyerName)
        {
            var lineItemsWithMiscFields = new List<LineItemMiscReportFields>();

            foreach (var lineItem in lineItems)
            {
                var lineItemSupplier = await _oc.Suppliers.GetAsync<HSSupplier>(lineItem.SupplierID);
                var lineItemWithMiscFields = new LineItemMiscReportFields
                {
                    ID = lineItem.ID,
                    SupplierName = lineItemSupplier?.Name,
                    BrandName = buyerName
                };

                if (orderWorksheet.OrderCalculateResponse != null && orderWorksheet.OrderCalculateResponse.xp != null && orderWorksheet.OrderCalculateResponse.xp.TaxCalculation.ExternalTransactionID != "NotTaxable")
                {
                    var lineTax = orderWorksheet.OrderCalculateResponse.xp.TaxCalculation.LineItems.FirstOrDefault(line => line.LineItemID == lineItem.ID);
                    lineItemWithMiscFields.Tax = lineTax?.LineItemTotalTax;
                    lineItemWithMiscFields.LineTaxAvailable = lineTax != null;
                }
                else
                {
                    lineItemWithMiscFields.Tax = null;
                    lineItemWithMiscFields.LineTaxAvailable = false;
                }
                lineItemsWithMiscFields.Add(lineItemWithMiscFields);
            }

            return lineItemsWithMiscFields;
        }
    }
}
