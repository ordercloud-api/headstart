using System.Collections.Generic;

namespace OrderCloud.Integrations.ExchangeRates.Models
{
    public class OrderCloudIntegrationsExchangeRate
    {
        public CurrencyCode BaseCode { get; set; }

        public List<OrderCloudIntegrationsConversionRate> Rates { get; set; }
    }
}
