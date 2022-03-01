using OrderCloud.SDK;
using OrderCloud.Catalyst;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Headstart.Common.Controllers
{
	// At one point we were trying to support a multi-tenant solution and configuration
	// could not live in the code so we stored these configurations in cosmos
	// this is no longer the goal and having these configurations stored in a database feels like overkill and adds complexity
	// once we have more time we should aim to remove the whole notion of supplier filter configs and just have this live in code
	public class SupplierFilterConfigController : CatalystController
	{
		/// <summary>
		/// The Default constructor method for the SupplierFilterConfigController
		/// </summary>
		public SupplierFilterConfigController() { }

		/// <summary>
		/// Gets the ListPage of Dynamic objects request (GET method)
		/// </summary>
		/// <returns>The ListPage of Dynamic objects request</returns>
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

		/// <summary>
		/// Private re-usable GetCountriesServicingDoc method
		/// </summary>
		/// <returns>The CountriesServicingDoc dynamic objects</returns>
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