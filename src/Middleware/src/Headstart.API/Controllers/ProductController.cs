using OrderCloud.SDK;
using Headstart.Common;
using OrderCloud.Catalyst;
using System.Threading.Tasks;
using Headstart.API.Commands;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Headstart.Common.Models.Headstart;

namespace Headstart.API.Controllers
{
	[Route("products")]
	public class ProductController : CatalystController
	{
		private readonly IHsProductCommand _command;
		private readonly AppSettings _settings;

		/// <summary>
		/// The IOC based constructor method for the ProductController class object with Dependency Injection
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="command"></param>
		public ProductController(AppSettings settings, IHsProductCommand command)
		{
			_settings = settings;
			_command = command;
		}

		/// <summary>
		/// Gets the Super Product object (GET method)
		/// </summary>
		/// <param name="id"></param>
		/// <returns>The Super Product object</returns>
		[HttpGet, Route("{id}"), OrderCloudUserAuth(ApiRole.ProductAdmin, ApiRole.ProductReader)]
		public async Task<SuperHsProduct> Get(string id)
		{
			return await _command.Get(id, UserContext.AccessToken);
		}

		/// <summary>
		/// Gets the ListPage of SuperHsProduct objects (GET method)
		/// </summary>
		/// <param name="args"></param>
		/// <returns>The ListPage of SuperHsProduct objects</returns>
		[HttpGet, OrderCloudUserAuth(ApiRole.ProductAdmin, ApiRole.ProductReader)]
		public async Task<ListPage<SuperHsProduct>> List(ListArgs<HsProduct> args)
		{
			return await _command.List(args, UserContext.AccessToken);
		}

		/// <summary>
		/// Creates the Super Product object (POST method)
		/// </summary>
		/// <param name="obj"></param>
		/// <returns>The newly created Super Product object</returns>
		[HttpPost, OrderCloudUserAuth(ApiRole.ProductAdmin)]
		public async Task<SuperHsProduct> Post([FromBody] SuperHsProduct obj)
		{
			return await _command.Post(obj, UserContext);
		}

		/// <summary>
		/// Updates the Super Product object, after posting the updates to it (PUT method)
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		[HttpPut, Route("{id}"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
		public async Task<SuperHsProduct> Put([FromBody] SuperHsProduct obj, string id)
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
		/// <param name="buyerId"></param>
		/// <returns>The Product pricing override object</returns>
		[HttpGet, Route("{id}/pricingoverride/buyer/{buyerId}"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
		public async Task<HsPriceSchedule> GetPricingOverride(string id, string buyerId)
		{
			return await _command.GetPricingOverride(id, buyerId, UserContext.AccessToken);
		}

		/// <summary>
		/// Creates a Product pricing override (POST method)
		/// </summary>
		/// <param name="id"></param>
		/// <param name="buyerId"></param>
		/// <param name="priceSchedule"></param>
		/// <returns>The newly created Product pricing override object</returns>
		[HttpPost, Route("{id}/pricingoverride/buyer/{buyerId}"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
		public async Task<HsPriceSchedule> CreatePricingOverride(string id, string buyerId, [FromBody] HsPriceSchedule priceSchedule)
		{
			return await _command.CreatePricingOverride(id, buyerId, priceSchedule, UserContext.AccessToken);
		}

		/// <summary>
		/// Updates the Product pricing override (POST method)
		/// </summary>
		/// <param name="id"></param>
		/// <param name="buyerId"></param>
		/// <param name="priceSchedule"></param>
		/// <returns>The newly updated Product pricing override object</returns>
		[HttpPut, Route("{id}/pricingoverride/buyer/{buyerId}"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
		public async Task<HsPriceSchedule> UpdatePricingOverride(string id, string buyerId, [FromBody] HsPriceSchedule priceSchedule)
		{
			return await _command.UpdatePricingOverride(id, buyerId, priceSchedule, UserContext.AccessToken);
		}

		/// <summary>
		/// Removes/Deletes an existing Product override object (DELETE method)
		/// </summary>
		/// <param name="id"></param>
		/// <param name="buyerId"></param>
		/// <returns></returns>
		[HttpDelete, Route("{id}/pricingoverride/buyer/{buyerId}"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
		public async Task DeletePricingOverride(string id, string buyerId)
		{
			await _command.DeletePricingOverride(id, buyerId, UserContext.AccessToken);
		}


		/// <summary>
		/// Patches the Product filter option override object (PATCH method)
		/// </summary>
		/// <param name="id"></param>
		/// <param name="product"></param>
		/// <returns>The patched Product filter option override object</returns>
		[HttpPatch, Route("filteroptionoverride/{id}"), OrderCloudUserAuth(ApiRole.AdminUserAdmin)]
		public async Task<Product> FilterOptionOverride(string id, [FromBody] Product product)
		{
			IDictionary<string, object> facets = product.xp.Facets;
			return await _command.FilterOptionOverride(id, product.DefaultSupplierID, facets, UserContext);
		}
	}
}