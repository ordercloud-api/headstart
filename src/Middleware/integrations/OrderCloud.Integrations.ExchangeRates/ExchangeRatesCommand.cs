using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Flurl.Http.Configuration;
using Newtonsoft.Json;
using OrderCloud.Catalyst;
using OrderCloud.Integrations.ExchangeRates.Models;
using OrderCloud.Integrations.Library;
using OrderCloud.Integrations.Library.Models;
using OrderCloud.SDK;

namespace OrderCloud.Integrations.ExchangeRates
{
    public interface IExchangeRatesCommand
    {
        Task<ListPage<ConversionRate>> Get(ListArgs<ConversionRate> rateArgs, CurrencyCode currency);

        Task<ConversionRates> Get(CurrencyCode currencyCode);

        Task<ListPage<ConversionRate>> GetRateList();

        ListPage<ConversionRate> Filter(ListArgs<ConversionRate> rateArgs, ConversionRates rates);

        Task<double?> ConvertCurrency(CurrencyCode from, CurrencyCode to, double value);

        Task Update();
    }

    public class ExchangeRatesCommand : IExchangeRatesCommand
    {
        private readonly IExchangeRatesClient client;
        private readonly IOrderCloudIntegrationsBlobService blob;
        private readonly ISimpleCache cache;

        public ExchangeRatesCommand(IOrderCloudIntegrationsBlobService blob, IFlurlClientFactory flurlFactory, ISimpleCache cache)
        {
            client = new ExchangeRatesClient(flurlFactory);
            this.blob = blob;
            this.cache = cache;
        }

        /// <summary>
        /// Intended for public API based consumption. Hence the ListArgs implementation.
        /// </summary>
        /// <param name="rateArgs">The conversion rates.</param>
        /// <param name="currency">The ISO 4217 currency code.</param>
        /// <returns>The conversion rate.</returns>
        public async Task<ListPage<ConversionRate>> Get(ListArgs<ConversionRate> rateArgs, CurrencyCode currency)
        {
            try
            {
                return await GetCachedRates(rateArgs, currency);
            }
            catch (Exception)
            {
                await Update();
                return await GetCachedRates(rateArgs, currency);
            }
        }

        public ListPage<ConversionRate> Filter(ListArgs<ConversionRate> rateArgs, ConversionRates rates)
        {
            if (rateArgs.Filters?.Any(filter => filter.PropertyName == "CurrencyCode") ?? false)
            {
                rates.Rates = (
                        from rate in rates.Rates
                        from s in rateArgs.Filters.FirstOrDefault(r => r.PropertyName == "CurrencyCode")?.FilterValues
                        where rate.Currency == s.Term.To<CurrencyCode>()
                        select rate).ToList();
            }

            var list = new ListPage<ConversionRate>()
            {
                Meta = new ListPageMeta()
                {
                    Page = 1,
                    PageSize = 1,
                    TotalCount = rates.Rates.Count,
                    ItemRange = new[] { 1, rates.Rates.Count },
                },
                Items = rates.Rates,
            };

            return list;
        }

        public async Task<double?> ConvertCurrency(CurrencyCode from, CurrencyCode to, double value)
        {
            var rates = await this.Get(new ListArgs<ConversionRate>(), from);
            var rate = rates.Items.FirstOrDefault(r => r.Currency == to)?.Rate;
            return value * rate;
        }

        /// <summary>
        /// Intended for private consumption by functions that update the cached resources.
        /// </summary>
        /// <param name="currencyCode">The ISO 4217 currency code.</param>
        /// <returns>The available exchange rates.</returns>
        public async Task<ConversionRates> Get(CurrencyCode currencyCode)
        {
            var rates = await client.Get(currencyCode);
            return new ConversionRates()
            {
                BaseCode = currencyCode,
                Rates = MapRates(rates.rates),
            };
        }

        public async Task<ListPage<ConversionRate>> GetRateList()
        {
            var rates = MapRates();
            return await Task.FromResult(new ListPage<ConversionRate>()
            {
                Meta = new ListPageMeta()
                {
                    Page = 1,
                    PageSize = 1,
                    TotalCount = rates.Count,
                    ItemRange = new[] { 1, rates.Count },
                },
                Items = rates,
            });
        }

        public async Task Update()
        {
            var list = await GetRateList();
            await Throttler.RunAsync(list.Items, 100, 10, async rate =>
            {
                var rates = await Get(rate.Currency);
                await blob.Save($"{rate.Currency}.json", JsonConvert.SerializeObject(rates));
            });
        }

        private static string GetIcon(CurrencyCode currencyCode)
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"OrderCloud.Integrations.ExchangeRates.Icons.{currencyCode}.gif");
            if (stream == null)
            {
                return null;
            }

            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            return $"data:image/jpg;base64,{Convert.ToBase64String(ms.ToArray())}";
        }

        private static List<ConversionRate> MapRates(ExchangeRatesValues ratesValues = null)
        {
            return Enum.GetValues(typeof(CurrencyCode)).Cast<CurrencyCode>().Select(currencyCode => new ConversionRate()
            {
                Currency = currencyCode,
                Icon = GetIcon(currencyCode),
                Symbol = CurrencyLookup.CurrencyCodeLookup.FirstOrDefault(s => s.Key == currencyCode).Value.Symbol,
                Name = CurrencyLookup.CurrencyCodeLookup.FirstOrDefault(s => s.Key == currencyCode).Value.Name,
                Rate = FixRate(ratesValues, currencyCode),
            }).ToList();
        }

        private static double? FixRate(ExchangeRatesValues values, CurrencyCode e)
        {
            var t = values?.GetType().GetProperty($"{e}")?.GetValue(values, null).To<double?>();
            if (!t.HasValue)
            {
                return 1;
            }

            return t.Value == 0 ? 1 : t.Value;
        }

        private async Task<ListPage<ConversionRate>> GetCachedRates(ListArgs<ConversionRate> rateArgs, CurrencyCode currency)
        {
            var rates = await cache.GetOrAddAsync($"exchangerates_{currency}", TimeSpan.FromHours(1), () =>
            {
                return blob.Get<ConversionRates>($"{currency}.json");
            });
            return Filter(rateArgs, rates);
        }
    }
}
