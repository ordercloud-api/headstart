using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OrderCloud.Catalyst;
using OrderCloud.SDK;
using ITaxCodesProvider = Headstart.Common.Services.ITaxCodesProvider;
using TaxCategorizationResponse = Headstart.Common.Services.TaxCategorizationResponse;

namespace OrderCloud.Integrations.Taxation
{
    /// <summary>
    /// Tax Functionality.
    /// </summary>
    public class TaxCategoryController : CatalystController
    {
        private readonly ITaxCodesProvider taxCodesProvider;

        public TaxCategoryController(ITaxCodesProvider taxCodesProvider)
        {
            this.taxCodesProvider = taxCodesProvider;
        }

        /// <summary>
        /// List Tax Codes.
        /// </summary>
        [HttpGet, Route("tax-category"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
        public async Task<TaxCategorizationResponse> ListTaxCategories([FromQuery] string search)
        {
            return await taxCodesProvider.ListTaxCodesAsync(search);
        }
    }
}
