using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Headstart.Common.Constants;
using Headstart.Common.Extensions;
using Headstart.Common.Models;
using Headstart.Common.Services;
using Headstart.Common.Settings;
using OrderCloud.Catalyst;
using OrderCloud.Integrations.EasyPost;
using OrderCloud.Integrations.EasyPost.Models;
using OrderCloud.Integrations.ExchangeRates;
using OrderCloud.Integrations.Library;
using OrderCloud.Integrations.Library.Interfaces;
using OrderCloud.Integrations.Library.Models;
using OrderCloud.SDK;
using ITaxCalculator = OrderCloud.Integrations.Taxation.Interfaces.ITaxCalculator;

namespace Headstart.API.Commands
{
    public interface ICheckoutIntegrationCommand
    {
        Task<ShipEstimateResponse> GetRatesAsync(HSOrderCalculatePayload orderCalculatePayload);

        Task<HSOrderCalculateResponse> CalculateOrder(HSOrderCalculatePayload orderCalculatePayload);

        Task<HSOrderCalculateResponse> CalculateOrder(string orderID, DecodedToken decodedToken);

        Task<ShipEstimateResponse> GetRatesAsync(string orderID);
    }

    public class CheckoutIntegrationCommand : ICheckoutIntegrationCommand
    {
        private readonly ITaxCalculator taxCalculator;
        private readonly IShippingService shippingService;
        private readonly IExchangeRatesCommand exchangeRates;
        private readonly IOrderCloudClient oc;
        private readonly IDiscountDistributionService discountDistribution;
        private readonly AppSettings settings;

        public CheckoutIntegrationCommand(
            IDiscountDistributionService discountDistribution,
            ITaxCalculator taxCalculator,
            IExchangeRatesCommand exchangeRates,
            IOrderCloudClient orderCloud,
            IShippingService shippingService,
            AppSettings settings)
        {
            this.taxCalculator = taxCalculator;
            this.exchangeRates = exchangeRates;
            oc = orderCloud;
            this.shippingService = shippingService;
            this.settings = settings;
            this.discountDistribution = discountDistribution;
        }

        public static IEnumerable<HSShipMethod> WhereRateIsCheapestOfItsKind(IEnumerable<HSShipMethod> methods)
        {
            return methods
                .GroupBy(method => method.EstimatedTransitDays)
                .Select(kind => kind.OrderBy(method => method.Cost).First());
        }

        public static IList<HSShipEstimate> FilterSlowerRatesWithHighCost(IList<HSShipEstimate> estimates)
        {
            // filter out rate estimates with slower transit days and higher costs than faster transit days
            // ex: 3 days for $20 vs 1 day for $10. Filter out the 3 days option
            var result = estimates.Select(estimate =>
            {
                var methodsList = estimate.ShipMethods.OrderBy(m => m.EstimatedTransitDays).ToList();
                var filtered = new List<HSShipMethod>();
                for (var i = methodsList.Count - 1; i >= 0; i--)
                {
                    var method = methodsList[i];
                    var fasterMethods = methodsList.GetRange(0, i);
                    var existsFasterCheaperRate = fasterMethods.Any(m => m.Cost < method.Cost);
                    if (!existsFasterCheaperRate)
                    {
                        filtered.Add(method);
                    }
                }

                filtered.Reverse(); // reorder back to original since we looped backwards
                estimate.ShipMethods = filtered;
                return estimate;
            }).ToList();

            return result;
        }

        public static IList<HSShipEstimate> ApplyFlatRateShipping(HSOrderWorksheet orderWorksheet, IList<HSShipEstimate> estimates)
        {
            var result = estimates.Select(estimate => ApplyFlatRateShippingOnEstimate(estimate, orderWorksheet)).ToList();
            return result;
        }

        public static HSShipEstimate ApplyFlatRateShippingOnEstimate(HSShipEstimate estimate, HSOrderWorksheet orderWorksheet)
        {
            var supplierID = orderWorksheet.LineItems.First(li => li.ID == estimate.ShipEstimateItems.FirstOrDefault()?.LineItemID).SupplierID;

            var supplierLineItems = orderWorksheet.LineItems.Where(li => li.SupplierID == supplierID);
            var supplierSubTotal = supplierLineItems.Select(li => li.LineSubtotal).Sum();
            var qualifiesForFlatRateShipping = supplierSubTotal > .01M && estimate.ShipMethods.Any(method => method.Name.Contains("GROUND"));
            if (qualifiesForFlatRateShipping)
            {
                estimate.ShipMethods = estimate.ShipMethods
                                        .Where(method => method.Name.Contains("GROUND")) // flat rate shipping only applies to ground shipping methods
                                        .Select(method => ApplyFlatRateShippingOnShipmethod(method, supplierSubTotal))
                                        .ToList();
            }

            return estimate;
        }

        public static HSShipMethod ApplyFlatRateShippingOnShipmethod(HSShipMethod method, decimal supplierSubTotal)
        {
            if (supplierSubTotal > .01M && supplierSubTotal <= 499.99M)
            {
                method.Cost = 29.99M;
            }
            else if (supplierSubTotal > 499.9M)
            {
                method.Cost = 0;
                method.xp.FreeShippingApplied = true;
            }

            return method;
        }

