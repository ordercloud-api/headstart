using Microsoft.AspNetCore.Mvc;
using OrderCloud.SDK;
using System.Threading.Tasks;
using OrderCloud.Catalyst;
using ITaxCodesProvider = OrderCloud.Integrations.Library.Interfaces.ITaxCodesProvider;
using TaxCategorizationResponse = OrderCloud.Integrations.Library.Interfaces.TaxCategorizationResponse;

namespace Headstart.Common.Controllers
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
