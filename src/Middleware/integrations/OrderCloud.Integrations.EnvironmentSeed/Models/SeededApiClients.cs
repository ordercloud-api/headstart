using OrderCloud.SDK;

namespace OrderCloud.Integrations.EnvironmentSeed.Models
{
    public class SeededApiClients
    {
        public ApiClient AdminUiApiClient { get; set; }

        public ApiClient BuyerUiApiClient { get; set; }

        public ApiClient BuyerLocalUiApiClient { get; set; }

        public ApiClient MiddlewareApiClient { get; set; }
    }
}
