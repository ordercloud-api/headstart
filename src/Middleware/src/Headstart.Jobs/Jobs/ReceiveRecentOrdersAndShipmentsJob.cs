using Headstart.Common.Models.Headstart;
using Headstart.Common.Repositories;
using Headstart.Jobs.Helpers;
using OrderCloud.Catalyst;
using OrderCloud.SDK;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Headstart.Jobs
{
	public class ReceiveRecentOrdersAndShipmentsJob : BaseReportJob
	{
		private readonly IOrderCloudClient _oc;
		private readonly IOrdersAndShipmentsDataRepo _ordersAndShipmentsDataRepo;

		public ReceiveRecentOrdersAndShipmentsJob(IOrderCloudClient oc, IOrdersAndShipmentsDataRepo ordersAndShipmentsDataRepo)
		{
			_oc = oc;
			_ordersAndShipmentsDataRepo = ordersAndShipmentsDataRepo;
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
				LogFailure($@"{ex.Message}. {ex?.InnerException?.Message} {ex.StackTrace}.");
				return ResultCode.PermanentFailure;
			}
		}

		private async Task UpsertOrderAndShipments(string orderID)
		{
			var orderWorksheet = await _oc.IntegrationEvents.GetWorksheetAsync<HsOrderWorksheet>(OrderDirection.Incoming, orderID);

			var shipments = await _oc.Shipments.ListAllAsync(orderID);

			foreach (var shipment in shipments)
			{
				var shipmentItems = await _oc.Shipments.ListAllItemsAsync(shipment.ID);

				foreach (var shipmentItem in shipmentItems)
				{
					var cosmosOrderWithShipments = OrderWithShipmentsMapper.Map(orderWorksheet, shipment, shipmentItem);

					if (cosmosOrderWithShipments.QuantityShipped != 0)
					{
						var queryable = _ordersAndShipmentsDataRepo.GetQueryable().Where(orderWithShipments => orderWithShipments.PartitionKey == "PartitionValue");
						var requestOptions = BuildQueryRequestOptions();
						var listOptions = BuildOrderWithShipmentsListOptions(orderID, shipmentItem.LineItemID, shipment.ID);
						var currentOrderWithShipmentsListPage = await _ordersAndShipmentsDataRepo.GetItemsAsync(queryable, requestOptions, listOptions);

						var cosmosId = string.Empty;
						if (currentOrderWithShipmentsListPage.Items.Count() == 1)
						{
							cosmosId = cosmosOrderWithShipments.id = currentOrderWithShipmentsListPage.Items[0].id;
						}
						await _ordersAndShipmentsDataRepo.UpsertItemAsync(cosmosId, cosmosOrderWithShipments);
					}
				}
			}
		}
	}
}