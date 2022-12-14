using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Headstart.Common.Models;
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
        Task<OrderTaxCalculation> CalculateEstimateAsync(HSOrderWorksheet orderWorksheet);

        /// <summary>
        /// Creates a tax transaction record in the calculating system. Use this once on purchase, payment capture, or fulfillment.
        /// </summary>
        Task<OrderTaxCalculation> CommitTransactionAsync(HSOrderWorksheet orderWorksheet);
    }

    public class VertexCommand : IVertexCommand, ITaxCalculator
    {
        private readonly VertexClient vertexClient;
        private readonly VertexSettings config;

        public VertexCommand(VertexSettings config, VertexClient vertexClient)
        {
            this.config = config;
            this.vertexClient = vertexClient;
        }

        /// <summary>
        /// Calculates tax for an order without creating any records. Use this to display tax amount to user prior to order submit.
        /// </summary>
        public async Task<OrderTaxCalculation> CalculateEstimateAsync(HSOrderWorksheet orderWorksheet) =>
            await CalculateTaxAsync(orderWorksheet, VertexSaleMessageType.QUOTATION);

        /// <summary>
        /// Creates a tax transaction record in the calculating system. Use this once on purchase, payment capture, or fulfillment.
        /// </summary>
        public async Task<OrderTaxCalculation> CommitTransactionAsync(HSOrderWorksheet orderWorksheet) =>
            await CalculateTaxAsync(orderWorksheet, VertexSaleMessageType.INVOICE);

        private async Task<OrderTaxCalculation> CalculateTaxAsync(HSOrderWorksheet orderWorksheet, VertexSaleMessageType type)
        {
            var promotions = orderWorksheet.OrderPromotions.ToList();
            var request = orderWorksheet.ToVertexCalculateTaxRequest(promotions, config.CompanyName, type);
            var response = await vertexClient.CalculateTax(request);
            var orderTaxCalculation = response.ToOrderTaxCalculation();
            return orderTaxCalculation;
        }
    }
 }
