using OrderCloud.Catalyst;
using OrderCloud.Integrations.TaxJar.Mappers;
using OrderCloud.SDK;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Taxjar;
using ITaxCalculator = OrderCloud.Integrations.Library.Interfaces.ITaxCalculator;
using ITaxCodesProvider = OrderCloud.Integrations.Library.Interfaces.ITaxCodesProvider;
using OrderTaxCalculation = OrderCloud.Integrations.Library.Interfaces.OrderTaxCalculation;
using TaxCategorizationResponse = OrderCloud.Integrations.Library.Interfaces.TaxCategorizationResponse;
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
        private readonly TaxjarApi client;

        public TaxJarCommand(TaxJarConfig config)
        {
            if (string.IsNullOrWhiteSpace(config.ApiKey))
            {
                return;
            }

            var apiUrl = config.Environment == TaxJarEnvironment.Production ? TaxjarConstants.DefaultApiUrl : TaxjarConstants.SandboxApiUrl;
            client = new TaxjarApi(config.ApiKey, new { apiUrl });
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

            await Throttler.RunAsync(orders, 100, 8, async order => await MakeRequest(() => client.CreateOrderAsync(order)));

            var orderTaxCalculation = orders.ToOrderTaxCalculation();
            return orderTaxCalculation;
        }

        public async Task<TaxCategorizationResponse> ListTaxCodesAsync(string searchTerm)
        {
            var categories = await MakeRequest(() => client.CategoriesAsync());
            var taxCategorizations = categories.ToTaxCategorization(searchTerm);
            return taxCategorizations;
        }

        private async Task<IEnumerable<(TaxJarOrder request, TaxResponseAttributes response)>> CalculateTax(OrderWorksheet orderWorksheet)
        {
            var orders = orderWorksheet.ToTaxJarOrders();
            var responses = await Throttler.RunAsync(orders, 100, 8, async order =>
            {
                var tax = await MakeRequest(() => client.TaxForOrderAsync(order));
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
