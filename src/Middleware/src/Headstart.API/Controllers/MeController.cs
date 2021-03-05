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
	[DocComments("Me and my stuff")]
	[HSSection.Headstart(ListOrder = 10)]
	[Route("me")]
	public class MeController : BaseController
	{

		private readonly IMeProductCommand _meProductCommand;
		private readonly IHSKitProductCommand _kitProductCommand;
		public MeController(IMeProductCommand meProductCommand, IHSKitProductCommand kitProductCommand)
		{
			_meProductCommand = meProductCommand;
			_kitProductCommand = kitProductCommand;
		}

		[DocName("GET Super Product")]
		[HttpGet, Route("products/{productID}"), OrderCloudUserAuth(ApiRole.Shopper)]
		public async Task<SuperHSMeProduct> GetSuperProduct(string productID)
		{
			return await _meProductCommand.Get(productID, UserContext);
		}

		[DocName("LIST products")]
		[HttpGet, Route("products"), OrderCloudUserAuth(ApiRole.Shopper)]
		public async Task<ListPageWithFacets<HSMeProduct>> ListMeProducts(ListArgs<HSMeProduct> args)
		{
			return await _meProductCommand.List(args, UserContext);
		}

		[DocName("POST request information about product")]
		[HttpPost, Route("products/requestinfo"), OrderCloudUserAuth(ApiRole.Shopper)]
		public async Task RequestProductInfo([FromBody] ContactSupplierBody template)
        {
			await _meProductCommand.RequestProductInfo(template);
        }

		//[DocName("GET Kit Product")]
		//[HttpGet, Route("kitproducts/{kitProductID}"), OrderCloudIntegrationsAuth(ApiRole.Shopper)]
		//public async Task<HSMeKitProduct> GetMeKit(string kitProductID)
		//{
		//	return await _kitProductCommand.GetMeKit(kitProductID, VerifiedUserContext);
		//}
	}
}
