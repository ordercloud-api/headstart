using Avalara.AvaTax.RestClient;
using System.Linq;
using System.Collections.Generic;
using TaxCategorization = OrderCloud.Integrations.Library.Interfaces.TaxCategorization;

namespace OrderCloud.Integrations.Avalara.Mappers
{
    public static class TaxCodeMapper
    {
        // Tax Codes for lines on Transactions
        public static List<TaxCategorization> MapTaxCodes(FetchResult<TaxCodeModel> codes)
        {
            return codes.value.Select(code => new TaxCategorization
            {
                Code = code.taxCode,
                Description = code.description,
            }).ToList();
        }

        public static string MapSearchString(string searchTerm)
        {
            var searchString = $"isActive eq true";
            if (searchTerm != string.Empty)
            {
                searchString = $"{searchString} and (taxCode contains '{searchTerm}' OR description contains '{searchTerm}')";
            }

            return searchString;
        }
    }
}
