using Headstart.API.Commands;
using Headstart.Common;
using Headstart.Models;
using Microsoft.AspNetCore.Mvc;
using OrderCloud.Catalyst;
using OrderCloud.SDK;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Headstart.API.Controllers
{
	[Route("products")]
	public class ProductController : CatalystController
	{
		private readonly IHSProductCommand _command;
		private readonly AppSettings _settings;

		/// <summary>
		/// The IOC based constructor method for the ProductController class object with Dependency Injection
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="command"></param>
		public ProductController(AppSettings settings, IHSProductCommand command)
		{
			_settings = settings;
			_command = command;
		}

		/// <summary>
		/// Gets the SuperHSProduct object (GET method)
		/// </summary>
		/// <param name="id"></param>
		/// <returns>The SuperHSProduct object</returns>
		[HttpGet, Route("{id}"), OrderCloudUserAuth(ApiRole.ProductAdmin, ApiRole.ProductReader)]
		public async Task<SuperHSProduct> Get(string id)
		{
			return await _command.Get(id, UserContext.AccessToken);
		}

		/// <summary>
		/// Gets the ListPage of SuperHSProduct objects (GET method)
		/// </summary>
		/// <param name="args"></param>
		/// <returns>The ListPage of SuperHSProduct objects</returns>
		[HttpGet, OrderCloudUserAuth(ApiRole.ProductAdmin, ApiRole.ProductReader)]
		public async Task<ListPage<SuperHSProduct>> List(ListArgs<HSProduct> args)
		{
			return await _command.List(args, UserContext.AccessToken);
		}

		/// <summary>
		/// Creates the SuperHSProduct object (POST method)
		/// </summary>
		/// <param name="obj"></param>
		/// <returns>The newly created SuperHSProduct object</returns>
		[HttpPost, OrderCloudUserAuth(ApiRole.ProductAdmin)]
		public async Task<SuperHSProduct> Post([FromBody] SuperHSProduct obj)
		{
			return await _command.Post(obj, UserContext);
		}

		/// <summary>
		/// Updates the SuperHSProduct object, after posting the updates to it (PUT method)
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="id"></param>
		/// <returns>The newly updated SuperHSProduct object</returns>
		[HttpPut, Route("{id}"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
		public async Task<SuperHSProduct> Put([FromBody] SuperHSProduct obj, string id)
		{
			return await _command.Put(id, obj, UserContext.AccessToken);
		}

		/// <summary>
		/// Removes/Deletes an existing Product object (DELETE method)
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[HttpDelete, Route("{id}"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
		public async Task Delete(string id)
		{
			await _command.Delete(id, UserContext.AccessToken);
		}

		/// <summary>
		/// Gets the Product pricing override (GET method)
		/// </summary>
		/// <param name="id"></param>
		/// <param name="buyerID"></param>
		/// <returns>The HSPriceSchedule object with Product pricing override object</returns>
		[HttpGet, Route("{id}/pricingoverride/buyer/{buyerId}"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
		public async Task<HSPriceSchedule> GetPricingOverride(string id, string buyerID)
		{
			return await _command.GetPricingOverride(id, buyerID, UserContext.AccessToken);
		}

		/// <summary>
		/// Creates a Product pricing override (POST method)
		/// </summary>
		/// <param name="id"></param>
		/// <param name="buyerID"></param>
		/// <param name="priceSchedule"></param>
		/// <returns>The newly created HSPriceSchedule object with Product pricing override object</returns>
		[HttpPost, Route("{id}/pricingoverride/buyer/{buyerID}"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
		public async Task<HSPriceSchedule> CreatePricingOverride(string id, string buyerID, [FromBody] HSPriceSchedule priceSchedule)
		{
			return await _command.CreatePricingOverride(id, buyerID, priceSchedule, UserContext.AccessToken);
		}

		/// <summary>
		/// Updates the Product pricing override (POST method)
		/// </summary>
		/// <param name="id"></param>
		/// <param name="buyerID"></param>
		/// <param name="priceSchedule"></param>
		/// <returns>The newly updated HSPriceSchedule object with Product pricing override object</returns>
		[HttpPut, Route("{id}/pricingoverride/buyer/{buyerID}"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
		public async Task<HSPriceSchedule> UpdatePricingOverride(string id, string buyerID, [FromBody] HSPriceSchedule priceSchedule)
		{
			return await _command.UpdatePricingOverride(id, buyerID, priceSchedule, UserContext.AccessToken);
		}

		/// <summary>
		/// Removes/Deletes an existing Product override object (DELETE method)
		/// </summary>
		/// <param name="id"></param>
		/// <param name="buyerID"></param>
		/// <returns></returns>
		[HttpDelete, Route("{id}/pricingoverride/buyer/{buyerID}"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
		public async Task DeletePricingOverride(string id, string buyerID)
		{
			await _command.DeletePricingOverride(id, buyerID, UserContext.AccessToken);
		}


		/// <summary>
		/// Patches the Product filter option override object (PATCH method)
		/// </summary>
		/// <param name="id"></param>
		/// <param name="product"></param>
		/// <returns>The patched Product object with filter option override</returns>
		[HttpPatch, Route("filteroptionoverride/{id}"), OrderCloudUserAuth(ApiRole.AdminUserAdmin)]
		public async Task<Product> FilterOptionOverride(string id, [FromBody] Product product)
		{
			IDictionary<string, object> facets = product.xp.Facets;
			return await _command.FilterOptionOverride(id, product.DefaultSupplierID, facets, UserContext);
		}
	}
}