using ordercloud.integrations.library;
using OrderCloud.SDK;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Taxjar;

namespace ordercloud.integrations.taxjar
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

	public class TaxJarCommand : ITaxJarCommand, ITaxCalculator
	{
		private readonly TaxjarApi _client;

		public TaxJarCommand(TaxJarConfig config)
		{
			var apiUrl = config.Environment == TaxJarEnvironment.Production ? TaxjarConstants.DefaultApiUrl : TaxjarConstants.SandboxApiUrl;
			_client = new TaxjarApi(config.APIKey, new { apiUrl });
		}

		/// <summary>
		/// Calculates tax for an order without creating any records. Use this to display tax amount to user prior to order submit.
		/// </summary>
		public async Task<OrderTaxCalculation> CalculateEstimateAsync(OrderWorksheet orderWorksheet, List<OrderPromotion> promotions)
		{

		}
		/// <summary>
		/// Creates a tax transaction record in the calculating system. Use this once on purchase, payment capture, or fulfillment.
		/// </summary>
		public async Task<OrderTaxCalculation> CommitTransactionAsync(OrderWorksheet orderWorksheet, List<OrderPromotion> promotions)
		{

		}
	}
}
