using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Headstart.Models.Attributes;
using Microsoft.AspNetCore.Mvc;
using ordercloud.integrations.exchangerates;
using ordercloud.integrations.library;
using OrderCloud.Catalyst;
using OrderCloud.SDK;

namespace Headstart.Common.Controllers
{
    /// <summary>
    ///  Currency Conversion Charts
    /// </summary>
    [Route("exchangerates")]
    public class ExchangeRatesController : CatalystController
    {
        private readonly IExchangeRatesCommand _command;

        public ExchangeRatesController(IExchangeRatesCommand command) 
        {
            _command = command;
        }

        [HttpGet, Route("{currency}")]
        public async Task<ListPage<OrderCloudIntegrationsConversionRate>> Get(ListArgs<OrderCloudIntegrationsConversionRate> rateArgs, CurrencySymbol currency)
        {
            return await _command.Get(rateArgs, currency);
        }

        [HttpGet, Route("supportedrates")]
        public async Task<ListPage<OrderCloudIntegrationsConversionRate>> GetRateList()
        {
            var list = await _command.GetRateList();
            return list;
        }
    }
}
