using Microsoft.AspNetCore.Mvc;
using OrderCloud.SDK;
using System.Threading.Tasks;
using OrderCloud.Catalyst;
using ITaxCodesProvider = ordercloud.integrations.library.intefaces.ITaxCodesProvider;
using TaxCategorizationResponse = ordercloud.integrations.library.intefaces.TaxCategorizationResponse;

namespace Headstart.Common.Controllers
{
    /// <summary>
    /// Tax Functionality.
    /// </summary>
    public class TaxCategoryController : CatalystController
    {
        private readonly ITaxCodesProvider _taxCodesProvider;

        public TaxCategoryController(ITaxCodesProvider taxCodesProvider)
        {
            _taxCodesProvider = taxCodesProvider;
        }

        /// <summary>
        /// List Tax Codes.
        /// </summary>
        [HttpGet, Route("tax-category"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
        public async Task<TaxCategorizationResponse> ListTaxCategories([FromQuery] string search)
        {
            return await _taxCodesProvider.ListTaxCodesAsync(search);
        }
    }
}
