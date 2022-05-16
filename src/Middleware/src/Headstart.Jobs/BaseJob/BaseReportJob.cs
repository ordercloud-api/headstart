﻿using Microsoft.Azure.Cosmos;
using OrderCloud.Catalyst;
using OrderCloud.Integrations.Library;

namespace Headstart.Jobs
{
    public abstract class BaseReportJob : BaseQueueJob<string>
    {
        protected CosmosListOptions BuildListOptions(string orderID)
        {
            var currentOrderFilter = new ListFilter("OrderID", orderID);
            return new CosmosListOptions()
            {
                PageSize = 1,
                ContinuationToken = null,
                Filters = { currentOrderFilter },
            };
        }

        protected CosmosListOptions BuildOrderWithShipmentsListOptions(string orderID, string lineItemID, string shipmentID)
        {
            var currentOrderFilter = new ListFilter("OrderID", orderID);
            var currentLineItemFilter = new ListFilter("LineItemID", lineItemID);
            var currentShipmentFilter = new ListFilter("ShipmentID", shipmentID);
            return new CosmosListOptions()
            {
                PageSize = 1,
                ContinuationToken = null,
                Filters = { currentOrderFilter, currentLineItemFilter, currentShipmentFilter },
            };
        }

        protected QueryRequestOptions BuildQueryRequestOptions()
        {
            return new QueryRequestOptions()
            {
                MaxItemCount = 1,
            };
        }
    }
}
