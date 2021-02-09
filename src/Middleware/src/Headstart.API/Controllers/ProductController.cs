using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Headstart.Models;
using Headstart.Models.Attributes;
using ordercloud.integrations.library;
using OrderCloud.SDK;
using System.Collections.Generic;
using Headstart.API.Controllers;
using Headstart.API.Commands.Crud;

namespace Headstart.Common.Controllers
{
	[DocComments("\"Products\" represents Products for Headstart")]
	[HSSection.Headstart(ListOrder = 3)]
	[Route("products")]
	public class ProductController : BaseController
	{

		private readonly IHSProductCommand _command;
		public ProductController(AppSettings settings, IHSProductCommand command) : base(settings)
		{
			_command = command;
		}

		[DocName("GET Super Product")]
		[HttpGet, Route("{id}"), OrderCloudIntegrationsAuth(ApiRole.ProductAdmin, ApiRole.ProductReader)]
		public async Task<SuperHSProduct> Get(string id)
		{
			return await _command.Get(id, VerifiedUserContext.AccessToken);
		}

		[DocName("LIST Super Product")]
		[HttpGet, OrderCloudIntegrationsAuth(ApiRole.ProductAdmin, ApiRole.ProductReader)]
		public async Task<ListPage<SuperHSProduct>> List(ListArgs<HSProduct> args)
		{
			return await _command.List(args, VerifiedUserContext.AccessToken);
		}

		[DocName("POST Super Product")]
		[HttpPost, OrderCloudIntegrationsAuth(ApiRole.ProductAdmin)]
		public async Task<SuperHSProduct> Post([FromBody] SuperHSProduct obj)
		{
			return await _command.Post(obj, VerifiedUserContext);
		}

		[DocName("PUT Super Product")]
		[HttpPut, Route("{id}"), OrderCloudIntegrationsAuth(ApiRole.ProductAdmin)]
		public async Task<SuperHSProduct> Put([FromBody] SuperHSProduct obj, string id)
		{
			return await _command.Put(id, obj, this.VerifiedUserContext.AccessToken);
		}

		[DocName("DELETE Product")]
		[HttpDelete, Route("{id}"), OrderCloudIntegrationsAuth(ApiRole.ProductAdmin)]
		public async Task Delete(string id)
		{
			await _command.Delete(id, VerifiedUserContext.AccessToken);
		}


		// todo add auth for seller user
		[DocName("GET Product pricing override")]
		[HttpGet, Route("{id}/pricingoverride/buyer/{buyerID}"), OrderCloudIntegrationsAuth(ApiRole.ProductAdmin)]
		public async Task<HSPriceSchedule> GetPricingOverride(string id, string buyerID)
		{
			return await _command.GetPricingOverride(id, buyerID, VerifiedUserContext.AccessToken);
		}

		// todo add auth for seller user
		[DocName("CREATE Product pricing override")]
		[HttpPost, Route("{id}/pricingoverride/buyer/{buyerID}"), OrderCloudIntegrationsAuth(ApiRole.ProductAdmin)]
		public async Task<HSPriceSchedule> CreatePricingOverride(string id, string buyerID, [FromBody] HSPriceSchedule priceSchedule)
		{
			return await _command.CreatePricingOverride(id, buyerID, priceSchedule, VerifiedUserContext.AccessToken);
		}

		// todo add auth for seller user
		[DocName("PUT Product pricing override")]
		[HttpPut, Route("{id}/pricingoverride/buyer/{buyerID}"), OrderCloudIntegrationsAuth(ApiRole.ProductAdmin)]
		public async Task<HSPriceSchedule> UpdatePricingOverride(string id, string buyerID, [FromBody] HSPriceSchedule priceSchedule)
		{
			return await _command.UpdatePricingOverride(id, buyerID, priceSchedule, VerifiedUserContext.AccessToken);
		}

		// todo add auth for seller user
		[DocName("DELETE Product pricing override")]
		[HttpDelete, Route("{id}/pricingoverride/buyer/{buyerID}"), OrderCloudIntegrationsAuth(ApiRole.ProductAdmin)]
		public async Task DeletePricingOverride(string id, string buyerID)
		{
			await _command.DeletePricingOverride(id, buyerID, VerifiedUserContext.AccessToken);
		}

		[DocName("PATCH Product filter option override")]
		[HttpPatch, Route("filteroptionoverride/{id}"), OrderCloudIntegrationsAuth(ApiRole.AdminUserAdmin)]
		public async Task<Product> FilterOptionOverride(string id, [FromBody] Product product)
        {
			IDictionary<string, object> facets = product.xp.Facets;
			var supplierID = product.DefaultSupplierID;
			return await _command.FilterOptionOverride(id, supplierID, facets, VerifiedUserContext);
        }
	}
}
