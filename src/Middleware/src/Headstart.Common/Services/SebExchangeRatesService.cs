using Headstart.Models;
using OrderCloud.Integrations.ExchangeRates;
using OrderCloud.Catalyst;
using OrderCloud.SDK;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrderCloud.Integrations.ExchangeRates.Models;

namespace Headstart.Common.Services
{
    public interface IHSExchangeRatesService
    {
        Task<CurrencySymbol> GetCurrencyForUser(string userToken);

        Task<List<OrderCloudIntegrationsConversionRate>> GetExchangeRatesForUser(string userToken);
    }

    public class HSExchangeRatesService : IHSExchangeRatesService
    {
        private readonly IOrderCloudClient oc;
        private readonly IExchangeRatesCommand exchangeRatesCommand;

        public HSExchangeRatesService(IOrderCloudClient oc, IExchangeRatesCommand exchangeRatesCommand)
        {
            this.oc = oc;
            this.exchangeRatesCommand = exchangeRatesCommand;
        }

        public async Task<CurrencySymbol> GetCurrencyForUser(string userToken)
        {
            var buyerUserGroups = await oc.Me.ListUserGroupsAsync<HSLocationUserGroup>(opts => opts.AddFilter(u => u.xp.Type == "BuyerLocation"), userToken);
            var currency = buyerUserGroups.Items.FirstOrDefault(u => u.xp.Currency != null)?.xp?.Currency;
            Require.That(currency != null, new ErrorCode("Exchange Rate Error", "Exchange Rate Not Defined For User"));
            return (CurrencySymbol)currency;
        }

        public async Task<List<OrderCloudIntegrationsConversionRate>> GetExchangeRatesForUser(string userToken)
        {
            var currency = await GetCurrencyForUser(userToken);
            var exchangeRates = await exchangeRatesCommand.Get(new ListArgs<OrderCloudIntegrationsConversionRate>() { }, currency);
            return exchangeRates.Items.ToList();
        }
    }
}
