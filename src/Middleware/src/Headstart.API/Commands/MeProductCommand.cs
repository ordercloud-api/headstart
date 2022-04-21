using System;
using System.Linq;
using OrderCloud.SDK;
using Headstart.Common;
using OrderCloud.Catalyst;
using Sitecore.Diagnostics;
using System.Threading.Tasks;
using Headstart.Common.Services;
using System.Collections.Generic;
using Headstart.Common.Models.Misc;
using Headstart.Common.Models.Headstart;
using ordercloud.integrations.exchangerates;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Foundation.SitecoreExtensions.MVC.Extensions;

namespace Headstart.API.Commands
{
	public interface IMeProductCommand
	{
		Task<ListPageWithFacets<HsMeProduct>> List(ListArgs<HsMeProduct> args, DecodedToken decodedToken);
		Task<SuperHsMeProduct> Get(string id, DecodedToken decodedToken);
		Task RequestProductInfo(ContactSupplierBody template);
	}

	public class MeProductCommand : IMeProductCommand
	{
		private readonly IOrderCloudClient _oc;
		private readonly IHsBuyerCommand _HsBuyerCommand;
		private readonly ISendgridService _sendgridService;
		private readonly ISimpleCache _cache;
		private readonly IExchangeRatesCommand _exchangeRatesCommand;
		private readonly AppSettings _settings;
		private readonly ConfigSettings _configSettings = ConfigSettings.Instance;

