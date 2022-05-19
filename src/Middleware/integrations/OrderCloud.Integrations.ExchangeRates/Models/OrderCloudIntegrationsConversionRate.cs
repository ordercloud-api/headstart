using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OrderCloud.Integrations.ExchangeRates.Models
{
    public class OrderCloudIntegrationsConversionRate
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public CurrencyCode Currency { get; set; }

        public string Symbol { get; set; }

        public string Name { get; set; }

        public double? Rate { get; set; }

        public string Icon { get; set; }
    }
}
