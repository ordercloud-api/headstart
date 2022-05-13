using System.Collections.Generic;

namespace OrderCloud.Integrations.ExchangeRates.Models
{
    public class OrderCloudIntegrationsExchangeRate
    {
        public CurrencySymbol BaseSymbol { get; set; }

        public List<OrderCloudIntegrationsConversionRate> Rates { get; set; }
    }
}