        public static IList<HSShipEstimate> CheckForEmptyRates(IList<HSShipEstimate> estimates, decimal noRatesCost, int noRatesTransitDays)
        {
            // if there are no rates for a set of line items then return a mocked response so user can check out
            // this order will additionally get marked as needing attention
            foreach (var shipEstimate in estimates)
            {
                if (!shipEstimate.ShipMethods.Any())
                {
                    shipEstimate.ShipMethods = new List<HSShipMethod>()
                    {
                        new HSShipMethod
                        {
                            ID = ShippingConstants.NoRatesID,
                            Name = "No shipping rates",
                            Cost = noRatesCost,
                            EstimatedTransitDays = noRatesTransitDays,
                            xp = new ShipMethodXP
                            {
                                OriginalCost = noRatesCost,
                            },
                        },
                    };
                }
            }

            return estimates;
        }

        public static IList<HSShipEstimate> UpdateFreeShippingRates(IList<HSShipEstimate> estimates, int freeShippingTransitDays)
        {
            foreach (var shipEstimate in estimates)
            {
                if (shipEstimate.ID.StartsWith(ShippingConstants.FreeShippingID))
                {
                    foreach (var method in shipEstimate.ShipMethods)
                    {
                        method.ID = shipEstimate.ID;
                        method.Cost = 0;
                        method.EstimatedTransitDays = freeShippingTransitDays;
                    }
                }
            }

            return estimates;
        }

        public async Task<ShipEstimateResponse> GetRatesAsync(HSOrderCalculatePayload orderCalculatePayload)
        {
            return await this.GetRatesAsync(orderCalculatePayload.OrderWorksheet, orderCalculatePayload.ConfigData);
        }

        public async Task<ShipEstimateResponse> GetRatesAsync(string orderID)
        {
            var order = await oc.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.Incoming, orderID);
            return await this.GetRatesAsync(order);
        }

        public IEnumerable<HSShipMethod> FilterMethodsBySupplierConfig(List<HSShipMethod> methods, EasyPostShippingProfile profile)
        {
            // will attempt to filter out by supplier method specs, but if there are filters and the result is none and there are valid methods still return the methods
            if (profile.AllowedServiceFilter.Count == 0)
            {
                return methods;
            }

            var filtered_methods = methods.Where(s => profile.AllowedServiceFilter.Contains(s.Name)).Select(s => s).ToList();
            return filtered_methods.Any() ? filtered_methods : methods;
        }

