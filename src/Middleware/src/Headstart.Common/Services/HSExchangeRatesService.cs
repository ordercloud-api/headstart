using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Headstart.Common.Models;
using OrderCloud.Catalyst;
using OrderCloud.Integrations.ExchangeRates;
using OrderCloud.Integrations.ExchangeRates.Models;
using OrderCloud.Integrations.Library.Models;
using OrderCloud.SDK;

namespace Headstart.Common.Services
{
    public interface IHSExchangeRatesService
    {
        Task<CurrencyCode> GetCurrencyForUser(string userToken);

        Task<List<ConversionRate>> GetExchangeRatesForUser(string userToken);
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
            var exchangeRates = await exchangeRatesCommand.Get(new ListArgs<ConversionRate>() { }, currency);
            return exchangeRates.Items.ToList();
        }
    }
}
