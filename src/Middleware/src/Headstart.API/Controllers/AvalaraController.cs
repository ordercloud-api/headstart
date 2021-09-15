using Microsoft.AspNetCore.Mvc;
using OrderCloud.SDK;
using System.Threading.Tasks;
using Headstart.Models.Attributes;
using ordercloud.integrations.avalara;
using ordercloud.integrations.library;
using Headstart.API.Commands;
using OrderCloud.Catalyst;

namespace Headstart.Common.Controllers.Avalara
{
	/// <summary>
	///  Avalara Tax Functionality
	/// </summary>
	[Route("avalara")]
	public class AvalaraController : CatalystController
	{
		private readonly IAvalaraCommand _avalara;
		private readonly IResaleCertCommand _resaleCertCommand;
		public AvalaraController(AppSettings settings, IAvalaraCommand avalara, IResaleCertCommand resaleCertCommand, IOrderCloudClient oc)
		{
			_avalara = avalara;
			_resaleCertCommand = resaleCertCommand;
		}

		// Commented out until swagger can reference the TransactionModel

		//[DocName("Get Tax Estimate")]
		//[HttpPost, Route("estimate"), OrderCloudIntegrationsAuth(ApiRole.Shopper)]
		//public async Task<decimal> GetTaxEstimate([FromBody] OrderWorksheet orderWorksheet)
		//{
		//	return await _avalara.GetEstimateAsync(orderWorksheet);
		//}

		//[DocName("Create Tax Transaction")]
		//[HttpPost, Route("transaction"), OrderCloudIntegrationsAuth(ApiRole.OrderAdmin)]
		//public async Task<TransactionModel> CreateTransaction([FromBody] OrderWorksheet orderWorksheet)
		//{
		//	return await _avalara.CreateTransactionAsync(orderWorksheet);
		//}

		//[DocName("Commit Tax Transaction")]
		//[HttpPost, Route("transaction/{transactionCode}/commit"), OrderCloudIntegrationsAuth(ApiRole.OrderAdmin)]
		//public async Task<TransactionModel> CommitTransaction(string transactionCode)
		//{
		//	return await _avalara.CommitTransactionAsync(transactionCode);
		//}

		/// <summary>
		/// List Tax Codes
		/// </summary>
		[HttpGet, Route("code"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
		public async Task<ListPage<TaxCode>> ListTaxCodes(ListArgs<TaxCode> hsListArgs)
		{
			return await _avalara.ListTaxCodesAsync(hsListArgs);
		}

		/// <summary>
		/// Get tax exeption certificate details
		/// </summary>
		[HttpGet, Route("certificate/{locationID}"), OrderCloudUserAuth(ApiRole.Shopper)]
		public async Task<TaxCertificate> GetCertificate(string locationID)
		{
			return await _resaleCertCommand.GetAsync(locationID, UserContext);
		}

		/// <summary>
		/// Create tax exeption certificate
		/// </summary>
		[HttpPost, Route("certificate/{locationID}"), OrderCloudUserAuth(ApiRole.Shopper)]
		public async Task<TaxCertificate> CreateCertificate(string locationID, [FromBody] TaxCertificate cert)
		{
			return await _resaleCertCommand.CreateAsync(locationID, cert, UserContext);
		}

		/// <summary>
		/// Update tax exeption certificate
		/// </summary>
		[HttpPut, Route("certificate/{locationID}"), OrderCloudUserAuth(ApiRole.Shopper)]
		public async Task<TaxCertificate> UpdateCertificate(string locationID, [FromBody] TaxCertificate cert)
		{
			return await _resaleCertCommand.UpdateAsync(locationID, cert, UserContext);
		}
	}
}
