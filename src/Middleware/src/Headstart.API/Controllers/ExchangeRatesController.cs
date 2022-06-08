using System.Threading.Tasks;
using Headstart.Common.Commands;
using Headstart.Common.Models;
using Microsoft.AspNetCore.Mvc;
using OrderCloud.Catalyst;
using OrderCloud.SDK;

namespace Headstart.Common.Controllers
{
    /// <summary>
    ///  Currency Conversion Charts.
    /// </summary>
    [Route("exchangerates")]
    public class ExchangeRatesController : CatalystController
    {
        private readonly ICurrencyConversionCommand currencyConversionCommand;

        public ExchangeRatesController(ICurrencyConversionCommand currencyConversionCommand)
        {
            this.currencyConversionCommand = currencyConversionCommand;
        }

        [HttpGet, Route("{currency}")]
        public async Task<ListPage<ConversionRate>> Get(ListArgs<ConversionRate> rateArgs, CurrencyCode currency)
        {
            return await currencyConversionCommand.Get(rateArgs, currency);
        }

        [HttpGet, Route("supportedrates")]
        public async Task<ListPage<ConversionRate>> GetRateList()
        {
            return await currencyConversionCommand.GetRateList();
        }
    }
}
