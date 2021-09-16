using Microsoft.AspNetCore.Mvc;
using OrderCloud.SDK;
using System.Threading.Tasks;
using ordercloud.integrations.library;
using Headstart.Models.Attributes;
using Headstart.Common.Models;
using Headstart.Models.Misc;
using Headstart.API.Commands;
using OrderCloud.Catalyst;

namespace Headstart.Common.Controllers
{
    /// <summary>
    /// Returns
    /// </summary>
    [Route("rma")]
    public class RMAController : CatalystController
    {
		private readonly IRMACommand _rmaCommand;
        private readonly ILineItemCommand _lineItemCommand;
        private readonly IOrderCloudClient _oc;
        private const string HSLocationViewAllOrders = "HSLocationViewAllOrders";
        private const string HSOrderAdmin = "HSOrderAdmin";
        private const string HSOrderReader = "HSOrderReader";
        private const string HSShipmentAdmin = "HSShipmentAdmin";

        public RMAController(IRMACommand rmaCommand, ILineItemCommand lineItemCommand, IOrderCloudClient oc)
        {
            _rmaCommand = rmaCommand;
            _lineItemCommand = lineItemCommand;
            _oc = oc;
        }

        // Buyer Routes
        /// <summary>
        /// POST Headstart RMA
        /// </summary>
        [HttpPost, OrderCloudUserAuth(ApiRole.Shopper)]
        public async Task<RMA> PostRMA([FromBody] RMA rma)
        {
            return await _rmaCommand.PostRMA(rma);
        }

        [HttpPost, Route("list/buyer"), OrderCloudUserAuth(HSLocationViewAllOrders)]
        public async Task<CosmosListPage<RMA>> ListBuyerRMAs([FromBody] CosmosListOptions listOptions)
        {
            var me = await _oc.Me.GetAsync(accessToken: UserContext.AccessToken);
            return await _rmaCommand.ListBuyerRMAs(listOptions, me?.Buyer?.ID);
        }

        // Seller/Supplier Routes
        [HttpGet, OrderCloudUserAuth(HSOrderAdmin, HSOrderReader, HSShipmentAdmin)]
        public async Task<RMA> Get(ListArgs<RMA> args)
        {
            return await _rmaCommand.Get(args, UserContext);
        }

        [HttpGet, Route("{orderID}"), OrderCloudUserAuth(HSOrderAdmin, HSOrderReader, HSShipmentAdmin)]
        public async Task<CosmosListPage<RMA>> ListRMAsByOrderID(string orderID, CommerceRole commerceRole, MeUser me, bool accessAllRMAsOnOrder = false)
        {
            return await _rmaCommand.ListRMAsByOrderID(orderID, commerceRole, me, accessAllRMAsOnOrder);
        }

        [HttpPost, Route("list"), OrderCloudUserAuth(HSOrderAdmin, HSOrderReader, HSShipmentAdmin)]
        public async Task<CosmosListPage<RMA>> ListRMAs([FromBody] CosmosListOptions listOptions)
        {
            return await _rmaCommand.ListRMAs(listOptions, UserContext);
        }

        [HttpPut, Route("process-rma"), OrderCloudUserAuth(ApiRole.ShipmentAdmin)]
        public async Task<RMA> ProcessRMA([FromBody] RMA rma)
        {
            RMAWithLineItemStatusByQuantity rmaWithLineItemStatusByQuantity = await _rmaCommand.ProcessRMA(rma, UserContext);
            await _lineItemCommand.HandleRMALineItemStatusChanges(OrderDirection.Incoming, rmaWithLineItemStatusByQuantity, UserContext);
            return rmaWithLineItemStatusByQuantity.RMA;
        }

        [HttpPost, Route("refund/{rmaNumber}"), OrderCloudUserAuth(ApiRole.ShipmentAdmin)]
        public async Task<RMA> ProcessRefund(string rmaNumber)
        {
            RMAWithLineItemStatusByQuantity rmaWithLineItemStatusByQuantity = await _rmaCommand.ProcessRefund(rmaNumber, UserContext);
            await _lineItemCommand.HandleRMALineItemStatusChanges(OrderDirection.Incoming, rmaWithLineItemStatusByQuantity, UserContext);
            return rmaWithLineItemStatusByQuantity.RMA;
        }
    }
}