		/// <summary>
		/// The IOC based constructor method for the MeProductCommand class object with Dependency Injection
		/// </summary>
		/// <param name="elevatedOc"></param>
		/// <param name="HsBuyerCommand"></param>
		/// <param name="sendgridService"></param>
		/// <param name="cache"></param>
		/// <param name="exchangeRatesCommand"></param>
		/// <param name="settings"></param>
		public MeProductCommand(IOrderCloudClient elevatedOc, IHsBuyerCommand HsBuyerCommand, ISendgridService sendgridService, ISimpleCache cache, IExchangeRatesCommand exchangeRatesCommand, AppSettings settings)
		{			
			try
			{
				_oc = elevatedOc;
				_HsBuyerCommand = HsBuyerCommand;
				_sendgridService = sendgridService;
				_cache = cache;
				_exchangeRatesCommand = exchangeRatesCommand;
				_settings = settings;
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Public re-usable Get SuperHsMeProduct task method
		/// </summary>
		/// <param name="id"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The SuperHsMeProduct response object from the Get SuperHsMeProduct process</returns>
		public async Task<SuperHsMeProduct> Get(string id, DecodedToken decodedToken)
		{
			var resp = new SuperHsMeProduct();
			try
			{
				var _product = _oc.Me.GetProductAsync<HsMeProduct>(id, sellerID: _settings.OrderCloudSettings.MarketplaceId, accessToken: decodedToken.AccessToken);
				var _specs = _oc.Me.ListSpecsAsync(id, null, null, decodedToken.AccessToken);
				var _variants = _oc.Products.ListVariantsAsync<HsVariant>(id, null, null, null, 1, 100, null);
				var unconvertedSuperHsProduct = new SuperHsMeProduct
				{
					Product = await _product,
					PriceSchedule = (await _product).PriceSchedule,
					Specs = (await _specs).Items,
					Variants = (await _variants).Items,
				};
				resp = await ApplyBuyerPricing(unconvertedSuperHsProduct, decodedToken);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Private re-usable ApplyBuyerPricing task method
		/// </summary>
		/// <param name="superHsProduct"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The SuperHsMeProduct response object from the ApplyBuyerPricing process</returns>
		private async Task<SuperHsMeProduct> ApplyBuyerPricing(SuperHsMeProduct superHsProduct, DecodedToken decodedToken)
		{
			try
			{
				var defaultMarkupMultiplierRequest = GetDefaultMarkupMultiplier(decodedToken);
				var exchangeRatesRequest = GetExchangeRatesForUser(decodedToken.AccessToken);
				await Task.WhenAll(defaultMarkupMultiplierRequest, exchangeRatesRequest);

				var defaultMarkupMultiplier = await defaultMarkupMultiplierRequest;
				var exchangeRates = await exchangeRatesRequest;

				var markedUpProduct = ApplyBuyerProductPricing(superHsProduct.Product, defaultMarkupMultiplier, exchangeRates);
				var productCurrency = superHsProduct.Product.xp.Currency ?? CurrencySymbol.USD;
				var markedUpSpecs = ApplySpecMarkups(superHsProduct.Specs.ToList(), productCurrency, exchangeRates);

				superHsProduct.Product = markedUpProduct;
				superHsProduct.Specs = markedUpSpecs;
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return superHsProduct;
		}

		/// <summary>
		/// Private re-usable ApplySpecMarkups method
		/// </summary>
		/// <param name="specs"></param>
		/// <param name="productCurrency"></param>
		/// <param name="exchangeRates"></param>
		/// <returns>The list of Spec response objects from the ApplySpecMarkups process</returns>
		private List<Spec> ApplySpecMarkups(List<Spec> specs, CurrencySymbol? productCurrency, List<OrderCloudIntegrationsConversionRate> exchangeRates)
		{
			var resp = new List<Spec>();
			try
			{
				resp = specs.Select(spec =>
				{
					spec.Options = spec.Options.Select(option =>
					{
						if (option.PriceMarkup != null)
						{
							var unconvertedMarkup = option.PriceMarkup ?? 0;
							option.PriceMarkup = ConvertPrice(unconvertedMarkup, productCurrency, exchangeRates);
						}
						return option;
					}).ToList();
					return spec;
				}).ToList();
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Public re-usable get a list of ListPageWithFacets of HsMeProduct response objects task method
		/// </summary>
		/// <param name="args"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The ListPageWithFacets of HsMeProduct response objects</returns>
		public async Task<ListPageWithFacets<HsMeProduct>> List(ListArgs<HsMeProduct> args, DecodedToken decodedToken)
		{
			var meProducts = new ListPageWithFacets<HsMeProduct>();
			try
			{
				var searchText = args.Search ?? "";
				var searchFields = args.Search != null ? "ID,Name,Description,xp.Facets.supplier" : "";
				var sortBy = args.SortBy.FirstOrDefault();
				var filters = string.IsNullOrEmpty(args.ToFilterString()) ? null : args.ToFilterString();
				meProducts = await _oc.Me.ListProductsAsync<HsMeProduct>(filters: filters, page: args.Page, search: searchText, searchOn: searchFields, searchType: SearchType.ExactPhrasePrefix, sortBy: sortBy, sellerID: _settings.OrderCloudSettings.MarketplaceId, accessToken: decodedToken.AccessToken);
				if (!(bool)meProducts?.Items?.Any())
				{
					meProducts = await _oc.Me.ListProductsAsync<HsMeProduct>(filters: filters, page: args.Page, search: searchText, searchOn: searchFields, searchType: SearchType.AnyTerm, sortBy: sortBy, sellerID: _settings.OrderCloudSettings.MarketplaceId, accessToken: decodedToken.AccessToken);
					if (!(bool)(meProducts?.Items?.Any()))
					{
						//if no products after retry search, avoid making extra calls for pricing details
						return meProducts;
					}
				}

				var defaultMarkupMultiplierRequest = GetDefaultMarkupMultiplier(decodedToken);
				var exchangeRatesRequest = GetExchangeRatesForUser(decodedToken.AccessToken);
				await Task.WhenAll(defaultMarkupMultiplierRequest, exchangeRatesRequest);
				var defaultMarkupMultiplier = await defaultMarkupMultiplierRequest;
				var exchangeRates = await exchangeRatesRequest;
				meProducts.Items = meProducts.Items.Select(product => ApplyBuyerProductPricing(product, defaultMarkupMultiplier, exchangeRates)).ToList();
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return meProducts;
		}

		/// <summary>
		/// Public re-usable RequestProductInfo task method
		/// </summary>
		/// <param name="template"></param>
		/// <returns></returns>
		public async Task RequestProductInfo(ContactSupplierBody template)
        {
			try
			{
				await _sendgridService.SendContactSupplierAboutProductEmail(template);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
        }

		/// <summary>
		/// Private re-usable ApplyBuyerProductPricing method
		/// </summary>
		/// <param name="product"></param>
		/// <param name="defaultMarkupMultiplier"></param>
		/// <param name="exchangeRates"></param>
		/// <returns>The HsMeProduct response object from the ApplyBuyerProductPricing process</returns>
		private HsMeProduct ApplyBuyerProductPricing(HsMeProduct product, decimal defaultMarkupMultiplier, List<OrderCloudIntegrationsConversionRate> exchangeRates)
		{
			try
			{
				if (product.PriceSchedule != null)
				{
					/* If the price schedule Id matches the product ID we 
					 * we mark up the produc
					 * if they don't match we just convert for currecny as the 
					 * seller has set custom pricing */
					var shouldMarkupProduct = product.PriceSchedule.ID == product.ID;
					if (shouldMarkupProduct)
					{
						product.PriceSchedule.PriceBreaks = product.PriceSchedule.PriceBreaks.Select(priceBreak =>
						{
							var markedUpPrice = Math.Round(priceBreak.Price * defaultMarkupMultiplier, 2); // round to 2 decimal places since we're dealing with price
							var currency = product?.xp?.Currency ?? CurrencySymbol.USD;
							var convertedPrice = ConvertPrice(markedUpPrice, currency, exchangeRates);
							priceBreak.Price = convertedPrice;
							return priceBreak;
						}).ToList();
					}
					else
					{
						product.PriceSchedule.PriceBreaks = product.PriceSchedule.PriceBreaks.Select(priceBreak =>
						{
							// Price on price schedule will be in USD as it is set by the seller
							// may be different rates in the future
							// refactor to save price on the price schedule not product xp?
							var currency = (Nullable<CurrencySymbol>)CurrencySymbol.USD;
							priceBreak.Price = ConvertPrice(priceBreak.Price, currency, exchangeRates);
							return priceBreak;
						}).ToList();
					}
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return product;
		}

		/// <summary>
		/// Private re-usable ConvertPrice method
		/// </summary>
		/// <param name="defaultPrice"></param>
		/// <param name="productCurrency"></param>
		/// <param name="exchangeRates"></param>
		/// <returns>The converted Price or defaultPrice decimal value from the ConvertPrice process</returns>
		private decimal ConvertPrice(decimal defaultPrice, CurrencySymbol? productCurrency, List<OrderCloudIntegrationsConversionRate> exchangeRates)
		{
			var price = defaultPrice;
			try
			{
				var exchangeRateForProduct = exchangeRates.Find(e => e.Currency == productCurrency).Rate;
				price = (defaultPrice / (decimal)exchangeRateForProduct);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return price;
		}

		/// <summary>
		/// Private re-usable GetDefaultMarkupMultiplier task method
		/// </summary>
		/// <param name="decodedToken"></param>
		/// <returns>The markupMultiplier decimal value from the GetDefaultMarkupMultiplier process</returns>
		private async Task<decimal> GetDefaultMarkupMultiplier(DecodedToken decodedToken)
		{
			decimal markupMultiplier = 0;
			try
			{
				var me = await _oc.Me.GetAsync(accessToken: decodedToken.AccessToken);
				var buyerId = me.Buyer.ID;
				var buyer = await _cache.GetOrAddAsync($@"buyer_{buyerId}", TimeSpan.FromHours(1), () => _HsBuyerCommand.Get(buyerId));

				// Must convert markup to decimal before division to prevent rounding error
				var markupPercent = ((decimal)buyer.Markup.Percent / 100);
				markupMultiplier = (markupPercent + 1);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return markupMultiplier;
		}

		/// <summary>
		/// Private re-usable GetCurrencyForUser task method
		/// </summary>
		/// <param name="userToken"></param>
		/// <returns>The CurrencySymbol response object from the GetCurrencyForUser process</returns>
		private async Task<CurrencySymbol> GetCurrencyForUser(string userToken)
		{
			var currency = new HsLocationUserGroupXp().Currency;
			try
			{
				var buyerUserGroups = await _oc.Me.ListUserGroupsAsync<HsLocationUserGroup>(opts => opts.AddFilter(u => u.xp.Type.Equals($@"BuyerLocation", StringComparison.OrdinalIgnoreCase)), userToken);
				currency = buyerUserGroups.Items.FirstOrDefault(u => u.xp.Currency != null)?.xp?.Currency;
				Require.That(currency != null, new ErrorCode($@"Exchange Rate Error", $@"The Exchange Rate is Not Defined for the User."));
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return (CurrencySymbol)currency;
		}

		/// <summary>
		/// Private re-usable GetExchangeRatesForUser task method
		/// </summary>
		/// <param name="userToken"></param>
		/// <returns>The list of OrderCloudIntegrationsConversionRate response objects from the GetExchangeRatesForUser process</returns>
		private async Task<List<OrderCloudIntegrationsConversionRate>> GetExchangeRatesForUser(string userToken)
		{
			var exchangeRates = new ListPage<OrderCloudIntegrationsConversionRate>();
			try
			{
				var currency = await GetCurrencyForUser(userToken);
				exchangeRates = await _exchangeRatesCommand.Get(new ListArgs<OrderCloudIntegrationsConversionRate>() { }, currency);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return exchangeRates.Items.ToList();
		}
	}
}