using System;
using System.Linq;
using OrderCloud.SDK;
using OrderCloud.Catalyst;
using System.Threading.Tasks;
using System.Collections.Generic;
using Headstart.Common.Models.Headstart;
using ordercloud.integrations.exchangerates;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
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
		private readonly AppSettings _settings;

		/// <summary>
		/// The IOC based constructor method for the HsExchangeRatesService class object with Dependency Injection
		/// </summary>
		/// <param name="oc"></param>
		/// <param name="exchangeRatesCommand"></param>
		/// <param name="settings"></param>
		public HsExchangeRatesService(IOrderCloudClient oc, IExchangeRatesCommand exchangeRatesCommand, AppSettings settings)
		{
			try
			{
				_settings = settings;
				_oc = oc;
				_exchangeRatesCommand = exchangeRatesCommand;
			}
			catch (Exception ex)
			{
				LoggingNotifications.LogApiResponseMessages(_settings.LogSettings, SitecoreExtensions.Helpers.GetMethodName(), "",
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace, ex);
			}
		}

		/// <summary>
		/// Public re-usable GetCurrencyForUser task method
		/// </summary>
		/// <param name="userToken"></param>
		/// <returns>The CurrencySymbol object value from the GetCurrencyForUser process</returns>
		public async Task<CurrencySymbol> GetCurrencyForUser(string userToken)
		{
			var buyerUserGroups = await _oc.Me.ListUserGroupsAsync<HsLocationUserGroup>(opts => opts.AddFilter(u => u.xp.Type.Equals("BuyerLocation", StringComparison.OrdinalIgnoreCase)), userToken);
			var currency = buyerUserGroups.Items.FirstOrDefault(u => u.xp.Currency != null)?.xp?.Currency;
			Require.That(currency != null, new ErrorCode(@"Exchange Rate Error", @"The Exchange Rate was not defined for this User."));
			return (CurrencySymbol)currency;
		}

		/// <summary>
		/// Public re-usable GetExchangeRatesForUser task method
		/// </summary>
		/// <param name="userToken"></param>
		/// <returns>The list of OrderCloudIntegrationsConversionRate objects from the GetExchangeRatesForUser process</returns>
		public async Task<List<OrderCloudIntegrationsConversionRate>> GetExchangeRatesForUser(string userToken)
		{
			var currency = await GetCurrencyForUser(userToken);
			var exchangeRates = await _exchangeRatesCommand.Get(new ListArgs<OrderCloudIntegrationsConversionRate>() { }, currency);
			return exchangeRates != null && exchangeRates.Items != null ? exchangeRates.Items.ToList() : new List<OrderCloudIntegrationsConversionRate>();
		}
	}
}