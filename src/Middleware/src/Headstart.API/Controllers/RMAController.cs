﻿using System.Threading.Tasks;
using Headstart.API.Commands;
using Headstart.Common.Models;
using Microsoft.AspNetCore.Mvc;
using OrderCloud.Catalyst;
using OrderCloud.Integrations.Library;
using OrderCloud.SDK;

namespace Headstart.Common.Controllers
{
    /// <summary>
    /// Returns.
    /// </summary>
    [Route("rma")]
    public class RMAController : CatalystController
    {
        private const string HSLocationViewAllOrders = "HSLocationViewAllOrders";
        private const string HSOrderAdmin = "HSOrderAdmin";
        private const string HSOrderReader = "HSOrderReader";
        private const string HSShipmentAdmin = "HSShipmentAdmin";
        private readonly IRMACommand rmaCommand;
        private readonly ILineItemCommand lineItemCommand;
        private readonly IOrderCloudClient oc;

        public RMAController(IRMACommand rmaCommand, ILineItemCommand lineItemCommand, IOrderCloudClient oc)
        {
            this.rmaCommand = rmaCommand;
            this.lineItemCommand = lineItemCommand;
            this.oc = oc;
        }

        // Buyer Routes

        /// <summary>
        /// POST Headstart RMA.
        /// </summary>
        [HttpPost, OrderCloudUserAuth(ApiRole.Shopper)]
        public async Task<RMA> PostRMA([FromBody] RMA rma)
        {
            return await rmaCommand.PostRMA(rma);
        }

        [HttpPost, Route("list/buyer"), OrderCloudUserAuth(HSLocationViewAllOrders)]
        public async Task<CosmosListPage<RMA>> ListBuyerRMAs([FromBody] CosmosListOptions listOptions)
        {
            var me = await oc.Me.GetAsync(accessToken: UserContext.AccessToken);
            return await rmaCommand.ListBuyerRMAs(listOptions, me?.Buyer?.ID);
        }

        // Seller/Supplier Routes
        [HttpGet, OrderCloudUserAuth(HSOrderAdmin, HSOrderReader, HSShipmentAdmin)]
        public async Task<RMA> Get(ListArgs<RMA> args)
        {
            return await rmaCommand.Get(args, UserContext);
        }

        [HttpGet, Route("{orderID}"), OrderCloudUserAuth(HSOrderAdmin, HSOrderReader, HSShipmentAdmin)]
        public async Task<CosmosListPage<RMA>> ListRMAsByOrderID(string orderID, CommerceRole commerceRole, MeUser me, bool accessAllRMAsOnOrder = false)
        {
            return await rmaCommand.ListRMAsByOrderID(orderID, commerceRole, me, accessAllRMAsOnOrder);
        }

        [HttpPost, Route("list"), OrderCloudUserAuth(HSOrderAdmin, HSOrderReader, HSShipmentAdmin)]
        public async Task<CosmosListPage<RMA>> ListRMAs([FromBody] CosmosListOptions listOptions)
        {
            return await rmaCommand.ListRMAs(listOptions, UserContext);
        }

        [HttpPut, Route("process-rma"), OrderCloudUserAuth(ApiRole.ShipmentAdmin)]
        public async Task<RMA> ProcessRMA([FromBody] RMA rma)
        {
            RMAWithLineItemStatusByQuantity rmaWithLineItemStatusByQuantity = await rmaCommand.ProcessRMA(rma, UserContext);
            await lineItemCommand.HandleRMALineItemStatusChanges(OrderDirection.Incoming, rmaWithLineItemStatusByQuantity, UserContext);
            return rmaWithLineItemStatusByQuantity.RMA;
        }

        [HttpPost, Route("refund/{rmaNumber}"), OrderCloudUserAuth(ApiRole.ShipmentAdmin)]
        public async Task<RMA> ProcessRefund(string rmaNumber)
        {
            RMAWithLineItemStatusByQuantity rmaWithLineItemStatusByQuantity = await rmaCommand.ProcessRefund(rmaNumber, UserContext);
            await lineItemCommand.HandleRMALineItemStatusChanges(OrderDirection.Incoming, rmaWithLineItemStatusByQuantity, UserContext);
            return rmaWithLineItemStatusByQuantity.RMA;
        }
    }
}
