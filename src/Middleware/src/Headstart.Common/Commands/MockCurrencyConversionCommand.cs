using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Headstart.Common.Extensions;
using Headstart.Common.Models;
using Headstart.Common.Services;
using OrderCloud.Catalyst;
using OrderCloud.SDK;

namespace Headstart.Common.Commands
{
    public class MockCurrencyConversionCommand : ICurrencyConversionCommand
    {
        private readonly ICurrencyConversionService currencyConversionService;

        public MockCurrencyConversionCommand(ICurrencyConversionService currencyConversionService)
        {
            this.currencyConversionService = currencyConversionService;
        }

        public Task<double?> ConvertCurrency(CurrencyCode from, CurrencyCode to, double value) => Task.FromResult((double?)value);

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

        public async Task<ListPage<ConversionRate>> Get(ListArgs<ConversionRate> rateArgs, CurrencyCode currency)
        {
            var rates = await currencyConversionService.Get(currency);
            return Filter(rateArgs, rates);
        }

        public Task<CurrencyCode> GetCurrencyForUser(string userToken) => Task.FromResult(CurrencyCode.USD);

        public async Task<List<ConversionRate>> GetExchangeRatesForUser(string userToken)
        {
            var rates = await Get(new ListArgs<ConversionRate>() { }, CurrencyCode.USD);
            return rates.Items.ToList();
        }

        public async Task<ListPage<ConversionRate>> GetRateList()
        {
            return await Get(new ListArgs<ConversionRate>() { }, CurrencyCode.USD);
        }

        public Task Update() => Task.CompletedTask;
    }
}
