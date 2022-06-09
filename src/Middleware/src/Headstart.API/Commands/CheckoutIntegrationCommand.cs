using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Headstart.Common.Commands;
using Headstart.Common.Constants;
using Headstart.Common.Extensions;
using Headstart.Common.Models;
using Headstart.Common.Services;
using OrderCloud.Catalyst;
using OrderCloud.SDK;
using ITaxCalculator = Headstart.Common.Services.ITaxCalculator;

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
        private readonly IShippingCommand shippingCommand;
        private readonly ITaxCalculator taxCalculator;
        private readonly ICurrencyConversionService currencyConversionService;
        private readonly IOrderCloudClient orderCloudClient;
        private readonly IDiscountDistributionService discountDistribution;

        public CheckoutIntegrationCommand(
            IDiscountDistributionService discountDistribution,
            ITaxCalculator taxCalculator,
            ICurrencyConversionService currencyConversionService,
            IOrderCloudClient orderCloudClient,
            IShippingCommand shippingCommand)
        {
            this.taxCalculator = taxCalculator;
            this.currencyConversionService = currencyConversionService;
            this.orderCloudClient = orderCloudClient;
            this.shippingCommand = shippingCommand;
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
            var shipEstimateResponse = await shippingCommand.GetRatesAsync(orderCalculatePayload.OrderWorksheet, orderCalculatePayload.ConfigData);
            var buyerCurrency = orderCalculatePayload.OrderWorksheet.Order.xp.Currency ?? CurrencyCode.USD;
            await shipEstimateResponse.ShipEstimates.ConvertCurrency(CurrencyCode.USD, buyerCurrency, currencyConversionService);

            return shipEstimateResponse;
        }

        public async Task<ShipEstimateResponse> GetRatesAsync(string orderID)
        {
            var orderWorksheet = await orderCloudClient.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.Incoming, orderID);
            var shipEstimateResponse = await shippingCommand.GetRatesAsync(orderWorksheet);
            var buyerCurrency = orderWorksheet.Order.xp.Currency ?? CurrencyCode.USD;
            await shipEstimateResponse.ShipEstimates.ConvertCurrency(CurrencyCode.USD, buyerCurrency, currencyConversionService);

            return shipEstimateResponse;
        }

        public async Task<HSOrderCalculateResponse> CalculateOrder(string orderID, DecodedToken decodedToken)
        {
            var worksheet = await orderCloudClient.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.Incoming, orderID, decodedToken.AccessToken);
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
                var promotions = await orderCloudClient.Orders.ListAllPromotionsAsync(OrderDirection.All, orderCalculatePayload.OrderWorksheet.Order.ID);
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

        private async Task<List<HSShipEstimate>> ConvertShippingRatesCurrency(IList<HSShipEstimate> shipEstimates, CurrencyCode shipperCurrency, CurrencyCode buyerCurrency)
        {
            var rates = (await currencyConversionService.Get(buyerCurrency)).Rates;
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
            var suppliers = await orderCloudClient.Suppliers.ListAsync<HSSupplier>(filters: $"ID={string.Join("|", supplierIDs)}");
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
