using System.Threading.Tasks;
using Flurl.Http;
using Flurl.Http.Configuration;
using OrderCloud.Integrations.ExchangeRates.Models;

namespace OrderCloud.Integrations.ExchangeRates
{
    /// <summary>
    /// Rates supported by https://exchangeratesapi.io/.
    /// </summary>
    public interface IOrderCloudIntegrationsExchangeRatesClient
    {
        Task<ExchangeRatesBase> Get(CurrencySymbol symbol);
    }

    public class OrderCloudIntegrationsExchangeRatesClient : IOrderCloudIntegrationsExchangeRatesClient
    {
        private readonly IFlurlClient flurl;

        public OrderCloudIntegrationsExchangeRatesClient(IFlurlClientFactory flurlFactory)
        {
            flurl = flurlFactory.Get($"https://api.exchangeratesapi.io/");
        }

        public async Task<ExchangeRatesBase> Get(CurrencySymbol symbol)
        {
            return await this.Request("latest")
                .SetQueryParam("base", symbol)
                .GetJsonAsync<ExchangeRatesBase>();
        }

        private IFlurlRequest Request(string resource)
        {
            return flurl.Request(resource);
        }
    }
}
