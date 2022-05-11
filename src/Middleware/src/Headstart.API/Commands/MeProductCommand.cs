using Headstart.Common;
using Headstart.Common.Services;
using Headstart.Models;
using Headstart.Models.Misc;
using ordercloud.integrations.exchangerates;
using OrderCloud.Catalyst;
using OrderCloud.SDK;
using Sitecore.Diagnostics;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Headstart.API.Commands
{
	public interface IMeProductCommand
	{
		Task<ListPageWithFacets<HSMeProduct>> List(ListArgs<HSMeProduct> args, DecodedToken decodedToken);
		Task<SuperHSMeProduct> Get(string id, DecodedToken decodedToken);
		Task RequestProductInfo(ContactSupplierBody template);
	}

	public class MeProductCommand : IMeProductCommand
	{
		private readonly IOrderCloudClient _oc;
		private readonly IHSBuyerCommand _hsBuyerCommand;
		private readonly ISendgridService _sendgridService;
		private readonly ISimpleCache _cache;
		private readonly IExchangeRatesCommand _exchangeRatesCommand;
		private readonly AppSettings _settings;

		/// <summary>
		/// The IOC based constructor method for the MeProductCommand class object with Dependency Injection
		/// </summary>
		/// <param name="elevatedOc"></param>
		/// <param name="HsBuyerCommand"></param>
		/// <param name="sendgridService"></param>
		/// <param name="cache"></param>
		/// <param name="exchangeRatesCommand"></param>
		/// <param name="settings"></param>
		public MeProductCommand(
			IOrderCloudClient elevatedOc, 
			IHSBuyerCommand hsBuyerCommand, 
			ISendgridService sendgridService, 
			ISimpleCache cache, 
			IExchangeRatesCommand exchangeRatesCommand, 
			AppSettings settings)
		{			
			try
			{
				_oc = elevatedOc;
				_hsBuyerCommand = hsBuyerCommand;
				_sendgridService = sendgridService;
				_cache = cache;
				_exchangeRatesCommand = exchangeRatesCommand;
				_settings = settings;
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Public re-usable Get SuperHSMeProduct task method
		/// </summary>
		/// <param name="id"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The SuperHSMeProduct object from the Get SuperHSMeProduct process</returns>
		public async Task<SuperHSMeProduct> Get(string id, DecodedToken decodedToken)
		{
			var _product = _oc.Me.GetProductAsync<HSMeProduct>(id, sellerID: _settings.OrderCloudSettings.MarketplaceID, accessToken: decodedToken.AccessToken);
			var _specs = _oc.Me.ListSpecsAsync(id, null, null, decodedToken.AccessToken);
			var _variants = _oc.Products.ListVariantsAsync<HSVariant>(id, null, null, null, 1, 100, null);
			var unconvertedSuperHsProduct = new SuperHSMeProduct 
			{
				Product = await _product,
				PriceSchedule = (await _product).PriceSchedule,
				Specs = (await _specs).Items,
				Variants = (await _variants).Items,
			};
			return await ApplyBuyerPricing(unconvertedSuperHsProduct, decodedToken);
		}

		/// <summary>
		/// Private re-usable ApplyBuyerPricing task method
		/// </summary>
		/// <param name="superHsProduct"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The SuperHSMeProduct object from the ApplyBuyerPricing process</returns>
		private async Task<SuperHSMeProduct> ApplyBuyerPricing(SuperHSMeProduct superHsProduct, DecodedToken decodedToken)
		{
			var defaultMarkupMultiplierRequest = GetDefaultMarkupMultiplier(decodedToken);
			var exchangeRatesRequest = GetExchangeRatesForUser(decodedToken.AccessToken);
			await Task.WhenAll(defaultMarkupMultiplierRequest, exchangeRatesRequest);

			var defaultMarkupMultiplier = await defaultMarkupMultiplierRequest;
			var exchangeRates = await exchangeRatesRequest;

			var markedupProduct = ApplyBuyerProductPricing(superHsProduct.Product, defaultMarkupMultiplier, exchangeRates);
			var productCurrency = superHsProduct.Product.xp.Currency ?? CurrencySymbol.USD;
			var markedupSpecs = ApplySpecMarkups(superHsProduct.Specs.ToList(), productCurrency, exchangeRates);
		
			superHsProduct.Product = markedupProduct;
			superHsProduct.Specs = markedupSpecs;
			return superHsProduct;
		}

		/// <summary>
		/// Private re-usable ApplySpecMarkups method
		/// </summary>
		/// <param name="specs"></param>
		/// <param name="productCurrency"></param>
		/// <param name="exchangeRates"></param>
		/// <returns>The list of Spec objects from the ApplySpecMarkups process</returns>
		private List<Spec> ApplySpecMarkups(List<Spec> specs, CurrencySymbol? productCurrency, List<OrderCloudIntegrationsConversionRate> exchangeRates)
		{
			return specs.Select(spec =>
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

		/// <summary>
		/// Public re-usable get a list of ListPageWithFacets of HSMeProduct objects task method
		/// </summary>
		/// <param name="args"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The ListPageWithFacets of HSMeProduct objects</returns>
		public async Task<ListPageWithFacets<HSMeProduct>> List(ListArgs<HSMeProduct> args, DecodedToken decodedToken)
		{
			var searchText = args.Search ?? "";
			var searchFields = args.Search!=null ? "ID,Name,Description,xp.Facets.supplier" : "";
			var sortBy = args.SortBy.FirstOrDefault();
			var filters = string.IsNullOrEmpty(args.ToFilterString()) ? null : args.ToFilterString();
			var meProducts = await _oc.Me.ListProductsAsync<HSMeProduct>(filters: filters, page: args.Page, search: searchText, searchOn: searchFields, searchType: SearchType.ExactPhrasePrefix, sortBy: sortBy, sellerID: _settings.OrderCloudSettings.MarketplaceID, accessToken: decodedToken.AccessToken);
			if(!(bool)(meProducts?.Items?.Any()))
			{
				meProducts = await _oc.Me.ListProductsAsync<HSMeProduct>(filters: filters, page: args.Page, search: searchText, searchOn: searchFields, searchType: SearchType.AnyTerm, sortBy: sortBy, sellerID: _settings.OrderCloudSettings.MarketplaceID, accessToken: decodedToken.AccessToken);
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

			return meProducts;
		}

		/// <summary>
		/// Public re-usable RequestProductInfo task method
		/// </summary>
		/// <param name="template"></param>
		/// <returns></returns>
		public async Task RequestProductInfo(ContactSupplierBody template)
		{
			await _sendgridService.SendContactSupplierAboutProductEmail(template);
		}

		/// <summary>
		/// Private re-usable ApplyBuyerProductPricing method
		/// </summary>
		/// <param name="product"></param>
		/// <param name="defaultMarkupMultiplier"></param>
		/// <param name="exchangeRates"></param>
		/// <returns>The HSMeProduct object from the ApplyBuyerProductPricing process</returns>
		private HSMeProduct ApplyBuyerProductPricing(HSMeProduct product, decimal defaultMarkupMultiplier, List<OrderCloudIntegrationsConversionRate> exchangeRates)
		{
			
			if(product.PriceSchedule != null)
			{
				/* if the price schedule Id matches the product ID we 
				 * we mark up the produc
				 * if they dont match we just convert for currecny as the 
				 * seller has set custom pricing */
				var shouldMarkupProduct = product.PriceSchedule.ID == product.ID;
				if (shouldMarkupProduct)
				{
					product.PriceSchedule.PriceBreaks = product.PriceSchedule.PriceBreaks.Select(priceBreak =>
					{
						var markedupPrice = Math.Round(priceBreak.Price * defaultMarkupMultiplier, 2); // round to 2 decimal places since we're dealing with price
						var currency = product?.xp?.Currency ?? CurrencySymbol.USD;
						var convertedPrice = ConvertPrice(markedupPrice, currency, exchangeRates);
						priceBreak.Price = convertedPrice;
						return priceBreak;
					}).ToList();
				}
				else
				{
					product.PriceSchedule.PriceBreaks = product.PriceSchedule.PriceBreaks.Select(priceBreak =>
					{
						// price on price schedule will be in USD as it is set by the seller
						// may be different rates in the future
						// refactor to save price on the price schedule not product xp?
						var currency = (Nullable<CurrencySymbol>)CurrencySymbol.USD;
						priceBreak.Price = ConvertPrice(priceBreak.Price, currency, exchangeRates);
						return priceBreak;
					}).ToList();
				}
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
			var exchangeRateForProduct = exchangeRates.Find(e => e.Currency == productCurrency).Rate;
			var price = defaultPrice / (decimal)exchangeRateForProduct;
			return price;
		}

		/// <summary>
		/// Private re-usable GetDefaultMarkupMultiplier task method
		/// </summary>
		/// <param name="decodedToken"></param>
		/// <returns>The markupMultiplier decimal value from the GetDefaultMarkupMultiplier process</returns>
		private async Task<decimal> GetDefaultMarkupMultiplier(DecodedToken decodedToken)
		{
			var me = await _oc.Me.GetAsync(accessToken: decodedToken.AccessToken);
			var buyerID = me.Buyer.ID;
			var buyer = await _cache.GetOrAddAsync($"buyer_{buyerID}", TimeSpan.FromHours(1), () => _hsBuyerCommand.Get(buyerID));

			// must convert markup to decimal before division to prevent rouding error
			var markupPercent = (decimal)buyer.Markup.Percent / 100;
			var markupMultiplier = markupPercent + 1;
			return markupMultiplier;
		}

		/// <summary>
		/// Private re-usable GetCurrencyForUser task method
		/// </summary>
		/// <param name="userToken"></param>
		/// <returns>The CurrencySymbol object from the GetCurrencyForUser process</returns>
		private async Task<CurrencySymbol> GetCurrencyForUser(string userToken)
		{
			var buyerUserGroups = await _oc.Me.ListUserGroupsAsync<HSLocationUserGroup>(opts => opts.AddFilter(u => u.xp.Type == "BuyerLocation"), userToken);
			var currency = buyerUserGroups.Items.FirstOrDefault(u => u.xp.Currency != null)?.xp?.Currency;
			Require.That(currency != null, new ErrorCode("Exchange Rate Error", "Exchange Rate Not Defined For User"));
			return (CurrencySymbol)currency;
		}

		/// <summary>
		/// Private re-usable GetExchangeRatesForUser task method
		/// </summary>
		/// <param name="userToken"></param>
		/// <returns>The list of OrderCloudIntegrationsConversionRate objects from the GetExchangeRatesForUser process</returns>
		private async Task<List<OrderCloudIntegrationsConversionRate>> GetExchangeRatesForUser(string userToken)
		{
			var currency = await GetCurrencyForUser(userToken);
			var exchangeRates = await _exchangeRatesCommand.Get(new ListArgs<OrderCloudIntegrationsConversionRate>() { }, currency);
			return exchangeRates.Items.ToList();
		}
	}
}