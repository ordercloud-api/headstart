using Microsoft.AspNetCore.Mvc;
using OrderCloud.SDK;
using System.Threading.Tasks;
using Headstart.Models.Attributes;
using ordercloud.integrations.library;
using System.Collections.Generic;
using OrderCloud.Catalyst;

namespace Headstart.Common.Controllers
{
	// At one point we were trying to support a multi-tenant solution and configuration
	// could not live in the code so we stored these configurations in cosmos
	// this is no longer the goal and having these configurations stored in a database feels like overkill and adds complexity
	// once we have more time we should aim to remove the whole notion of supplier filter configs and just have this live in code

	/// <summary>
	/// Supplier Category Configuration
	/// </summary>

	public class SupplierFilterConfigController : CatalystController
	{
		public SupplierFilterConfigController()
		{

		}

		/// <summary>
		/// GET SupplierCategoryConfig
		[HttpGet, Route("/supplierfilterconfig"), OrderCloudUserAuth(ApiRole.Shopper, ApiRole.SupplierReader)]
		public async Task<ListPage<dynamic>> Get()
		{
			return new ListPage<dynamic>
			{
				Items = new List<dynamic>
				{
					GetCountriesServicingDoc()
				}
			};
		}

		private dynamic GetCountriesServicingDoc()
		{
			return new
			{
				ID = "CountriesServicing",
				Doc = new
				{
					Display = "Countries Servicing",
					Path = "xp.CountriesServicing",
					Items = new List<dynamic>
					{
						new
						{
							Text = "UnitedStates",
							Value = "US"
						}
					},
					AllowSellerEdit = true,
					AllowSupplierEdit = true,
					BuyerAppFilterType = "NonUI"
				}
			};
		}
	}
}
