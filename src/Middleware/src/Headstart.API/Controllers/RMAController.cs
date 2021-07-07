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
        private const string HSLocationViewAllOrders = "HSLocationViewAllOrders";

        public RMAController(IRMACommand rmaCommand)
        {
            _rmaCommand = rmaCommand;
        }

        // Buyer Routes
        /// <summary>
        /// POST Headstart RMA
        /// </summary>
        [HttpPost, OrderCloudUserAuth(ApiRole.Shopper)]
        public async Task<RMA> GenerateRMA([FromBody] RMA rma)
        {
            return await _rmaCommand.GenerateRMA(rma, UserContext);
        }
        /// <summary>
        /// LIST Me Headstart RMAs
        /// </summary>
        [HttpPost, Route("list/me"), OrderCloudUserAuth(ApiRole.Shopper)]
        public async Task<CosmosListPage<RMA>> ListMeRMAs([FromBody] CosmosListOptions listOptions)
        {
            return await _rmaCommand.ListMeRMAs(listOptions, UserContext);
        }
        /// <summary>
        /// LIST Buyer Headstart RMAs
        /// </summary>
        [HttpPost, Route("list/buyer"), OrderCloudUserAuth(HSLocationViewAllOrders)]
        public async Task<CosmosListPage<RMA>> ListBuyerRMAs([FromBody] CosmosListOptions listOptions)
        {
            return await _rmaCommand.ListBuyerRMAs(listOptions, UserContext);
        }

        // Seller/Supplier Routes (TO-DO)
    }
}
