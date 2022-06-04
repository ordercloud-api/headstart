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
        private readonly ICurrencyConversionCommand command;

        public ExchangeRatesController(ICurrencyConversionCommand command)
        {
            this.command = command;
        }

        [HttpGet, Route("{currency}")]
        public async Task<ListPage<ConversionRate>> Get(ListArgs<ConversionRate> rateArgs, CurrencyCode currency)
        {
            return await command.Get(rateArgs, currency);
        }

        [HttpGet, Route("supportedrates")]
        public async Task<ListPage<ConversionRate>> GetRateList()
        {
            var list = await command.GetRateList();
            return list;
        }
    }
}
