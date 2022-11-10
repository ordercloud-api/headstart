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
        Task<ExchangeRatesBase> Get(CurrencyCode currency);
    }

    public class ExchangeRatesClient : IExchangeRatesClient
    {
        private readonly IFlurlClient flurl;
        private readonly ExchangeRateSettings settings;

        public ExchangeRatesClient(IFlurlClientFactory flurlFactory, ExchangeRateSettings settings)
        {
            this.flurl = flurlFactory.Get($"https://api.apilayer.com/exchangerates_data");
            this.settings = settings;
        }

        public virtual async Task<ExchangeRatesBase> Get(CurrencyCode currency)
        {
            return await this.Request("latest")
                .SetQueryParam("base", currency)
                .WithHeader("apikey", settings.ApiKey)
                .GetJsonAsync<ExchangeRatesBase>();
        }

        private IFlurlRequest Request(string resource)
        {
            return flurl.Request(resource);
        }
    }
}
