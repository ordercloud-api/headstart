using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ordercloud.integrations.library;
using OrderCloud.SDK;


namespace ordercloud.integrations.vertex
{
	public interface IVertexCommand
	{
		/// <summary>
		/// Calculates tax for an order without creating any records. Use this to display tax amount to user prior to order submit.
		/// </summary>
		Task<OrderTaxCalculation> CalculateEstimateAsync(OrderWorksheet orderWorksheet);
		/// <summary>
		/// Creates a tax transaction record in the calculating system. Use this once on purchase, payment capture, or fulfillment.
		/// </summary>
		Task<OrderTaxCalculation> CalculateTransactionAsync(OrderWorksheet orderWorksheet);
	}
}
