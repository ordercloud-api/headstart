using Microsoft.AspNetCore.Mvc;
using OrderCloud.SDK;
using System.Threading.Tasks;
using Headstart.Models.Attributes;
using ordercloud.integrations.library;
using Headstart.API.Commands;
using OrderCloud.Catalyst;
using ordercloud.integrations.library.intefaces;
using System.Collections.Generic;

namespace Headstart.Common.Controllers
{
	/// <summary>
	/// Tax Functionality
	/// </summary>
	public class TaxCategoryController : CatalystController
	{
		private readonly ITaxCodesProvider _taxCodesProvider;
		public TaxCategoryController(ITaxCodesProvider taxCodesProvider)
		{
			_taxCodesProvider = taxCodesProvider;
		}

		/// <summary>
		/// List Tax Codes
		/// </summary>
		[HttpGet, Route("tax-category"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
		public async Task<TaxCategorizationResponse> ListTaxCategories([FromQuery] string search)
		{
			return await _taxCodesProvider.ListTaxCodesAsync(search);
		}
	}
}
