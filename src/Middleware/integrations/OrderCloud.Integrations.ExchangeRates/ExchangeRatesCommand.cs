using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Headstart.Common.Commands;
using Headstart.Common.Models;
using Headstart.Common.Services;
using Newtonsoft.Json;
using OrderCloud.Catalyst;
using OrderCloud.Integrations.AzureStorage;
using OrderCloud.Integrations.ExchangeRates.Mappers;
using OrderCloud.Integrations.Library;
using OrderCloud.SDK;

namespace OrderCloud.Integrations.ExchangeRates
{
    public class ExchangeRatesCommand : ICurrencyConversionCommand
    {
        private readonly IOrderCloudClient oc;
        private readonly ICurrencyConversionService currencyConversionService;
        private readonly ICloudBlobService cloudBlobService;
        private readonly ISimpleCache cache;

        public ExchangeRatesCommand(IOrderCloudClient oc, ICloudBlobService cloudBlobService, ICurrencyConversionService currencyConversionService, ISimpleCache cache)
        {
            this.oc = oc;
            this.currencyConversionService = currencyConversionService;
            this.cloudBlobService = cloudBlobService;
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

        public async Task<ListPage<ConversionRate>> GetRateList()
        {
            var rates = ExchangeRatesMapper.MapRates();
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

        public async Task<CurrencyCode> GetCurrencyForUser(string userToken)
        {
            var buyerUserGroups = await oc.Me.ListUserGroupsAsync<HSLocationUserGroup>(opts => opts.AddFilter(u => u.xp.Type == "BuyerLocation"), userToken);
            var currency = buyerUserGroups.Items.FirstOrDefault(u => u.xp.Currency != null)?.xp?.Currency;
            Require.That(currency != null, new ErrorCode("Exchange Rate Error", "Exchange Rate Not Defined For User"));
            return (CurrencyCode)currency;
        }

        public async Task<List<ConversionRate>> GetExchangeRatesForUser(string userToken)
        {
            var currency = await GetCurrencyForUser(userToken);
            var exchangeRates = await Get(new ListArgs<ConversionRate>() { }, currency);
            return exchangeRates.Items.ToList();
        }

        public async Task Update()
        {
            var list = await GetRateList();
            await Throttler.RunAsync(list.Items, 100, 10, async rate =>
            {
                var rates = await currencyConversionService.Get(rate.Currency);
                await cloudBlobService.Save($"{rate.Currency}.json", JsonConvert.SerializeObject(rates));
            });
        }

        private async Task<ListPage<ConversionRate>> GetCachedRates(ListArgs<ConversionRate> rateArgs, CurrencyCode currency)
        {
            var rates = await cache.GetOrAddAsync($"exchangerates_{currency}", TimeSpan.FromHours(1), () =>
            {
                return cloudBlobService.Get<ConversionRates>($"{currency}.json");
            });
            return Filter(rateArgs, rates);
        }
    }
}
