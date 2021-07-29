using Headstart.Models;
using Microsoft.AspNetCore.Mvc;
using OrderCloud.SDK;
using System.Threading.Tasks;
using Headstart.Models.Attributes;
using System.Collections.Generic;
using ordercloud.integrations.library;
using Headstart.API.Commands;
using OrderCloud.Catalyst;

namespace Headstart.API.Controllers
{
    //[DocComments("\"Headstart Storefronts\" for handling deploying new sites automatically for storefronts")]
    [Route("storefronts")]
    public class StorefrontsController : BaseController
    {
        private readonly IStorefrontCommand _command;

        public StorefrontsController(IStorefrontCommand command)
        {
            _command = command;
        }

        //[DocName("Deploy a Storefront")]
        //[DocComments("Automatically deploys a a new storefront instance via users azure storage account")]
        [HttpPost, Route("deploy")]
        public async Task DeployStoreFront([FromBody] ApiClient client)
        {
            await _command.DeployBuyerSite(client);
        }
    }
}
