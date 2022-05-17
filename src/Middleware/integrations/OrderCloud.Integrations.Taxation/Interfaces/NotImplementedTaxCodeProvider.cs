using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderCloud.Integrations.Taxation.Interfaces
{
    public class NotImplementedTaxCodesProvider : ITaxCodesProvider
    {
        public Task<TaxCategorizationResponse> ListTaxCodesAsync(string searchTerm)
        {
            return Task.FromResult(new TaxCategorizationResponse()
            {
                ProductsShouldHaveTaxCodes = false,
                Categories = new List<TaxCategorization>(),
            });
        }
    }
}