        public async Task<HSOrderCalculateResponse> CalculateOrder(string orderID, DecodedToken decodedToken)
        {
            var worksheet = await oc.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.Incoming, orderID, decodedToken.AccessToken);
            return await this.CalculateOrder(new HSOrderCalculatePayload()
            {
                ConfigData = null,
                OrderWorksheet = worksheet,
            });
        }

        public async Task<HSOrderCalculateResponse> CalculateOrder(HSOrderCalculatePayload orderCalculatePayload)
        {
            if (orderCalculatePayload.OrderWorksheet.Order.xp != null && orderCalculatePayload.OrderWorksheet.Order.xp.OrderType == OrderType.Quote)
            {
                // quote orders do not have tax cost associated with them
                return new HSOrderCalculateResponse();
            }
            else
            {
                var promotions = await oc.Orders.ListAllPromotionsAsync(OrderDirection.All, orderCalculatePayload.OrderWorksheet.Order.ID);
                var promoCalculationTask = discountDistribution.SetLineItemProportionalDiscount(orderCalculatePayload.OrderWorksheet, promotions);
                var taxCalculationTask = taxCalculator.CalculateEstimateAsync(orderCalculatePayload.OrderWorksheet.Reserialize<OrderWorksheet>(), promotions);
                var taxCalculation = await taxCalculationTask;
                await promoCalculationTask;

                return new HSOrderCalculateResponse
                {
                    TaxTotal = taxCalculation.TotalTax,
                    xp = new OrderCalculateResponseXp()
                    {
                        TaxCalculation = taxCalculation,
                    },
                };
            }
        }

        // TODO: To be replaced with direct calls to integration services once solution has been rearchitected.
        private async Task<ShipEstimateResponse> GetRatesAsync(HSOrderWorksheet worksheet, CheckoutIntegrationConfiguration config = null)
        {
            HSShipEstimateResponse shipResponse;
            switch (settings.EnvironmentSettings.ShippingProvider)
            {
                case ShippingProvider.Custom:
                    shipResponse = await CustomShippingCommand(worksheet, config);
                    break;
                default:
                    shipResponse = await EasyPostShippingCommand(worksheet, config);
                    break;
            }

            return shipResponse;
        }

        private async Task<HSShipEstimateResponse> CustomShippingCommand(HSOrderWorksheet worksheet, CheckoutIntegrationConfiguration config = null)
        {
            return await Task.FromResult(new HSShipEstimateResponse());
        }

        private async Task<HSShipEstimateResponse> EasyPostShippingCommand(HSOrderWorksheet worksheet, CheckoutIntegrationConfiguration config = null)
        {
            var easyPostShippingService = shippingService as IEasyPostShippingService;
            var groupedLineItems = worksheet.LineItems.GroupBy(li => new AddressPair { ShipFrom = li.ShipFromAddress, ShipTo = li.ShippingAddress }).ToList();

            // include all accounts at this stage so we can save on order worksheet and analyze
            var shipResponse = (await shippingService.GetRates(groupedLineItems)).Reserialize<HSShipEstimateResponse>();

            // Certain suppliers use certain shipping accounts. This filters available rates based on those accounts.
            for (var i = 0; i < groupedLineItems.Count; i++)
            {
                var supplierID = groupedLineItems[i].First().SupplierID;
                var profile = easyPostShippingService.Profiles.FirstOrDefault(supplierID);
                var methods = FilterMethodsBySupplierConfig(shipResponse.ShipEstimates[i].ShipMethods.Where(s => profile.CarrierAccountIDs.Contains(s.xp.CarrierAccountID)).ToList(), profile);
                shipResponse.ShipEstimates[i].ShipMethods = methods.Select(s =>
                {
                    // there is logic here to support not marking up shipping over list rate. But USPS is always list rate
                    // so adding an override to the suppliers that use USPS
                    var carrier = easyPostShippingService.Profiles.ShippingProfiles.First(p => p.CarrierAccountIDs.Contains(s.xp?.CarrierAccountID));
                    s.Cost = carrier.MarkupOverride ?
                        s.xp.OriginalCost * carrier.Markup :
                        Math.Min(s.xp.OriginalCost * carrier.Markup, s.xp.ListRate);

                    return s;
                }).ToList();
            }

            var buyerCurrency = worksheet.Order.xp.Currency ?? CurrencyCode.USD;

            await shipResponse.ShipEstimates
                .CheckForEmptyRates(settings.EasyPostSettings.NoRatesFallbackCost, settings.EasyPostSettings.NoRatesFallbackTransitDays)
                .ApplyShippingLogic(worksheet, oc, settings.EasyPostSettings.FreeShippingTransitDays).Result
                .ConvertCurrency(CurrencyCode.USD, buyerCurrency, exchangeRates);

            return shipResponse;
        }

        private async Task<List<HSShipEstimate>> ConvertShippingRatesCurrency(IList<HSShipEstimate> shipEstimates, CurrencyCode shipperCurrency, CurrencyCode buyerCurrency)
        {
            var rates = (await exchangeRates.Get(buyerCurrency)).Rates;
            var conversionRate = rates.Find(r => r.Currency == shipperCurrency).Rate;
            return shipEstimates.Select(estimate =>
            {
                estimate.ShipMethods = estimate.ShipMethods.Select(method =>
                {
                    method.xp.OriginalCurrency = shipperCurrency;
                    method.xp.OrderCurrency = buyerCurrency;
                    method.xp.ExchangeRate = conversionRate;
                    if (conversionRate != null)
                    {
                        method.Cost /= (decimal)conversionRate;
                    }

                    return method;
                }).ToList();
                return estimate;
            }).ToList();
        }

        private async Task<List<HSShipEstimate>> ApplyFreeShipping(HSOrderWorksheet orderWorksheet, IList<HSShipEstimate> shipEstimates)
        {
            var supplierIDs = orderWorksheet.LineItems.Select(li => li.SupplierID);
            var suppliers = await oc.Suppliers.ListAsync<HSSupplier>(filters: $"ID={string.Join("|", supplierIDs)}");
            var updatedEstimates = new List<HSShipEstimate>();

            foreach (var estimate in shipEstimates)
            {
                // get supplier and supplier subtotal
                var supplierID = orderWorksheet.LineItems.First(li => li.ID == estimate.ShipEstimateItems.FirstOrDefault()?.LineItemID).SupplierID;
                var supplier = suppliers.Items.FirstOrDefault(s => s.ID == supplierID);
                var supplierLineItems = orderWorksheet.LineItems.Where(li => li.SupplierID == supplier?.ID);
                var supplierSubTotal = supplierLineItems.Select(li => li.LineSubtotal).Sum();
                if (supplier?.xp?.FreeShippingThreshold != null && supplier.xp?.FreeShippingThreshold < supplierSubTotal)
                {
                    // free shipping for this supplier
                    foreach (var method in estimate.ShipMethods)
                    {
                        // free shipping on ground shipping or orders where we weren't able to calculate a shipping rate
                        if (method.Name.Contains("GROUND") || method.ID == ShippingConstants.NoRatesID)
                        {
                            method.xp.FreeShippingApplied = true;
                            method.xp.FreeShippingThreshold = supplier.xp.FreeShippingThreshold;
                            method.Cost = 0;
                        }
                    }
                }

                updatedEstimates.Add(estimate);
            }

            return updatedEstimates;
        }
    }
}
