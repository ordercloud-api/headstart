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
        Task<ExchangeRatesBase> Get(CurrencyCode currencyCode);
    }

    public class OrderCloudIntegrationsExchangeRatesClient : IOrderCloudIntegrationsExchangeRatesClient
    {
        private readonly IFlurlClient flurl;

        public OrderCloudIntegrationsExchangeRatesClient(IFlurlClientFactory flurlFactory)
        {
            flurl = flurlFactory.Get($"https://api.exchangeratesapi.io/");
        }

        public async Task<ExchangeRatesBase> Get(CurrencyCode currencyCode)
        {
            return await this.Request("latest")
                .SetQueryParam("base", currencyCode)
                .GetJsonAsync<ExchangeRatesBase>();
        }

        private IFlurlRequest Request(string resource)
        {
            return flurl.Request(resource);
        }
    }
}
