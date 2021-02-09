using Microsoft.AspNetCore.Mvc;
using OrderCloud.SDK;
using System.Threading.Tasks;
using Headstart.Models.Attributes;
using ordercloud.integrations.library;
using Headstart.Models;
using Headstart.Common.Services.CMS;
using Headstart.Common.Services.CMS.Models;
using Headstart.API.Controllers;
using System.Collections.Generic;

namespace Headstart.Common.Controllers
{
	// At one point we were trying to support a multi-tenant solution and configuration
	// could not live in the code so we stored these configurations in cosmos
	// this is no longer the goal and having these configurations stored in a database feels like overkill and adds complexity
	// once we have more time we should aim to remove the whole notion of supplier filter configs and just have this live in code

    [DocComments("\"Supplier Filter Config\" represents Supplier Category Configuration")]
    [HSSection.Headstart(ListOrder = 5)]
    public class SupplierFilterConfigController : BaseController
    {
        public SupplierFilterConfigController(AppSettings settings) : base(settings)
        {

        }

        [DocName("GET SupplierCategoryConfig")]
        [HttpGet, Route("/supplierfilterconfig"), OrderCloudIntegrationsAuth(ApiRole.Shopper, ApiRole.SupplierReader)]
        public async Task<ListPage<SupplierFilterConfigDocument>> Get()
        {
			return new ListPage<SupplierFilterConfigDocument>
			{
				Items = new List<SupplierFilterConfigDocument>
				{
					GetCountriesServicingDoc(),
					GetServiceCategoryDoc(),
					GetVendorLevelDoc()
				}
			};
        }

		private SupplierFilterConfigDocument GetCountriesServicingDoc()
		{
			return new SupplierFilterConfigDocument
			{
				ID = "CountriesServicing",
				Doc = new SupplierFilterConfig
				{
					Display = "Countries Servicing",
					Path = "xp.CountriesServicing",
					Items = new List<Filter>
					{
						new Filter
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

		private SupplierFilterConfigDocument GetServiceCategoryDoc()
		{
			return new SupplierFilterConfigDocument
			{
				ID = "ServiceCategory",
				Doc = new SupplierFilterConfig
				{
					Display = "Service Category",
					Path = "xp.Categories.ServiceCategory",
					AllowSupplierEdit = false,
					AllowSellerEdit = true,
					BuyerAppFilterType = "SelectOption",
					Items = new List<Filter>
					{

					}
				}
			};
		}

		private SupplierFilterConfigDocument GetVendorLevelDoc()
		{
			return new SupplierFilterConfigDocument
			{
				ID = "VendorLevel",
				Doc = new SupplierFilterConfig
				{
					Display = "Vendor Level",
					Path = "xp.Categories.VendorLevel",
					AllowSupplierEdit = true,
					AllowSellerEdit = true,
					BuyerAppFilterType = "SelectOption",
					Items = new List<Filter>
					{

					}
				}
			};
		}

	}

	[SwaggerModel]
    // swagger generator can't handle composite models so alias into one
    public class SupplierFilterConfigDocument: Document<SupplierFilterConfig>
    {

    }
}
