using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Headstart.Models;
using Headstart.Models.Attributes;
using ordercloud.integrations.library;
using OrderCloud.SDK;
using System.Collections.Generic;
using Headstart.API.Commands.Crud;
using OrderCloud.Catalyst;
using Headstart.Common.Services.CMS.Models;

namespace Headstart.Common.Controllers
{
	[DocComments("\"Products\" represents Products for Headstart")]
	[HSSection.Headstart(ListOrder = 3)]
	[Route("products")]
	public class ProductController : BaseController
	{

		private readonly IHSProductCommand _command;
		public ProductController(AppSettings settings, IHSProductCommand command)
		{
			_command = command;
		}

		[DocName("GET Super Product")]
		[HttpGet, Route("{id}"), OrderCloudUserAuth(ApiRole.ProductAdmin, ApiRole.ProductReader)]
		public async Task<SuperHSProduct> Get(string id)
		{
			return await _command.Get(id, UserContext.AccessToken);
		}

		[DocName("LIST Super Product")]
		[HttpGet, OrderCloudUserAuth(ApiRole.ProductAdmin, ApiRole.ProductReader)]
		public async Task<ListPage<SuperHSProduct>> List(ListArgs<HSProduct> args)
		{
			return await _command.List(args, UserContext.AccessToken);
		}

		[DocName("POST Super Product")]
		[HttpPost, OrderCloudUserAuth(ApiRole.ProductAdmin)]
		public async Task<SuperHSProduct> Post([FromBody] SuperHSProduct obj)
		{
			return await _command.Post(obj, UserContext);
		}

		[DocName("PUT Super Product")]
		[HttpPut, Route("{id}"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
		public async Task<SuperHSProduct> Put([FromBody] SuperHSProduct obj, string id)
		{
			return await _command.Put(id, obj, UserContext.AccessToken);
		}

        [DocName("PUT Product Image")]
        [HttpPut, Route("images/{id}"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
        public async Task Put([FromForm] AssetUpload asset, string id)
        {
            //return await _command.Put(id, obj, UserContext.AccessToken);
            await _command.SaveProductImage(asset);
        }

        [DocName("DELETE Product")]
		[HttpDelete, Route("{id}"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
		public async Task Delete(string id)
		{
			await _command.Delete(id, UserContext.AccessToken);
		}


		// todo add auth for seller user
		[DocName("GET Product pricing override")]
		[HttpGet, Route("{id}/pricingoverride/buyer/{buyerID}"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
		public async Task<HSPriceSchedule> GetPricingOverride(string id, string buyerID)
		{
			return await _command.GetPricingOverride(id, buyerID, UserContext.AccessToken);
		}

		// todo add auth for seller user
		[DocName("CREATE Product pricing override")]
		[HttpPost, Route("{id}/pricingoverride/buyer/{buyerID}"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
		public async Task<HSPriceSchedule> CreatePricingOverride(string id, string buyerID, [FromBody] HSPriceSchedule priceSchedule)
		{
			return await _command.CreatePricingOverride(id, buyerID, priceSchedule, UserContext.AccessToken);
		}

		// todo add auth for seller user
		[DocName("PUT Product pricing override")]
		[HttpPut, Route("{id}/pricingoverride/buyer/{buyerID}"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
		public async Task<HSPriceSchedule> UpdatePricingOverride(string id, string buyerID, [FromBody] HSPriceSchedule priceSchedule)
		{
			return await _command.UpdatePricingOverride(id, buyerID, priceSchedule, UserContext.AccessToken);
		}

		// todo add auth for seller user
		[DocName("DELETE Product pricing override")]
		[HttpDelete, Route("{id}/pricingoverride/buyer/{buyerID}"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
		public async Task DeletePricingOverride(string id, string buyerID)
		{
			await _command.DeletePricingOverride(id, buyerID, UserContext.AccessToken);
		}

		[DocName("PATCH Product filter option override")]
		[HttpPatch, Route("filteroptionoverride/{id}"), OrderCloudUserAuth(ApiRole.AdminUserAdmin)]
		public async Task<Product> FilterOptionOverride(string id, [FromBody] Product product)
        {
			IDictionary<string, object> facets = product.xp.Facets;
			var supplierID = product.DefaultSupplierID;
			return await _command.FilterOptionOverride(id, supplierID, facets, UserContext);
        }
	}
}
