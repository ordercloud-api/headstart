using System.Threading.Tasks;
using Headstart.Common.Commands;
using Headstart.Common.Models;
using Microsoft.AspNetCore.Mvc;
using OrderCloud.Catalyst;
using OrderCloud.SDK;

namespace Headstart.Common.Controllers
{
    /// <summary>
    /// Buyers for Headstart.
    /// </summary>
    [Route("buyer")]
    public class BuyerController : CatalystController
    {
        private readonly IHSBuyerCommand command;
        private readonly IOrderCloudClient oc;

        public BuyerController(IHSBuyerCommand command, IOrderCloudClient oc)
        {
            this.command = command;
            this.oc = oc;
        }

        /// <summary>
        /// POST Headstart Buyer.
        /// </summary>
        [HttpPost, OrderCloudUserAuth(ApiRole.BuyerAdmin)]
        public async Task<SuperHSBuyer> Create([FromBody] SuperHSBuyer buyer)
        {
            return await command.Create(buyer);
        }

        /// <summary>
        /// PUT Headstart Buyer.
        /// </summary>
        [HttpPut, Route("{buyerID}"), OrderCloudUserAuth(ApiRole.BuyerAdmin)]
        public async Task<SuperHSBuyer> Put([FromBody] SuperHSBuyer superBuyer, string buyerID)
        {
            return await command.Update(buyerID, superBuyer);
        }

        /// <summary>
        /// GET Headstart Buyer".
        /// </summary>
        [HttpGet, Route("{buyerID}"), OrderCloudUserAuth(ApiRole.BuyerAdmin)]
        public async Task<SuperHSBuyer> Get(string buyerID)
        {
            return await command.Get(buyerID);
        }
    }
}
