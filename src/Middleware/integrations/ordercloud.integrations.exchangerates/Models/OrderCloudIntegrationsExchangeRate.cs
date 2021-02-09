using System.Collections.Generic;
using ordercloud.integrations.library;

namespace ordercloud.integrations.exchangerates
{
    [SwaggerModel]
    public class OrderCloudIntegrationsExchangeRate
    {
        public CurrencySymbol BaseSymbol { get; set; }
        public List<OrderCloudIntegrationsConversionRate> Rates { get; set; }
    }
}
