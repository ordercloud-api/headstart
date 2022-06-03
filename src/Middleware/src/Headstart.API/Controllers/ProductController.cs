using System.Collections.Generic;
using System.Threading.Tasks;
using Headstart.API.Commands.Crud;
using Headstart.Common.Models;
using Microsoft.AspNetCore.Mvc;
using OrderCloud.Catalyst;
using OrderCloud.SDK;

namespace Headstart.Common.Controllers
{
    [Route("products")]
    public class ProductController : CatalystController
    {
        private readonly IHSProductCommand command;

        public ProductController(IHSProductCommand command)
        {
            this.command = command;
        }

        /// <summary>
        /// GET Super Product.
        /// </summary>
        [HttpGet, Route("{id}"), OrderCloudUserAuth(ApiRole.ProductAdmin, ApiRole.ProductReader)]
        public async Task<SuperHSProduct> Get(string id)
        {
            return await command.Get(id, UserContext.AccessToken);
        }

        /// <summary>
        /// LIST Super Product.
        /// </summary>
        [HttpGet, OrderCloudUserAuth(ApiRole.ProductAdmin, ApiRole.ProductReader)]
        public async Task<ListPage<SuperHSProduct>> List(ListArgs<HSProduct> args)
        {
            return await command.List(args, UserContext.AccessToken);
        }

        /// <summary>
        /// POST Super Product.
        /// </summary>
        [HttpPost, OrderCloudUserAuth(ApiRole.ProductAdmin)]
        public async Task<SuperHSProduct> Post([FromBody] SuperHSProduct obj)
        {
            return await command.Post(obj, UserContext);
        }

        /// <summary>
        /// PUT Super Product.
        /// </summary>
        [HttpPut, Route("{id}"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
        public async Task<SuperHSProduct> Put([FromBody] SuperHSProduct obj, string id)
        {
            return await command.Put(id, obj, UserContext.AccessToken);
        }

        /// <summary>
        /// DELETE Product.
        /// </summary>
        [HttpDelete, Route("{id}"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
        public async Task Delete(string id)
        {
            await command.Delete(id, UserContext.AccessToken);
        }

        // todo add auth for seller user

        /// <summary>
        /// GET Product pricing override.
        /// </summary>
        [HttpGet, Route("{id}/pricingoverride/buyer/{buyerID}"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
        public async Task<HSPriceSchedule> GetPricingOverride(string id, string buyerID)
        {
            return await command.GetPricingOverride(id, buyerID, UserContext.AccessToken);
        }

        // todo add auth for seller user

        /// <summary>
        /// CREATE Product pricing override.
        /// </summary>
        [HttpPost, Route("{id}/pricingoverride/buyer/{buyerID}"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
        public async Task<HSPriceSchedule> CreatePricingOverride(string id, string buyerID, [FromBody] HSPriceSchedule priceSchedule)
        {
            return await command.CreatePricingOverride(id, buyerID, priceSchedule, UserContext.AccessToken);
        }

        // todo add auth for seller user

        /// <summary>
        /// PUT Product pricing override.
        /// </summary>
        [HttpPut, Route("{id}/pricingoverride/buyer/{buyerID}"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
        public async Task<HSPriceSchedule> UpdatePricingOverride(string id, string buyerID, [FromBody] HSPriceSchedule priceSchedule)
        {
            return await command.UpdatePricingOverride(id, buyerID, priceSchedule, UserContext.AccessToken);
        }

        // todo add auth for seller user

        /// <summary>
        /// DELETE Product pricing override.
        /// </summary>
        [HttpDelete, Route("{id}/pricingoverride/buyer/{buyerID}"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
        public async Task DeletePricingOverride(string id, string buyerID)
        {
            await command.DeletePricingOverride(id, buyerID, UserContext.AccessToken);
        }

        /// <summary>
        /// PATCH Product filter option override.
        /// </summary>
        [HttpPatch, Route("filteroptionoverride/{id}"), OrderCloudUserAuth(ApiRole.AdminUserAdmin)]
        public async Task<Product> FilterOptionOverride(string id, [FromBody] Product product)
        {
            IDictionary<string, object> facets = product.xp.Facets;
            var supplierID = product.DefaultSupplierID;
            return await command.FilterOptionOverride(id, supplierID, facets, UserContext);
        }
    }
}
