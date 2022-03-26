using System;
using System.Linq;
using OrderCloud.SDK;
using OrderCloud.Catalyst;
using System.Threading.Tasks;
using System.Collections.Generic;
using Headstart.Common.Models.Headstart;
using ordercloud.integrations.exchangerates;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Foundation.SitecoreExtensions.MVC.Extensions;
using SitecoreExtensions = Sitecore.Foundation.SitecoreExtensions.Extensions;

namespace Headstart.Common.Services
{
	public interface IHsExchangeRatesService
	{
		Task<CurrencySymbol> GetCurrencyForUser(string userToken);
		Task<List<OrderCloudIntegrationsConversionRate>> GetExchangeRatesForUser(string userToken);
	}

	public class HsExchangeRatesService : IHsExchangeRatesService
	{
		private readonly IOrderCloudClient _oc;
		private readonly IExchangeRatesCommand _exchangeRatesCommand;
		private readonly ConfigSettings _configSettings = ConfigSettings.Instance;

		public HsExchangeRatesService(IOrderCloudClient oc, IExchangeRatesCommand exchangeRatesCommand)
		{
			try
			{
				_oc = oc;
				_exchangeRatesCommand = exchangeRatesCommand;
			}
			catch (Exception ex)
			{
				LoggingNotifications.LogApiResponseMessages(_configSettings.AppLogFileKey, SitecoreExtensions.Helpers.GetMethodName(), "",
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace);
			}
		}

		public async Task<CurrencySymbol> GetCurrencyForUser(string userToken)
		{
			var buyerUserGroups = await _oc.Me.ListUserGroupsAsync<HsLocationUserGroup>(opts => opts.AddFilter(u => u.xp.Type.Equals("BuyerLocation", StringComparison.OrdinalIgnoreCase)), userToken);
			var currency = buyerUserGroups.Items.FirstOrDefault(u => u.xp.Currency != null)?.xp?.Currency;
			Require.That(currency != null, new ErrorCode(@"Exchange Rate Error", @"The Exchange Rate was not defined for this User."));
			return (CurrencySymbol)currency;
		}

		public async Task<List<OrderCloudIntegrationsConversionRate>> GetExchangeRatesForUser(string userToken)
		{
			var currency = await GetCurrencyForUser(userToken);
			var exchangeRates = await _exchangeRatesCommand.Get(new ListArgs<OrderCloudIntegrationsConversionRate>() { }, currency);
			return exchangeRates != null && exchangeRates.Items != null ? exchangeRates.Items.ToList() : new List<OrderCloudIntegrationsConversionRate>();
		}
	}
}