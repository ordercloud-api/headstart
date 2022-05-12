using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ordercloud.integrations.exchangerates;
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
        private readonly IExchangeRatesCommand command;

        public ExchangeRatesController(IExchangeRatesCommand command)
        {
            this.command = command;
        }

        [HttpGet, Route("{currency}")]
        public async Task<ListPage<OrderCloudIntegrationsConversionRate>> Get(ListArgs<OrderCloudIntegrationsConversionRate> rateArgs, CurrencySymbol currency)
        {
            return await command.Get(rateArgs, currency);
        }

        [HttpGet, Route("supportedrates")]
        public async Task<ListPage<OrderCloudIntegrationsConversionRate>> GetRateList()
        {
            var list = await command.GetRateList();
            return list;
        }
    }
}
