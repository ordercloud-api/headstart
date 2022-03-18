using Microsoft.Azure.Cosmos;
using ordercloud.integrations.library;
using OrderCloud.Catalyst;

namespace Headstart.Jobs
{
	public abstract class BaseReportJob : BaseQueueJob<string>
	{
		protected CosmosListOptions BuildListOptions(string orderId)
		{
			var currentOrderFilter = new ListFilter(@"OrderID", orderId);
			return new CosmosListOptions()
			{
				PageSize = 1,
				ContinuationToken = null,
				Filters =
				{
					currentOrderFilter
				}
			};
		}

		protected CosmosListOptions BuildOrderWithShipmentsListOptions(string orderId, string lineItemId, string shipmentId)
		{
			var currentOrderFilter = new ListFilter(@"OrderID", orderId);
			var currentLineItemFilter = new ListFilter(@"LineItemID", lineItemId);
			var currentShipmentFilter = new ListFilter(@"ShipmentID", shipmentId);
			return new CosmosListOptions()
			{
				PageSize = 1,
				ContinuationToken = null,
				Filters =
				{
					currentOrderFilter, 
					currentLineItemFilter, 
					currentShipmentFilter
				}
			};
		}

		protected QueryRequestOptions BuildQueryRequestOptions()
		{
			return new QueryRequestOptions()
			{
				MaxItemCount = 1
			};
		}
	}
}
