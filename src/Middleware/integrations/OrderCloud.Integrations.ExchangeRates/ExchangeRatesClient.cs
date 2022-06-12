using System.Threading.Tasks;
using Flurl.Http;
using Flurl.Http.Configuration;
using Headstart.Common.Models;
using OrderCloud.Integrations.ExchangeRates.Models;

namespace OrderCloud.Integrations.ExchangeRates
{
    /// <summary>
    /// Rates supported by https://exchangeratesapi.io/.
    /// </summary>
    public interface IExchangeRatesClient
    {
        Task<ExchangeRatesBase> Get(CurrencyCode currencyCode);
    }

    public class ExchangeRatesClient : IExchangeRatesClient
    {
        private readonly IFlurlClient flurl;

        public ExchangeRatesClient(IFlurlClientFactory flurlFactory)
        {
            flurl = flurlFactory.Get($"https://api.exchangeratesapi.io/");
        }

        public virtual async Task<ExchangeRatesBase> Get(CurrencyCode currencyCode)
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
