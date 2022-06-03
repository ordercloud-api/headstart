using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Headstart.Common.Commands;
using Headstart.Common.Models;
using Headstart.Models.Misc;
using OrderCloud.Catalyst;
using OrderCloud.Integrations.Emails;
using OrderCloud.Integrations.ExchangeRates;
using OrderCloud.Integrations.ExchangeRates.Models;
using OrderCloud.Integrations.Library.Models;
using OrderCloud.SDK;

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
        private readonly IEmailServiceProvider emailServiceProvider;
        private readonly ISimpleCache cache;
        private readonly IExchangeRatesCommand exchangeRatesCommand;
        private readonly AppSettings settings;

        public MeProductCommand(
            IOrderCloudClient elevatedOc,
            IHSBuyerCommand hsBuyerCommand,
            IEmailServiceProvider emailServiceProvider,
            ISimpleCache cache,
            IExchangeRatesCommand exchangeRatesCommand,
            AppSettings settings)
        {
            oc = elevatedOc;
            this.hsBuyerCommand = hsBuyerCommand;
            this.emailServiceProvider = emailServiceProvider;
            this.cache = cache;
            this.exchangeRatesCommand = exchangeRatesCommand;
            this.settings = settings;
        }

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
                    // if no products after retry search, avoid making extra calls for pricing details
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

        public async Task RequestProductInfo(ContactSupplierBody template)
        {
            await emailServiceProvider.SendContactSupplierAboutProductEmail(template);
        }

        private async Task<SuperHSMeProduct> ApplyBuyerPricing(SuperHSMeProduct superHsProduct, DecodedToken decodedToken)
        {
            var defaultMarkupMultiplierRequest = GetDefaultMarkupMultiplier(decodedToken);
            var exchangeRatesRequest = GetExchangeRatesForUser(decodedToken.AccessToken);
            await Task.WhenAll(defaultMarkupMultiplierRequest, exchangeRatesRequest);

            var defaultMarkupMultiplier = await defaultMarkupMultiplierRequest;
            var exchangeRates = await exchangeRatesRequest;

            var markedupProduct = ApplyBuyerProductPricing(superHsProduct.Product, defaultMarkupMultiplier, exchangeRates);
            var productCurrency = superHsProduct.Product.xp.Currency ?? CurrencyCode.USD;
            var markedupSpecs = ApplySpecMarkups(superHsProduct.Specs.ToList(), productCurrency, exchangeRates);

            superHsProduct.Product = markedupProduct;
            superHsProduct.Specs = markedupSpecs;
            return superHsProduct;
        }

        private List<Spec> ApplySpecMarkups(List<Spec> specs, CurrencyCode? productCurrency, List<ConversionRate> exchangeRates)
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

        private HSMeProduct ApplyBuyerProductPricing(HSMeProduct product, decimal defaultMarkupMultiplier, List<ConversionRate> exchangeRates)
        {
            if (product.PriceSchedule != null)
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
                        var currency = product?.xp?.Currency ?? CurrencyCode.USD;
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
                        var currency = (CurrencyCode?)CurrencyCode.USD;
                        priceBreak.Price = ConvertPrice(priceBreak.Price, currency, exchangeRates);
                        return priceBreak;
                    }).ToList();
                }
            }

            return product;
        }

        private decimal ConvertPrice(decimal defaultPrice, CurrencyCode? productCurrency, List<ConversionRate> exchangeRates)
        {
            var exchangeRateForProduct = exchangeRates.Find(e => e.Currency == productCurrency).Rate;
            var price = defaultPrice / (decimal)exchangeRateForProduct;
            return price;
        }

        private async Task<decimal> GetDefaultMarkupMultiplier(DecodedToken decodedToken)
        {
            var me = await oc.Me.GetAsync(accessToken: decodedToken.AccessToken);
            var buyerID = me.Buyer.ID;
            var buyer = await cache.GetOrAddAsync($"buyer_{buyerID}", TimeSpan.FromHours(1), () => hsBuyerCommand.Get(buyerID));

            // must convert markup to decimal before division to prevent rounding error
            var markupPercent = (decimal)buyer.Buyer.xp.MarkupPercent / 100;
            var markupMultiplier = markupPercent + 1;
            return markupMultiplier;
        }

        private async Task<CurrencyCode> GetCurrencyForUser(string userToken)
        {
            var buyerUserGroups = await oc.Me.ListUserGroupsAsync<HSLocationUserGroup>(opts => opts.AddFilter(u => u.xp.Type == "BuyerLocation"), userToken);
            var currency = buyerUserGroups.Items.FirstOrDefault(u => u.xp.Currency != null)?.xp?.Currency;
            Require.That(currency != null, new ErrorCode("Exchange Rate Error", "Exchange Rate Not Defined For User"));
            return (CurrencyCode)currency;
        }

        private async Task<List<ConversionRate>> GetExchangeRatesForUser(string userToken)
        {
            var currency = await GetCurrencyForUser(userToken);
            var exchangeRates = await exchangeRatesCommand.Get(new ListArgs<ConversionRate>() { }, currency);
            return exchangeRates.Items.ToList();
        }
    }
}
