using System.Threading.Tasks;
using Headstart.Common.Models;
using Headstart.Common.Services;
using OrderCloud.Integrations.ExchangeRates.Mappers;

namespace OrderCloud.Integrations.ExchangeRates
{
    public class ExchangeRatesService : ICurrencyConversionService
    {
        private readonly IExchangeRatesClient exchangeRatesClient;

        public ExchangeRatesService(IExchangeRatesClient exchangeRatesClient)
        {
            this.exchangeRatesClient = exchangeRatesClient;
        }

        /// <summary>
        /// Intended for private consumption by functions that update the cached resources.
        /// </summary>
        /// <param name="currencyCode">The ISO 4217 currency code.</param>
        /// <returns>The available exchange rates.</returns>
        public async Task<ConversionRates> Get(CurrencyCode currencyCode)
        {
            var rates = await exchangeRatesClient.Get(currencyCode);
            return new ConversionRates()
            {
                BaseCode = currencyCode,
                Rates = ExchangeRatesMapper.MapRates(rates.rates),
            };
        }
    }
}
