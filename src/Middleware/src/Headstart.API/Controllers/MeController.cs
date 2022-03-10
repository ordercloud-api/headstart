using OrderCloud.SDK;
using OrderCloud.Catalyst;
using System.Threading.Tasks;
using Headstart.API.Commands;
using Microsoft.AspNetCore.Mvc;
using Headstart.Common.Models.Misc;
using Headstart.Common.Models.Headstart;

namespace Headstart.API.Controllers
{
	[Route("me")]
	public class MeController : CatalystController
	{
		private readonly IMeProductCommand _meProductCommand;
        
		/// <summary>
		/// The IOC based constructor method for the MeController class object with Dependency Injection
		/// </summary>
		/// <param name="meProductCommand"></param>
		public MeController(IMeProductCommand meProductCommand)
		{
			_meProductCommand = meProductCommand;
		}

		/// <summary>
		/// Gets the list of Super Products (GET method)
		/// </summary>
		/// <param name="productId"></param>
		/// <returns>The list of Super Products</returns>
		[HttpGet, Route("products/{productId}"), OrderCloudUserAuth(ApiRole.Shopper)]
		public async Task<SuperHsMeProduct> GetSuperProduct(string productId)
		{
			return await _meProductCommand.Get(productId, UserContext);
		}

		/// <summary>
		/// Gets the ListPage of Product objects (GET method)
		/// </summary>
		/// <param name="args"></param>
		/// <returns>The ListPage of Product objects</returns>
		[HttpGet, Route("products"), OrderCloudUserAuth(ApiRole.Shopper)]
		public async Task<ListPageWithFacets<HsMeProduct>> ListMeProducts(ListArgs<HsMeProduct> args)
		{
			return await _meProductCommand.List(args, UserContext);
		}

		/// <summary>
		/// Posts requested information about a product (POST method)
		/// </summary>
		/// <param name="template"></param>
		/// <returns></returns>
		[HttpPost, Route("products/requestinfo"), OrderCloudUserAuth(ApiRole.Shopper)]
		public async Task RequestProductInfo([FromBody] ContactSupplierBody template)
		{
			await _meProductCommand.RequestProductInfo(template);
		}
	}
}