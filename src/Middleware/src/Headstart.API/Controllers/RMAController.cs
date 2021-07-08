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
    public class RMAController : BaseController
    {
		private readonly IRMACommand _rmaCommand;
        private readonly ILineItemCommand _lineItemCommand;
        private const string HSLocationViewAllOrders = "HSLocationViewAllOrders";
        private const string HSOrderAdmin = "HSOrderAdmin";
        private const string HSOrderReader = "HSOrderReader";
        private const string HSShipmentAdmin = "HSShipmentAdmin";

        public RMAController(IRMACommand rmaCommand, ILineItemCommand lineItemCommand, AppSettings settings)
        {
            _rmaCommand = rmaCommand;
            _lineItemCommand = lineItemCommand;
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

        [DocName("LIST Buyer Headstart RMAs")]
        [HttpPost, Route("list/buyer"), OrderCloudUserAuth(HSLocationViewAllOrders)]
        public async Task<CosmosListPage<RMA>> ListBuyerRMAs([FromBody] CosmosListOptions listOptions)
        {
            return await _rmaCommand.ListBuyerRMAs(listOptions, UserContext.Buyer.ID);
        }

        // Seller/Supplier Routes
        [DocName("GET a single RMA")]
        [HttpGet, OrderCloudUserAuth(HSOrderAdmin, HSOrderReader, HSShipmentAdmin)]
        public async Task<RMA> Get(ListArgs<RMA> args)
        {
            return await _rmaCommand.Get(args, UserContext);
        }

        [DocName("LIST RMAs by Order ID")]
        [HttpGet, Route("{orderID}"), OrderCloudUserAuth(HSOrderAdmin, HSOrderReader, HSShipmentAdmin)]
        public async Task<CosmosListPage<RMA>> ListRMAsByOrderID(string orderID)
        {
            return await _rmaCommand.ListRMAsByOrderID(orderID, UserContext);
        }

        [DocName("LIST Seller/Supplier Headstart RMAs")]
        [HttpPost, Route("list"), OrderCloudUserAuth(HSOrderAdmin, HSOrderReader, HSShipmentAdmin)]
        public async Task<CosmosListPage<RMA>> ListRMAs([FromBody] CosmosListOptions listOptions)
        {
            return await _rmaCommand.ListRMAs(listOptions, UserContext);
        }

        [DocName("PUT Supplier Headstart RMA")]
        [HttpPut, Route("process-rma"), OrderCloudUserAuth(ApiRole.ShipmentAdmin)]
        public async Task<RMA> ProcessRMA([FromBody] RMA rma)
        {
            RMAWithLineItemStatusByQuantity rmaWithLineItemStatusByQuantity = await _rmaCommand.ProcessRMA(rma, UserContext);
            await _lineItemCommand.HandleRMALineItemStatusChanges(OrderDirection.Incoming, rmaWithLineItemStatusByQuantity, UserContext);
            return rmaWithLineItemStatusByQuantity.RMA;
        }

        [DocName("POST RMA Refund")]
        [HttpPost, Route("refund/{rmaNumber}"), OrderCloudUserAuth(ApiRole.ShipmentAdmin)]
        public async Task<RMA> ProcessRefund(string rmaNumber)
        {
            RMAWithLineItemStatusByQuantity rmaWithLineItemStatusByQuantity = await _rmaCommand.ProcessRefund(rmaNumber, UserContext);
            await _lineItemCommand.HandleRMALineItemStatusChanges(OrderDirection.Incoming, rmaWithLineItemStatusByQuantity, UserContext);
            return rmaWithLineItemStatusByQuantity.RMA;
        }
    }
}
