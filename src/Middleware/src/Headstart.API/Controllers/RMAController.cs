using OrderCloud.SDK;
using OrderCloud.Catalyst;
using Headstart.API.Commands;
using System.Threading.Tasks;
using Headstart.Common.Models;
using Microsoft.AspNetCore.Mvc;
using ordercloud.integrations.library;

namespace Headstart.Common.Controllers
{
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

        /// <summary>
        /// The IOC based constructor method for the RMAController with Dependency Injection
        /// </summary>
        /// <param name="rmaCommand"></param>
        /// <param name="lineItemCommand"></param>
        /// <param name="oc"></param>
        public RMAController(IRMACommand rmaCommand, ILineItemCommand lineItemCommand, IOrderCloudClient oc)
        {
            _rmaCommand = rmaCommand;
            _lineItemCommand = lineItemCommand;
            _oc = oc;
        }

        /// <summary>
        /// Post action for the RMA request (POST method)
        /// </summary>
        /// <param name="rma"></param>
        /// <returns>The response from the post action for the RMA request</returns>
        [HttpPost, OrderCloudUserAuth(ApiRole.Shopper)]
        public async Task<RMA> PostRMA([FromBody] RMA rma)
        {
            return await _rmaCommand.PostRMA(rma);
        }

        /// <summary>
        /// Post action for the BuyerRMAs request (POST method)
        /// </summary>
        /// <param name="listOptions"></param>
        /// <returns>The CosmosListPage of RMAs for a buyer request</returns>
        [HttpPost, Route("list/buyer"), OrderCloudUserAuth(HSLocationViewAllOrders)]
        public async Task<CosmosListPage<RMA>> ListBuyerRMAs([FromBody] CosmosListOptions listOptions)
        {
            var me = await _oc.Me.GetAsync(accessToken: UserContext.AccessToken);
            return await _rmaCommand.ListBuyerRMAs(listOptions, me?.Buyer?.ID);
        }

        /// <summary>
        /// Gets the RMA object from a ListArgs of RMA objects - Seller/Supplier Routes (GET method)
        /// </summary>
        /// <param name="args"></param>
        /// <returns>The RMA object from a ListArgs of RMA objects</returns>
        [HttpGet, OrderCloudUserAuth(HSOrderAdmin, HSOrderReader, HSShipmentAdmin)]
        public async Task<RMA> Get(ListArgs<RMA> args)
        {
            return await _rmaCommand.Get(args, UserContext);
        }

        /// <summary>
        /// Gets the CosmosListPage of RMAsByOrderID objects (GET method)
        /// </summary>
        /// <param name="orderID"></param>
        /// <param name="commerceRole"></param>
        /// <param name="me"></param>
        /// <param name="accessAllRMAsOnOrder"></param>
        /// <returns>The CosmosListPage of RMAsByOrderID objects</returns>
        [HttpGet, Route("{orderID}"), OrderCloudUserAuth(HSOrderAdmin, HSOrderReader, HSShipmentAdmin)]
        public async Task<CosmosListPage<RMA>> ListRMAsByOrderID(string orderID, CommerceRole commerceRole, MeUser me, bool accessAllRMAsOnOrder = false)
        {
            return await _rmaCommand.ListRMAsByOrderID(orderID, commerceRole, me, accessAllRMAsOnOrder);
        }

        /// <summary>
        /// Post action for the CosmosListPage of RMAs request (POST method)
        /// </summary>
        /// <param name="listOptions"></param>
        /// <returns>The response from the post action for the CosmosListPage of RMAs request</returns>
        [HttpPost, Route("list"), OrderCloudUserAuth(HSOrderAdmin, HSOrderReader, HSShipmentAdmin)]
        public async Task<CosmosListPage<RMA>> ListRMAs([FromBody] CosmosListOptions listOptions)
        {
            return await _rmaCommand.ListRMAs(listOptions, UserContext);
        }

        /// <summary>
        /// Update action for processing an RMA request (PUT method)
        /// </summary>
        /// <param name="rma"></param>
        /// <returns>The response from the update action for processing an RMA request</returns>
        [HttpPut, Route("process-rma"), OrderCloudUserAuth(ApiRole.ShipmentAdmin)]
        public async Task<RMA> ProcessRMA([FromBody] RMA rma)
        {
            RMAWithLineItemStatusByQuantity rmaWithLineItemStatusByQuantity = await _rmaCommand.ProcessRMA(rma, UserContext);
            await _lineItemCommand.HandleRMALineItemStatusChanges(OrderDirection.Incoming, rmaWithLineItemStatusByQuantity, UserContext);
            return rmaWithLineItemStatusByQuantity.RMA;
        }

        /// <summary>
        /// Post action for processing an RMA request (POST method)
        /// </summary>
        /// <param name="rmaNumber"></param>
        /// <returns>The response from the post action for processing an RMA request</returns>
        [HttpPost, Route("refund/{rmaNumber}"), OrderCloudUserAuth(ApiRole.ShipmentAdmin)]
        public async Task<RMA> ProcessRefund(string rmaNumber)
        {
            RMAWithLineItemStatusByQuantity rmaWithLineItemStatusByQuantity = await _rmaCommand.ProcessRefund(rmaNumber, UserContext);
            await _lineItemCommand.HandleRMALineItemStatusChanges(OrderDirection.Incoming, rmaWithLineItemStatusByQuantity, UserContext);
            return rmaWithLineItemStatusByQuantity.RMA;
        }
    }
}