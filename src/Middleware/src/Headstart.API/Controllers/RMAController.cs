using Microsoft.AspNetCore.Mvc;
using OrderCloud.SDK;
using System.Threading.Tasks;
using ordercloud.integrations.library;
using Headstart.Models.Attributes;
using Headstart.Common.Models;
using Headstart.Models.Misc;
using Headstart.API.Controllers;
using Headstart.API.Commands;

namespace Headstart.Common.Controllers
{
    [DocComments("\"Headstart RMAs\" for managing RMAs in the Headstart application")]
    [HSSection.Headstart(ListOrder = 12)]
    [Route("rma")]
    public class RMAController : BaseController
    {
        private readonly IRMACommand _rmaCommand;

        public RMAController(IRMACommand rmaCommand, AppSettings settings) : base(settings)
        {
            _rmaCommand = rmaCommand;
        }

        // Buyer Routes
        [DocName("POST Headstart RMA")]
        [HttpPost, OrderCloudIntegrationsAuth(ApiRole.Shopper)]
        public async Task<RMA> GenerateRMA([FromBody] RMA rma)
        {
            return await _rmaCommand.GenerateRMA(rma, VerifiedUserContext);
        }

        [DocName("LIST Me Headstart RMAs")]
        [HttpPost, Route("list/me"), OrderCloudIntegrationsAuth(ApiRole.Shopper)]
        public async Task<CosmosListPage<RMA>> ListMeRMAs([FromBody] CosmosListOptions listOptions)
        {
            return await _rmaCommand.ListMeRMAs(listOptions, VerifiedUserContext);
        }

        [DocName("LIST Buyer Headstart RMAs")]
        [HttpPost, Route("list/buyer"), OrderCloudIntegrationsAuth]
        public async Task<CosmosListPage<RMA>> ListBuyerRMAs([FromBody] CosmosListOptions listOptions)
        {
            RequireOneOf(CustomRole.MPLocationViewAllOrders);
            return await _rmaCommand.ListBuyerRMAs(listOptions, VerifiedUserContext);
        }

        // Seller/Supplier Routes (TO-DO)
    }
}
