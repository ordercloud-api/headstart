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

namespace Headstart.API.Commands
{
	public interface IMeProductCommand
	{
		Task<ListPageWithFacets<HSMeProduct>> List(ListArgs<HSMeProduct> args, VerifiedUserContext user);
		Task<SuperHSMeProduct> Get(string id, VerifiedUserContext user);
		Task RequestProductInfo(ContactSupplierBody template);
		Task<HSMeKitProduct> ApplyBuyerPricing(HSMeKitProduct kitProduct, VerifiedUserContext user);
	}

	public class MeProductCommand : IMeProductCommand
	{
		private readonly IOrderCloudClient _oc;
		private readonly IHSBuyerCommand _hsBuyerCommand;
		private readonly IHSProductCommand _hsProductCommand;
		private readonly ISendgridService _sendgridService;
		private readonly IAppCache _cache;
		public MeProductCommand(
			IOrderCloudClient elevatedOc, 
			IHSBuyerCommand hsBuyerCommand,
			IHSProductCommand hsProductCommand,
			ISendgridService sendgridService,
			IAppCache cache
		)
		{
			_oc = elevatedOc;
			_hsBuyerCommand = hsBuyerCommand;
			_hsProductCommand = hsProductCommand;
			_sendgridService = sendgridService;
			_cache = cache;
		}
		public async Task<SuperHSMeProduct> Get(string id, VerifiedUserContext user)
		{
			var _product = _oc.Me.GetProductAsync<HSMeProduct>(id, user.AccessToken);
			var _specs = _oc.Me.ListSpecsAsync(id, null, null, user.AccessToken);
			var _variants = _oc.Products.ListVariantsAsync<HSVariant>(id, null, null, null, 1, 100, null);
			var _images = _hsProductCommand.GetProductImages(id, user.AccessToken);
			var _attachments = _hsProductCommand.GetProductAttachments(id, user.AccessToken);
			var unconvertedSuperHsProduct = new SuperHSMeProduct 
			{
				Product = await _product,
				PriceSchedule = (await _product).PriceSchedule,
				Specs = (await _specs).Items,
				Variants = (await _variants).Items,
				Images = await _images,
				Attachments = await _attachments
			};
			return await ApplyBuyerPricing(unconvertedSuperHsProduct, user);
		}

		private async Task<SuperHSMeProduct> ApplyBuyerPricing(SuperHSMeProduct superHsProduct, VerifiedUserContext user)
		{
			var defaultMarkupMultiplierRequest = GetDefaultMarkupMultiplier(user);

			var defaultMarkupMultiplier = await defaultMarkupMultiplierRequest;

			var markedupProduct = ApplyBuyerProductPricing(superHsProduct.Product, defaultMarkupMultiplier);
			var productCurrency = superHsProduct.Product.xp.Currency;
			var markedupSpecs = ApplySpecMarkups(superHsProduct.Specs.ToList(), defaultMarkupMultiplier, productCurrency);
		
			superHsProduct.Product = markedupProduct;
			superHsProduct.Specs = markedupSpecs;
			return superHsProduct;
		}

		public async Task<HSMeKitProduct> ApplyBuyerPricing(HSMeKitProduct kitProduct, VerifiedUserContext user)
		{
			var defaultMarkupMultiplierRequest = GetDefaultMarkupMultiplier(user);

			var defaultMarkupMultiplier = await defaultMarkupMultiplierRequest;

			foreach(var kit in kitProduct.ProductAssignments.ProductsInKit)
            {
				var markedupProduct = ApplyBuyerProductPricing(kit.Product, defaultMarkupMultiplier);
				var productCurrency = kit.Product.xp.Currency;
				var markedupSpecs = ApplySpecMarkups(kit.Specs.ToList(), defaultMarkupMultiplier, productCurrency);
				kit.Product = markedupProduct;
				kit.Specs = markedupSpecs;
			}

			return kitProduct;
		}

		private List<Spec> ApplySpecMarkups(List<Spec> specs, decimal defaultMarkupMultiplier, CurrencySymbol? productCurrency)
		{
			return specs.Select(spec =>
			{
				spec.Options = spec.Options.Select(option =>
				{
					if (option.PriceMarkup != null)
					{
						var unconvertedMarkup = option.PriceMarkup ?? 0;
						option.PriceMarkup = ConvertPrice(unconvertedMarkup, productCurrency);
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

			var defaultMarkupMultiplier = await GetDefaultMarkupMultiplier(user);
			meProducts.Items = meProducts.Items.Select(product => ApplyBuyerProductPricing(product, defaultMarkupMultiplier)).ToList();

			return meProducts;
		}

		public async Task RequestProductInfo(ContactSupplierBody template)
        {
			await _sendgridService.SendContactSupplierAboutProductEmail(template);
        }

		private HSMeProduct ApplyBuyerProductPricing(HSMeProduct product, decimal defaultMarkupMultiplier)
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
						var convertedPrice = ConvertPrice(markedupPrice, currency);
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
						priceBreak.Price = ConvertPrice(priceBreak.Price, currency);
						return priceBreak;
					}).ToList();
				}
			}
			return product;
		}

        private decimal ConvertPrice(decimal defaultPrice, CurrencySymbol? productCurrency, List<OrderCloudIntegrationsConversionRate> exchangeRates = null)
        {
            var exchangeRateForProduct = exchangeRates.Find(e => e.Currency == productCurrency).Rate;
            return defaultPrice / (decimal)(exchangeRateForProduct ?? 1);
        }

        private async Task<decimal> GetDefaultMarkupMultiplier(VerifiedUserContext user)
		{
			var buyer = await _cache.GetOrAddAsync($"buyer_{user.BuyerID}", () => _hsBuyerCommand.Get(user.BuyerID), TimeSpan.FromHours(1));

			// must convert markup to decimal before division to prevent rouding error
			var markupPercent = (decimal)buyer.Markup.Percent / 100;
			var markupMultiplier = markupPercent + 1;
			return markupMultiplier;
		}

	}
}
