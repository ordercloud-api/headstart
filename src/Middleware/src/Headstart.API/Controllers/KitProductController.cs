using Microsoft.AspNetCore.Mvc;
using OrderCloud.SDK;
using System.Threading.Tasks;
using Headstart.Models.Attributes;
using ordercloud.integrations.library;
using Headstart.Models;
using Headstart.Common.Services.CMS.Models;
using Headstart.API.Controllers;
using Headstart.API.Commands.Crud;

namespace Headstart.Common.Controllers
{
    [DocComments("\"Kit Products\" represents Kit Products for Headstart")]
    [HSSection.Headstart(ListOrder = 3)]
    [Route("kitproducts")]
    public class KitProductController : BaseController
    {
        private readonly IHSKitProductCommand _command;
        public KitProductController(AppSettings settings, IHSKitProductCommand command) : base(settings)
        {
            _command = command;
        }

        //[DocName("Get Kit Product")]
        //[HttpGet, Route("{id}"), OrderCloudIntegrationsAuth(ApiRole.ProductAdmin)]
        //public async Task<HSKitProduct> Get(string id)
        //{
        //    return await _command.Get(id, VerifiedUserContext.AccessToken);
        //}
        
        //[DocName("Create Kit Product")]
        //[HttpPost, OrderCloudIntegrationsAuth(ApiRole.ProductAdmin)]
        //public async Task<HSKitProduct> Create([FromBody] HSKitProduct kitProduct)
        //{
        //    return await _command.Post(kitProduct, VerifiedUserContext.AccessToken);
        //}
        //[DocName("List Kit Products")]
        //[HttpGet, OrderCloudIntegrationsAuth(ApiRole.ProductAdmin)]
        //public async Task<ListPage<HSKitProduct>> List(ListArgs<Document<HSKitProductAssignment>> args)
        //{
        //    return await _command.List(args, VerifiedUserContext.AccessToken);
        //}
        //[DocName("List Me Kit Products")]
        //[HttpGet, Route("me"), OrderCloudIntegrationsAuth(ApiRole.Shopper)]
        //public async Task<ListPage<HSKitProduct>> ListMeKits(ListArgs<Document<HSKitProductAssignment>> args)
        //{
        //    return await _command.List(args, VerifiedUserContext.AccessToken);
        //}
        //[DocName("Save Kit Product")]
        //[HttpPut, Route("{id}"), OrderCloudIntegrationsAuth(ApiRole.ProductAdmin)]
        //public async Task<HSKitProduct> Save([FromBody] HSKitProduct kitProduct, string id)
        //{
        //    return await _command.Put(id, kitProduct, VerifiedUserContext.AccessToken);
        //}

        //[DocName("Delete Kit Product")]
        //[HttpDelete, Route("{id}"), OrderCloudIntegrationsAuth(ApiRole.ProductAdmin)]
        //public async Task Delete(string id)
        //{
        //    await _command.Delete(id, VerifiedUserContext.AccessToken);
        //}


    }
}
