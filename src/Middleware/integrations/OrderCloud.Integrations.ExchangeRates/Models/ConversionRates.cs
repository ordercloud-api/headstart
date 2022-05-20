using System.Collections.Generic;

namespace OrderCloud.Integrations.ExchangeRates.Models
{
    public class ConversionRates
    {
        public CurrencyCode BaseCode { get; set; }

        public List<ConversionRate> Rates { get; set; }
    }
}
