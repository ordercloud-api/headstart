using Headstart.Models;
using Microsoft.AspNetCore.Mvc;
using OrderCloud.SDK;
using System.Threading.Tasks;
using Headstart.Models.Attributes;
using ordercloud.integrations.library;
using Headstart.API.Controllers;
using Headstart.API.Commands;

namespace Headstart.Common.Controllers
{
    [DocComments("\"Buyers\" represents Buyers for Headstart")]
    [HSSection.Headstart(ListOrder = 1)]
    [Route("buyer")]
    public class BuyerController : BaseController
    {
        
        private readonly IHSBuyerCommand _command;
        private readonly IOrderCloudClient _oc;
        public BuyerController(IHSBuyerCommand command, IOrderCloudClient oc, AppSettings settings) : base(settings)
        {
            _command = command;
            _oc = oc;
        }

        [DocName("POST Headstart Buyer")]
        [HttpPost, OrderCloudIntegrationsAuth(ApiRole.BuyerAdmin)]
        public async Task<SuperHSBuyer> Create([FromBody] SuperHSBuyer buyer)
        {
            return await _command.Create(buyer, VerifiedUserContext.AccessToken);
        }

        [DocName("PUT Headstart Buyer")]
        [HttpPut, Route("{buyerID}"), OrderCloudIntegrationsAuth(ApiRole.BuyerAdmin)]
        public async Task<SuperHSBuyer> Put([FromBody] SuperHSBuyer superBuyer, string buyerID)
        {
            return await _command.Update(buyerID, superBuyer, VerifiedUserContext.AccessToken);
        }

        [DocName("GET Headstart Buyer")]
        [HttpGet, Route("{buyerID}"), OrderCloudIntegrationsAuth(ApiRole.BuyerAdmin)]
        public async Task<SuperHSBuyer> Get(string buyerID)
        {
            return await _command.Get(buyerID, VerifiedUserContext.AccessToken);
        }
    }
}
