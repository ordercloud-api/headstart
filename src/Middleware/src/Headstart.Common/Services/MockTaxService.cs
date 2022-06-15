using System.Collections.Generic;
using System.Threading.Tasks;
using OrderCloud.SDK;

namespace Headstart.Common.Services
{
    public class MockTaxService : ITaxCodesProvider, ITaxCalculator
    {
        public Task<OrderTaxCalculation> CalculateEstimateAsync(OrderWorksheet orderWorksheet, List<OrderPromotion> promotions)
        {
            return Task.FromResult(new OrderTaxCalculation()
            {
                ExternalTransactionID = "Mock Tax Response for Headstart",
                TotalTax = 123.45m,
            });
        }

        public Task<OrderTaxCalculation> CommitTransactionAsync(OrderWorksheet orderWorksheet, List<OrderPromotion> promotions)
        {
            return Task.FromResult(new OrderTaxCalculation()
            {
                ExternalTransactionID = "Mock Tax Response for Headstart",
                TotalTax = 123.45m,
            });
        }

        public Task<TaxCategorizationResponse> ListTaxCodesAsync(string searchTerm)
        {
            return Task.FromResult(new TaxCategorizationResponse()
            {
                ProductsShouldHaveTaxCodes = true,
                Categories = new List<TaxCategorization>()
                {
                    new TaxCategorization()
                    {
                        Code = "Headstart Tax Code",
                        Description = "Mock Tax Code for Headstart",
                        LongDescription = "This is a mock tax categorization",
                    },
                },
            });
        }
    }
}
