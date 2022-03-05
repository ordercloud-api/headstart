using System;
using System.Linq;
using OrderCloud.SDK;
using Headstart.Models;
using OrderCloud.Catalyst;
using System.Threading.Tasks;
using System.Collections.Generic;
using ordercloud.integrations.exchangerates;

namespace Headstart.Common.Services
{
    public interface IHSExchangeRatesService
    {
        Task<CurrencySymbol> GetCurrencyForUser(string userToken);
        Task<List<OrderCloudIntegrationsConversionRate>> GetExchangeRatesForUser(string userToken);
    }

    public class HSExchangeRatesService : IHSExchangeRatesService
    {
        private readonly IOrderCloudClient _oc;
        private readonly IExchangeRatesCommand _exchangeRatesCommand;

        public HSExchangeRatesService(IOrderCloudClient oc, IExchangeRatesCommand exchangeRatesCommand)
        {
            _oc = oc;
            _exchangeRatesCommand = exchangeRatesCommand;
        }

        public async Task<CurrencySymbol> GetCurrencyForUser(string userToken)
        {
            ListPage<HSLocationUserGroup> buyerUserGroups = await _oc.Me.ListUserGroupsAsync<HSLocationUserGroup>(opts => opts.AddFilter(u => u.xp.Type.Equals("BuyerLocation", StringComparison.OrdinalIgnoreCase)), userToken);
            CurrencySymbol? currency = buyerUserGroups.Items.FirstOrDefault(u => u.xp.Currency != null)?.xp?.Currency;
            Require.That(currency != null, new ErrorCode($@"Exchange Rate Error", $@"The Exchange Rate was not defined for this User."));
            return (CurrencySymbol)currency;
        }

        public async Task<List<OrderCloudIntegrationsConversionRate>> GetExchangeRatesForUser(string userToken)
        {
            CurrencySymbol currency = await GetCurrencyForUser(userToken);
            ListPage<OrderCloudIntegrationsConversionRate> exchangeRates = await _exchangeRatesCommand.Get(new ListArgs<OrderCloudIntegrationsConversionRate>() { }, currency);
            return exchangeRates.Items.ToList();
        }
    }
}