using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Headstart.Models;
using Headstart.Models.Attributes;
using ordercloud.integrations.library;
using OrderCloud.SDK;
using System.Collections.Generic;
using Headstart.API.Commands.Crud;
using OrderCloud.Catalyst;

namespace Headstart.Common.Controllers
{
	[Route("products")]
	public class ProductController : CatalystController
	{

		private readonly IHSProductCommand _command;
		public ProductController(AppSettings settings, IHSProductCommand command)
		{
			_command = command;
		}
		/// <summary>
		/// GET Super Product
		/// </summary>
		[HttpGet, Route("{id}"), OrderCloudUserAuth(ApiRole.ProductAdmin, ApiRole.ProductReader)]
		public async Task<SuperHSProduct> Get(string id)
		{
			return await _command.Get(id, UserContext.AccessToken);
		}
		/// <summary>
		/// LIST Super Product
		/// </summary>
		[HttpGet, OrderCloudUserAuth(ApiRole.ProductAdmin, ApiRole.ProductReader)]
		public async Task<ListPage<SuperHSProduct>> List(ListArgs<HSProduct> args)
		{
			return await _command.List(args, UserContext.AccessToken);
		}
		/// <summary>
		/// POST Super Product
		/// </summary>
		[HttpPost, OrderCloudUserAuth(ApiRole.ProductAdmin)]
		public async Task<SuperHSProduct> Post([FromBody] SuperHSProduct obj)
		{
			return await _command.Post(obj, UserContext);
		}
		/// <summary>
		/// PUT Super Product
		/// </summary>
		[HttpPut, Route("{id}"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
		public async Task<SuperHSProduct> Put([FromBody] SuperHSProduct obj, string id)
		{
			return await _command.Put(id, obj, UserContext.AccessToken);
		}

		/// <summary>
		/// DELETE Product
		/// </summary>
		[HttpDelete, Route("{id}"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
		public async Task Delete(string id)
		{
			await _command.Delete(id, UserContext.AccessToken);
		}

		// todo add auth for seller user
		/// <summary>
		/// GET Product pricing override
		/// </summary>
		[HttpGet, Route("{id}/pricingoverride/buyer/{buyerID}"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
		public async Task<HSPriceSchedule> GetPricingOverride(string id, string buyerID)
		{
			return await _command.GetPricingOverride(id, buyerID, UserContext.AccessToken);
		}

		// todo add auth for seller user
		/// <summary>
		/// CREATE Product pricing override
		/// </summary>
		[HttpPost, Route("{id}/pricingoverride/buyer/{buyerID}"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
		public async Task<HSPriceSchedule> CreatePricingOverride(string id, string buyerID, [FromBody] HSPriceSchedule priceSchedule)
		{
			return await _command.CreatePricingOverride(id, buyerID, priceSchedule, UserContext.AccessToken);
		}

		// todo add auth for seller user
		/// <summary>
		/// PUT Product pricing override
		/// </summary>
		[HttpPut, Route("{id}/pricingoverride/buyer/{buyerID}"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
		public async Task<HSPriceSchedule> UpdatePricingOverride(string id, string buyerID, [FromBody] HSPriceSchedule priceSchedule)
		{
			return await _command.UpdatePricingOverride(id, buyerID, priceSchedule, UserContext.AccessToken);
		}

		// todo add auth for seller user
		/// <summary>
		/// DELETE Product pricing override
		/// </summary>
		[HttpDelete, Route("{id}/pricingoverride/buyer/{buyerID}"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
		public async Task DeletePricingOverride(string id, string buyerID)
		{
			await _command.DeletePricingOverride(id, buyerID, UserContext.AccessToken);
		}
		/// <summary>
		/// PATCH Product filter option override
		/// </summary>
		[HttpPatch, Route("filteroptionoverride/{id}"), OrderCloudUserAuth(ApiRole.AdminUserAdmin)]
		public async Task<Product> FilterOptionOverride(string id, [FromBody] Product product)
        {
			IDictionary<string, object> facets = product.xp.Facets;
			var supplierID = product.DefaultSupplierID;
			return await _command.FilterOptionOverride(id, supplierID, facets, UserContext);
        }
	}
}
