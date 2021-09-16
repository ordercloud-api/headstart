using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Headstart.Common.Models;
using Headstart.Common.Repositories;
using Headstart.Common.Services.ShippingIntegration.Models;
using Headstart.Jobs.Helpers;
using Headstart.Models;
using Headstart.Models.Headstart;
using Microsoft.Azure.Cosmos;
using ordercloud.integrations.library;
using OrderCloud.Catalyst;
using OrderCloud.SDK;

namespace Headstart.Jobs
{
    public class ReceiveRecentPurchaseOrdersJob : BaseReportJob
    {
        private readonly IOrderCloudClient _oc;
        private readonly IPurchaseOrderDetailDataRepo _purchaseOrderDetailDataRepo;

        public ReceiveRecentPurchaseOrdersJob(IOrderCloudClient oc, IPurchaseOrderDetailDataRepo purchaseOrderDetailDataRepo)
        {
            _oc = oc;
            _purchaseOrderDetailDataRepo = purchaseOrderDetailDataRepo;
        }

        protected override async Task<ResultCode> ProcessJobAsync(string message)
        {
            try
            {
                await UpsertPurchaseOrderDetail(message);
                return ResultCode.Success;
            }
            catch (Exception ex)
            {
                LogFailure($"{ex.Message} {ex?.InnerException?.Message} {ex.StackTrace}");
                return ResultCode.PermanentFailure;
            }
        }

        private async Task UpsertPurchaseOrderDetail(string orderID)
        {
            var orders = await _oc.Orders.ListAllAsync<HSOrder>(OrderDirection.Outgoing, filters: $"ID={orderID}-*");

            var queryable = _purchaseOrderDetailDataRepo.GetQueryable().Where(order => order.PartitionKey == "PartitionValue");

            var requestOptions = BuildQueryRequestOptions();

            var salesOrderWorksheet = await _oc.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.Incoming, orderID);

            var promos = await _oc.Orders.ListPromotionsAsync(OrderDirection.Incoming, salesOrderWorksheet.Order.ID);

            var discountedLineItems = new List<HSLineItem>();

            if (promos.Items.Count() > 0)
            {
                var discountedLineFilter = new Dictionary<string, object>
                {
                    ["PromotionDiscount"] = ">0"
                };

                discountedLineItems = await _oc.LineItems.ListAllAsync<HSLineItem>(OrderDirection.Incoming, salesOrderWorksheet.Order.ID, filters: discountedLineFilter);
            }

            foreach (var order in orders)
            {
                order.FromUser = salesOrderWorksheet.Order.FromUser;

                order.BillingAddress = new HSAddressBuyer()
                {
                    xp = new BuyerAddressXP()
                    {
                        LocationID = salesOrderWorksheet?.Order?.BillingAddress?.xp?.LocationID
                    }
                };

                var brand = await _oc.Buyers.GetAsync<HSBuyer>(salesOrderWorksheet.Order.FromCompanyID);
                var supplier = await _oc.Suppliers.GetAsync<HSSupplier>(order.ToCompanyID);

                order.ShippingCost = GetPurchaseOrderShippingCost(salesOrderWorksheet, order.ToCompanyID);
                if (salesOrderWorksheet.Order.PromotionDiscount > 0)
                {
                    order.PromotionDiscount = GetPurchaseOrderPromotionDiscount(salesOrderWorksheet, order.ToCompanyID);
                }

                var cosmosPurchaseOrder = new OrderDetailData()
                {
                    PartitionKey = "PartitionValue",
                    OrderID = order.ID,
                    Data = order,
                    SupplierName = supplier.Name,
                    BrandName = brand.Name,
                };

                if (promos.Items.Count > 0)
                {
                    cosmosPurchaseOrder.Promos = ReportPromoBuilder.BuildPromoFields(promos, ReportTypeEnum.PurchaseOrderDetail, order.ToCompanyID, discountedLineItems);
                }

                var listOptions = BuildListOptions(order.ID);

                CosmosListPage<OrderDetailData> currentOrderListPage = await _purchaseOrderDetailDataRepo.GetItemsAsync(queryable, requestOptions, listOptions);

                var cosmosID = "";
                if (currentOrderListPage.Items.Count() == 1)
                {
                    cosmosID = cosmosPurchaseOrder.id = currentOrderListPage.Items[0].id;
                }

                await _purchaseOrderDetailDataRepo.UpsertItemAsync(cosmosID, cosmosPurchaseOrder);
            }
        }

        private decimal GetPurchaseOrderShippingCost(HSOrderWorksheet orderWorksheet, string supplierID)
        {
            var supplierShipEstimate = orderWorksheet?.ShipEstimateResponse?.ShipEstimates?.FirstOrDefault(estimate => estimate?.xp?.SupplierID == supplierID);
            if (supplierShipEstimate == null)
            {
                return 0M;
            }

            if (orderWorksheet?.OrderCalculateResponse?.xp?.TaxResponse?.lines?.FirstOrDefault(line => line?.lineNumber == supplierShipEstimate?.SelectedShipMethodID) != null)
            {
                var shippingCost = orderWorksheet?.OrderCalculateResponse?.xp?.TaxResponse?.lines?.FirstOrDefault(line => line?.lineNumber == supplierShipEstimate?.SelectedShipMethodID)?.lineAmount;
                return shippingCost != null ? Math.Round((decimal)shippingCost, 2) : 0M;
            }

            return 0M;
        }

        private decimal GetPurchaseOrderPromotionDiscount(HSOrderWorksheet orderWorksheet, string supplierID)
        {
            var supplierLineItems = orderWorksheet?.LineItems?.Where(li => li?.SupplierID == supplierID);
            // Line-level discounts
            var lineItemDiscounts = supplierLineItems?.Sum(line => line?.PromotionDiscount);

            var supplierLineItemIDs = supplierLineItems?.Select(li => li?.ID);
            if (supplierLineItemIDs != null)
            {
                var orderCalculateResponseLines = orderWorksheet?.OrderCalculateResponse?.xp?.TaxResponse?.lines?.Where(avalaraLine => supplierLineItemIDs.Contains(avalaraLine?.lineNumber));
                // Order-level discounts, as it applies to the line item and adjusted for line item pricing
                var orderCalculateLineDiscounts = orderCalculateResponseLines?.Sum(line => line?.discountAmount);
                
                var totalDiscount = lineItemDiscounts += orderCalculateLineDiscounts;
                return totalDiscount != null ? (decimal)totalDiscount : 0M;
            }

            return 0M;
        }
    }
}
