using System;
using System.Linq;
using System.Threading.Tasks;
using Headstart.Common.Models;
using Headstart.Common.Repositories;
using Headstart.Jobs.Helpers;
using OrderCloud.Integrations.Library;
using OrderCloud.SDK;

namespace Headstart.Jobs
{
    public class ReceiveRecentSalesOrdersJob : BaseReportJob
    {
        private readonly IOrderCloudClient oc;
        private readonly ISalesOrderDetailDataRepo salesOrderDetailDataRepo;

        public ReceiveRecentSalesOrdersJob(IOrderCloudClient oc, ISalesOrderDetailDataRepo salesOrderDetailDataRepo)
        {
            this.oc = oc;
            this.salesOrderDetailDataRepo = salesOrderDetailDataRepo;
        }

        protected override async Task<ResultCode> ProcessJobAsync(string message)
        {
            try
            {
                await UpsertSalesOrderDetail(message);
                return ResultCode.Success;
            }
            catch (Exception ex)
            {
                LogFailure($"{ex.Message} {ex?.InnerException?.Message} {ex.StackTrace}");
                return ResultCode.PermanentFailure;
            }
        }

        private async Task UpsertSalesOrderDetail(string orderID)
        {
            var order = await oc.Orders.GetAsync<HSOrder>(OrderDirection.Incoming, orderID);

            var brand = await oc.Buyers.GetAsync<HSBuyer>(order.FromCompanyID);

            var promos = await oc.Orders.ListPromotionsAsync(OrderDirection.Incoming, orderID);

            var cosmosSalesOrder = new OrderDetailData()
            {
                PartitionKey = "PartitionValue",
                OrderID = orderID,
                Data = order,
                BrandName = brand.Name,
            };

            if (promos.Items.Count > 0)
            {
                cosmosSalesOrder.Promos = ReportPromoBuilder.BuildPromoFields(promos, ReportTypeEnum.SalesOrderDetail);
            }

            var queryable = salesOrderDetailDataRepo.GetQueryable().Where(order => order.PartitionKey == "PartitionValue");

            var requestOptions = BuildQueryRequestOptions();

            var listOptions = BuildListOptions(orderID);

            CosmosListPage<OrderDetailData> currentOrderListPage = await salesOrderDetailDataRepo.GetItemsAsync(queryable, requestOptions, listOptions);

            var cosmosID = string.Empty;
            if (currentOrderListPage.Items.Count() == 1)
            {
                cosmosID = cosmosSalesOrder.id = currentOrderListPage.Items[0].id;
            }

            await salesOrderDetailDataRepo.UpsertItemAsync(cosmosID, cosmosSalesOrder);
        }
    }
}
