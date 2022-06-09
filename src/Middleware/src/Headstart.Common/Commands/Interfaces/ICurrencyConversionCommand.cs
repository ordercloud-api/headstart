using System.Collections.Generic;
using System.Threading.Tasks;
using Headstart.Common.Models;
using OrderCloud.Catalyst;
using OrderCloud.SDK;

namespace Headstart.Common.Commands
{
    public interface ICurrencyConversionCommand
    {
        Task<ListPage<ConversionRate>> Get(ListArgs<ConversionRate> rateArgs, CurrencyCode currency);

        Task<CurrencyCode> GetCurrencyForUser(string userToken);

        Task<List<ConversionRate>> GetExchangeRatesForUser(string userToken);

        Task<ListPage<ConversionRate>> GetRateList();

        ListPage<ConversionRate> Filter(ListArgs<ConversionRate> rateArgs, ConversionRates rates);

        Task<double?> ConvertCurrency(CurrencyCode from, CurrencyCode to, double value);

        Task Update();
    }
}
