using System.Collections.Generic;
using OrderCloud.Integrations.Library.Models;

namespace OrderCloud.Integrations.ExchangeRates.Models
{
    public class ConversionRates
    {
        public CurrencyCode BaseCode { get; set; }

        public List<ConversionRate> Rates { get; set; }
    }
}
