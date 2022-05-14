using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Headstart.Common.Services;
using Headstart.Models;
using Headstart.Models.Misc;
using OrderCloud.Integrations.ExchangeRates;
using OrderCloud.SDK;
using OrderCloud.Catalyst;
using Headstart.Common;
using OrderCloud.Integrations.ExchangeRates.Models;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using SitecoreExtensions = Sitecore.Foundation.SitecoreExtensions.Extensions;

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
		private readonly IOrderCloudClient oc;
		private readonly IHSBuyerCommand hsBuyerCommand;
		private readonly ISendgridService sendgridService;
		private readonly ISimpleCache cache;
		private readonly IExchangeRatesCommand exchangeRatesCommand;
		private readonly AppSettings settings;

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
				
            	this.settings = settings;
				oc = elevatedOc;
				this.hsBuyerCommand = hsBuyerCommand;
	            this.sendgridService = sendgridService;
	            this.cache = cache;
	            this.exchangeRatesCommand = exchangeRatesCommand;
			}
			catch (Exception ex)
			{
				LoggingNotifications.LogApiResponseMessages(this.settings.LogSettings, SitecoreExtensions.Helpers.GetMethodName(), "",
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace, ex);
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
            var product = oc.Me.GetProductAsync<HSMeProduct>(id, sellerID: settings.OrderCloudSettings.MarketplaceID, accessToken: decodedToken.AccessToken);
            var specs = oc.Me.ListSpecsAsync(id, null, null, decodedToken.AccessToken);
            var variants = oc.Products.ListVariantsAsync<HSVariant>(id, null, null, null, 1, 100, null);
			var unconvertedSuperHsProduct = new SuperHSMeProduct 
			{
                Product = await product,
                PriceSchedule = (await product).PriceSchedule,
                Specs = (await specs).Items,
                Variants = (await variants).Items,
			};
			return await ApplyBuyerPricing(unconvertedSuperHsProduct, decodedToken);
		}
		
		/// <summary>
		/// Public re-usable get a list of ListPageWithFacets of HSMeProduct objects task method
		/// </summary>
		/// <param name="args"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The ListPageWithFacets of HSMeProduct objects</returns>
		public async Task<ListPageWithFacets<HSMeProduct>> List(ListArgs<HSMeProduct> args, DecodedToken decodedToken)
		{
            var searchText = args.Search ?? string.Empty;
            var searchFields = args.Search != null ? "ID,Name,Description,xp.Facets.supplier" : string.Empty;
			var sortBy = args.SortBy.FirstOrDefault();
			var filters = string.IsNullOrEmpty(args.ToFilterString()) ? null : args.ToFilterString();
            var meProducts = await oc.Me.ListProductsAsync<HSMeProduct>(filters: filters, page: args.Page, search: searchText, searchOn: searchFields, searchType: SearchType.ExactPhrasePrefix, sortBy: sortBy, sellerID: settings.OrderCloudSettings.MarketplaceID, accessToken: decodedToken.AccessToken);
            if (!(bool)meProducts?.Items?.Any())
			{
                meProducts = await oc.Me.ListProductsAsync<HSMeProduct>(filters: filters, page: args.Page, search: searchText, searchOn: searchFields, searchType: SearchType.AnyTerm, sortBy: sortBy, sellerID: settings.OrderCloudSettings.MarketplaceID, accessToken: decodedToken.AccessToken);
                if (!(bool)meProducts?.Items?.Any())
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
            await sendgridService.SendContactSupplierAboutProductEmail(template);
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
                        var currency = (CurrencySymbol?)CurrencySymbol.USD;
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
            var me = await oc.Me.GetAsync(accessToken: decodedToken.AccessToken);
			var buyerID = me.Buyer.ID;
            var buyer = await cache.GetOrAddAsync($"buyer_{buyerID}", TimeSpan.FromHours(1), () => hsBuyerCommand.Get(buyerID));

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
            var buyerUserGroups = await oc.Me.ListUserGroupsAsync<HSLocationUserGroup>(opts => opts.AddFilter(u => u.xp.Type == "BuyerLocation"), userToken);
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
            var exchangeRates = await exchangeRatesCommand.Get(new ListArgs<OrderCloudIntegrationsConversionRate>() { }, currency);
			return exchangeRates.Items.ToList();
		}
	}
}
