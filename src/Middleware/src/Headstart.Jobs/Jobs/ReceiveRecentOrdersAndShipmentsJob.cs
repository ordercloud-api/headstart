using Headstart.Common.Models;
using Headstart.Common.Repositories;
using Headstart.Common.Services.ShippingIntegration.Models;
using Headstart.Jobs.Helpers;
using ordercloud.integrations.library;
using OrderCloud.Catalyst;
using OrderCloud.SDK;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Headstart.Jobs
{
    public class ReceiveRecentOrdersAndShipmentsJob : BaseReportJob
    {
        private readonly IOrderCloudClient oc;
        private readonly IOrdersAndShipmentsDataRepo ordersAndShipmentsDataRepo;

        public ReceiveRecentOrdersAndShipmentsJob(IOrderCloudClient oc, IOrdersAndShipmentsDataRepo ordersAndShipmentsDataRepo)
        {
            this.oc = oc;
            this.ordersAndShipmentsDataRepo = ordersAndShipmentsDataRepo;
        }

        protected override async Task<ResultCode> ProcessJobAsync(string message)
        {
            try
            {
                await UpsertOrderAndShipments(message);
                return ResultCode.Success;
            }
            catch (Exception ex)
            {
                LogFailure($"{ex.Message} {ex?.InnerException?.Message} {ex.StackTrace}");
                return ResultCode.PermanentFailure;
            }
        }

        private async Task UpsertOrderAndShipments(string orderID)
        {
            var orderWorksheet = await oc.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.Incoming, orderID);

            var shipments = await oc.Shipments.ListAllAsync(orderID);

            foreach (var shipment in shipments)
            {
                var shipmentItems = await oc.Shipments.ListAllItemsAsync(shipment.ID);

                foreach (var shipmentItem in shipmentItems)
                {
                    var cosmosOrderWithShipments = OrderWithShipmentsMapper.Map(orderWorksheet, shipment, shipmentItem);

                    if (cosmosOrderWithShipments.QuantityShipped != 0)
                    {
                        var queryable = ordersAndShipmentsDataRepo.GetQueryable().Where(orderWithShipments => orderWithShipments.PartitionKey == "PartitionValue");

                        var requestOptions = BuildQueryRequestOptions();

                        var listOptions = BuildOrderWithShipmentsListOptions(orderID, shipmentItem.LineItemID, shipment.ID);

                        CosmosListPage<OrderWithShipments> currentOrderWithShipmentsListPage = await ordersAndShipmentsDataRepo.GetItemsAsync(queryable, requestOptions, listOptions);

                        var cosmosID = string.Empty;
                        if (currentOrderWithShipmentsListPage.Items.Count() == 1)
                        {
                            cosmosID = cosmosOrderWithShipments.id = currentOrderWithShipmentsListPage.Items[0].id;
                        }

                        await ordersAndShipmentsDataRepo.UpsertItemAsync(cosmosID, cosmosOrderWithShipments);
                    }
                }
            }
        }
    }
}
