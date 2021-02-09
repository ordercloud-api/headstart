using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ordercloud.integrations.library;

namespace ordercloud.integrations.exchangerates
{
    [SwaggerModel]
    public class OrderCloudIntegrationsConversionRate
    {
		[JsonConverter(typeof(StringEnumConverter))]
		public CurrencySymbol Currency { get; set; }
        public string Symbol { get; set; }
        public string Name { get; set; }
        public double? Rate { get; set; }
        public string Icon { get; set; }
    }
}
