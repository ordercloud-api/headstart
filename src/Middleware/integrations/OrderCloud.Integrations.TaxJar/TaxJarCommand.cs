using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrderCloud.Catalyst;
using OrderCloud.Integrations.TaxJar.Mappers;
using OrderCloud.SDK;
using Taxjar;
using ITaxCalculator = Headstart.Common.Services.ITaxCalculator;
using ITaxCodesProvider = Headstart.Common.Services.ITaxCodesProvider;
using OrderTaxCalculation = Headstart.Common.Services.OrderTaxCalculation;
using TaxCategorizationResponse = Headstart.Common.Services.TaxCategorizationResponse;
using TaxJarOrder = Taxjar.Order;

namespace OrderCloud.Integrations.TaxJar
{
    public interface ITaxJarCommand
    {
        /// <summary>
        /// Calculates tax for an order without creating any records. Use this to display tax amount to user prior to order submit.
        /// </summary>
        Task<OrderTaxCalculation> CalculateEstimateAsync(OrderWorksheet orderWorksheet, List<OrderPromotion> promotions);

        /// <summary>
        /// Creates a tax transaction record in the calculating system. Use this once on purchase, payment capture, or fulfillment.
        /// </summary>
        Task<OrderTaxCalculation> CommitTransactionAsync(OrderWorksheet orderWorksheet, List<OrderPromotion> promotions);
    }

    public class TaxJarCommand : ITaxJarCommand, ITaxCalculator, ITaxCodesProvider
    {
        private readonly TaxjarApi taxJarClient;

        public TaxJarCommand(TaxjarApi taxJarClient)
        {
            this.taxJarClient = taxJarClient;
        }

        /// <summary>
        /// Calculates tax for an order without creating any records. Use this to display tax amount to user prior to order submit.
        /// </summary>
        public async Task<OrderTaxCalculation> CalculateEstimateAsync(OrderWorksheet orderWorksheet, List<OrderPromotion> promotions)
        {
            var orders = await CalculateTax(orderWorksheet);
            var orderTaxCalculation = orders.ToOrderTaxCalculation();
            return orderTaxCalculation;
        }

        /// <summary>
        /// Creates a tax transaction record in the calculating system. Use this once on purchase, payment capture, or fulfillment.
        /// </summary>
        public async Task<OrderTaxCalculation> CommitTransactionAsync(OrderWorksheet orderWorksheet, List<OrderPromotion> promotions)
        {
            var orders = await CalculateTax(orderWorksheet);
            foreach (var response in orders)
            {
                response.request.TransactionDate = DateTime.Now.ToString("yyyy/MM/dd");
                response.request.SalesTax = response.response.TaxableAmount;
            }

            await Throttler.RunAsync(orders, 100, 8, async order => await MakeRequest(() => taxJarClient.CreateOrderAsync(order)));

            var orderTaxCalculation = orders.ToOrderTaxCalculation();
            return orderTaxCalculation;
        }

        public async Task<TaxCategorizationResponse> ListTaxCodesAsync(string searchTerm)
        {
            var categories = await MakeRequest(() => taxJarClient.CategoriesAsync());
            var taxCategorizations = categories.ToTaxCategorization(searchTerm);
            return taxCategorizations;
        }

        private async Task<IEnumerable<(TaxJarOrder request, TaxResponseAttributes response)>> CalculateTax(OrderWorksheet orderWorksheet)
        {
            var orders = orderWorksheet.ToTaxJarOrders();
            var responses = await Throttler.RunAsync(orders, 100, 8, async order =>
            {
                var tax = await MakeRequest(() => taxJarClient.TaxForOrderAsync(order));
                return (order, tax);
            });
            return responses;
        }

        private async Task<T> MakeRequest<T>(Func<Task<T>> request)
        {
            try
            {
                return await request();
            }
            catch (TaxjarException ex)
            {
                throw new CatalystBaseException(
                    "TaxJarTaxCalculationError",
                    "The taxjar api returned an error",
                    ex.TaxjarError,
                    int.Parse(ex.TaxjarError.StatusCode));
            }
        }
    }
}
