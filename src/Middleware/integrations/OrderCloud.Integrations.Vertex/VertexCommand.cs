using System.Collections.Generic;
using System.Threading.Tasks;
using Headstart.Common.Services;
using OrderCloud.Integrations.Vertex.Mappers;
using OrderCloud.Integrations.Vertex.Models;
using OrderCloud.SDK;

namespace OrderCloud.Integrations.Vertex
{
    public interface IVertexCommand
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

    public class VertexCommand : IVertexCommand, ITaxCalculator
    {
        private readonly VertexClient client;
        private readonly VertexConfig config;

        public VertexCommand(VertexConfig config)
        {
            this.config = config;
            client = new VertexClient(config);
        }

        /// <summary>
        /// Calculates tax for an order without creating any records. Use this to display tax amount to user prior to order submit.
        /// </summary>
        public async Task<OrderTaxCalculation> CalculateEstimateAsync(OrderWorksheet orderWorksheet, List<OrderPromotion> promotions) =>
            await CalculateTaxAsync(orderWorksheet, promotions, VertexSaleMessageType.QUOTATION);

        /// <summary>
        /// Creates a tax transaction record in the calculating system. Use this once on purchase, payment capture, or fulfillment.
        /// </summary>
        public async Task<OrderTaxCalculation> CommitTransactionAsync(OrderWorksheet orderWorksheet, List<OrderPromotion> promotions) =>
            await CalculateTaxAsync(orderWorksheet, promotions, VertexSaleMessageType.INVOICE);

        private async Task<OrderTaxCalculation> CalculateTaxAsync(OrderWorksheet orderWorksheet, List<OrderPromotion> promotions, VertexSaleMessageType type)
        {
            var request = orderWorksheet.ToVertexCalculateTaxRequest(promotions, config.CompanyName, type);
            var response = await client.CalculateTax(request);
            var orderTaxCalculation = response.ToOrderTaxCalculation();
            return orderTaxCalculation;
        }
    }
 }
