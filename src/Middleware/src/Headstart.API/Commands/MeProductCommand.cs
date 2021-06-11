using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LazyCache;
using Headstart.Common.Services;
using Headstart.Models;
using Headstart.Models.Misc;
using ordercloud.integrations.exchangerates;
using ordercloud.integrations.library;
using OrderCloud.SDK;
using Headstart.API.Commands.Crud;
using Headstart.Models.Headstart;
using OrderCloud.Catalyst;

namespace Headstart.API.Commands
{
	public interface IMeProductCommand
	{
		Task<ListPageWithFacets<HSMeProduct>> List(ListArgs<HSMeProduct> args, VerifiedUserContext user);
		Task<SuperHSMeProduct> Get(string id, VerifiedUserContext user);
		Task RequestProductInfo(ContactSupplierBody template);
	}

	public class MeProductCommand : IMeProductCommand
	{
		private readonly IOrderCloudClient _oc;
		private readonly IHSBuyerCommand _hsBuyerCommand;
		private readonly IHSProductCommand _hsProductCommand;
		private readonly ISendgridService _sendgridService;
		private readonly ISimpleCache _cache;
		private readonly IExchangeRatesCommand _exchangeRatesCommand;
		public MeProductCommand(
			IOrderCloudClient elevatedOc, 
			IHSBuyerCommand hsBuyerCommand,
			IHSProductCommand hsProductCommand,
			ISendgridService sendgridService,
			ISimpleCache cache,
			IExchangeRatesCommand exchangeRatesCommand
		)
		{
			_oc = elevatedOc;
			_hsBuyerCommand = hsBuyerCommand;
			_hsProductCommand = hsProductCommand;
			_sendgridService = sendgridService;
			_cache = cache;
			_exchangeRatesCommand = exchangeRatesCommand;
		}
		public async Task<SuperHSMeProduct> Get(string id, VerifiedUserContext user)
		{
			var _product = _oc.Me.GetProductAsync<HSMeProduct>(id, user.AccessToken);
			var _specs = _oc.Me.ListSpecsAsync(id, null, null, user.AccessToken);
			var _variants = _oc.Products.ListVariantsAsync<HSVariant>(id, null, null, null, 1, 100, null);
			var unconvertedSuperHsProduct = new SuperHSMeProduct 
			{
				Product = await _product,
				PriceSchedule = (await _product).PriceSchedule,
				Specs = (await _specs).Items,
				Variants = (await _variants).Items,
			};
			return await ApplyBuyerPricing(unconvertedSuperHsProduct, user);
		}
		private async Task<SuperHSMeProduct> ApplyBuyerPricing(SuperHSMeProduct superHsProduct, VerifiedUserContext user)
		{
			var defaultMarkupMultiplierRequest = GetDefaultMarkupMultiplier(user);
			var exchangeRatesRequest = GetExchangeRatesForUser(user.AccessToken);
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

		public async Task<ListPageWithFacets<HSMeProduct>> List(ListArgs<HSMeProduct> args, VerifiedUserContext user)
		{
			var searchText = args.Search ?? "";
			var searchFields = args.Search!=null ? "ID,Name,Description,xp.Facets.supplier" : "";
			var sortBy = args.SortBy.FirstOrDefault();
			var meProducts = await _oc.Me.ListProductsAsync<HSMeProduct>(filters: args.ToFilterString(), page: args.Page, search: searchText, searchOn: searchFields, searchType: SearchType.ExactPhrasePrefix, sortBy: sortBy,  accessToken: user.AccessToken);
			if(!(bool)(meProducts?.Items?.Any()))
            {
				meProducts = await _oc.Me.ListProductsAsync<HSMeProduct>(filters: args.ToFilterString(), page: args.Page, search: searchText, searchOn: searchFields, searchType: SearchType.AnyTerm, sortBy: sortBy, accessToken: user.AccessToken);
				if (!(bool)(meProducts?.Items?.Any()))
                {
					//if no products after retry search, avoid making extra calls for pricing details
					return meProducts;
                }
			}

			var defaultMarkupMultiplierRequest = GetDefaultMarkupMultiplier(user);
			var exchangeRatesRequest = GetExchangeRatesForUser(user.AccessToken);
			await Task.WhenAll(defaultMarkupMultiplierRequest, exchangeRatesRequest);
			var defaultMarkupMultiplier = await defaultMarkupMultiplierRequest;
			var exchangeRates = await exchangeRatesRequest;
			meProducts.Items = meProducts.Items.Select(product => ApplyBuyerProductPricing(product, defaultMarkupMultiplier, exchangeRates)).ToList();

			return meProducts;
		}

		public async Task RequestProductInfo(ContactSupplierBody template)
        {
			await _sendgridService.SendContactSupplierAboutProductEmail(template);
        }

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

		private decimal ConvertPrice(decimal defaultPrice, CurrencySymbol? productCurrency, List<OrderCloudIntegrationsConversionRate> exchangeRates)
		{
			var exchangeRateForProduct = exchangeRates.Find(e => e.Currency == productCurrency).Rate;
			var price = defaultPrice / (decimal)exchangeRateForProduct;
			return price;
		}

		private async Task<decimal> GetDefaultMarkupMultiplier(VerifiedUserContext user)
		{
			var buyer = await _cache.GetOrAddAsync($"buyer_{user.Buyer.ID}", TimeSpan.FromHours(1), () => _hsBuyerCommand.Get(user.Buyer.ID));

			// must convert markup to decimal before division to prevent rouding error
			var markupPercent = (decimal)buyer.Markup.Percent / 100;
			var markupMultiplier = markupPercent + 1;
			return markupMultiplier;
		}

		private async Task<CurrencySymbol> GetCurrencyForUser(string userToken)
		{
			var buyerUserGroups = await _oc.Me.ListUserGroupsAsync<HSLocationUserGroup>(opts => opts.AddFilter(u => u.xp.Type == "BuyerLocation"), userToken);
			var currency = buyerUserGroups.Items.FirstOrDefault(u => u.xp.Currency != null)?.xp?.Currency;
			Require.That(currency != null, new ErrorCode("Exchange Rate Error", 400, "Exchange Rate Not Defined For User"));
			return (CurrencySymbol)currency;
		}

		private async Task<List<OrderCloudIntegrationsConversionRate>> GetExchangeRatesForUser(string userToken)
		{
			var currency = await GetCurrencyForUser(userToken);
			var exchangeRates = await _exchangeRatesCommand.Get(new ListArgs<OrderCloudIntegrationsConversionRate>() { }, currency);
			return exchangeRates.Items.ToList();
		}
	}
}
