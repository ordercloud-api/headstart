using System.Threading.Tasks;
using Flurl.Http;
using Flurl.Http.Configuration;

namespace ordercloud.integrations.exchangerates
{
    /// <summary>
    /// Rates supported by https://exchangeratesapi.io/
    /// </summary>
    public interface IOrderCloudIntegrationsExchangeRatesClient
    {
        Task<ExchangeRatesBase> Get(CurrencySymbol symbol);
    }

    public class OrderCloudIntegrationsExchangeRatesClient: IOrderCloudIntegrationsExchangeRatesClient
    {
        private readonly IFlurlClient _flurl;

        public OrderCloudIntegrationsExchangeRatesClient(IFlurlClientFactory flurlFactory)
        {
            _flurl = flurlFactory.Get($"https://api.exchangeratesapi.io/");
        }

        private IFlurlRequest Request(string resource)
        {
            return _flurl.Request(resource);
        }
       
        public async Task<ExchangeRatesBase> Get(CurrencySymbol symbol)
        {
            return await this.Request("latest")
                .SetQueryParam("base", symbol)
                .GetJsonAsync<ExchangeRatesBase>();
        }
    }
}
