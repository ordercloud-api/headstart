using OrderCloud.SDK;
using OrderCloud.Catalyst;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TaxCategorizationResponse = ordercloud.integrations.library.intefaces.TaxCategorizationResponse;
using ITaxCodesProvider = ordercloud.integrations.library.intefaces.ITaxCodesProvider;

namespace Headstart.API.Controllers
{
	public class TaxCategoryController : CatalystController
	{
		private readonly ITaxCodesProvider _taxCodesProvider;

		/// <summary>
		/// The IOC based constructor method for the TaxCategoryController class object with Dependency Injection
		/// </summary>
		/// <param name="taxCodesProvider"></param>
		public TaxCategoryController(ITaxCodesProvider taxCodesProvider)
		{
			_taxCodesProvider = taxCodesProvider;
		}

		/// <summary>
		/// Get the list of TaxCategory objects (GET method)
		/// </summary>
		/// <param name="search"></param>
		/// <returns>The list of TaxCategory objects</returns>
		[HttpGet, Route("tax-category"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
		public async Task<TaxCategorizationResponse> ListTaxCategories([FromQuery] string search)
		{
			return await _taxCodesProvider.ListTaxCodesAsync(search);
		}
	}
}