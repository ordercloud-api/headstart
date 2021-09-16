using System;
using System.Threading.Tasks;
using Headstart.API.Commands;
using Headstart.API.Commands.Crud;
using Headstart.Models;
using Headstart.Models.Attributes;
using Headstart.Models.Misc;
using Microsoft.AspNetCore.Mvc;
using ordercloud.integrations.library;
using OrderCloud.Catalyst;
using OrderCloud.SDK;

namespace Headstart.Common.Controllers
{
	/// <summary>
	/// Me and my stuff
	/// </summary> 
	[Route("me")]
	public class MeController : CatalystController
	{
		private readonly IMeProductCommand _meProductCommand;
		public MeController(IMeProductCommand meProductCommand)
		{
			_meProductCommand = meProductCommand;
		}

		/// <summary>
		/// GET Super Product
		/// </summary> 
		[HttpGet, Route("products/{productID}"), OrderCloudUserAuth(ApiRole.Shopper)]
		public async Task<SuperHSMeProduct> GetSuperProduct(string productID)
		{
			return await _meProductCommand.Get(productID, UserContext);
		}

		/// <summary>
		/// LIST products
		/// </summary> 
		[HttpGet, Route("products"), OrderCloudUserAuth(ApiRole.Shopper)]
		public async Task<ListPageWithFacets<HSMeProduct>> ListMeProducts(ListArgs<HSMeProduct> args)
		{
			return await _meProductCommand.List(args, UserContext);
		}

		/// <summary>
		/// POST request information about product
		/// </summary> 
		[HttpPost, Route("products/requestinfo"), OrderCloudUserAuth(ApiRole.Shopper)]
		public async Task RequestProductInfo([FromBody] ContactSupplierBody template)
        {
			await _meProductCommand.RequestProductInfo(template);
        }

	}
}
