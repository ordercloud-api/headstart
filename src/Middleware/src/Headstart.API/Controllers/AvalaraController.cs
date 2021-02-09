using Microsoft.AspNetCore.Mvc;
using OrderCloud.SDK;
using System.Threading.Tasks;
using Headstart.Models.Attributes;
using ordercloud.integrations.avalara;
using ordercloud.integrations.library;
using Headstart.API.Controllers;
using Headstart.API.Commands;

namespace Headstart.Common.Controllers.Avalara
{
	[DocComments("\"Integration\" represents Avalara Tax Functionality")]
	[HSSection.Integration(ListOrder = 1)]
	[Route("avalara")]
	public class AvalaraController : BaseController
	{
		private readonly IAvalaraCommand _avalara;
		private readonly IResaleCertCommand _resaleCertCommand;
		public AvalaraController(AppSettings settings, IAvalaraCommand avalara, IResaleCertCommand resaleCertCommand, IOrderCloudClient oc) : base(settings)
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

		[DocName("List Tax Codes")]
		[HttpGet, Route("code"), OrderCloudIntegrationsAuth(ApiRole.ProductAdmin)]
		public async Task<ListPage<TaxCode>> ListTaxCodes(ListArgs<TaxCode> hsListArgs)
		{
			return await _avalara.ListTaxCodesAsync(hsListArgs);
		}

		[DocName("Get tax exeption certificate details")]
		[HttpGet, Route("certificate/{locationID}"), OrderCloudIntegrationsAuth(ApiRole.Shopper)]
		public async Task<TaxCertificate> GetCertificate(string locationID)
		{
			return await _resaleCertCommand.GetAsync(locationID, VerifiedUserContext);
		}

		[DocName("Create tax exeption certificate")]
		[HttpPost, Route("certificate/{locationID}"), OrderCloudIntegrationsAuth(ApiRole.Shopper)]
		public async Task<TaxCertificate> CreateCertificate(string locationID, [FromBody] TaxCertificate cert)
		{
			return await _resaleCertCommand.CreateAsync(locationID, cert, VerifiedUserContext);
		}

		[DocName("Update tax exeption certificate")]
		[HttpPut, Route("certificate/{locationID}"), OrderCloudIntegrationsAuth(ApiRole.Shopper)]
		public async Task<TaxCertificate> UpdateCertificate(string locationID, [FromBody] TaxCertificate cert)
		{
			return await _resaleCertCommand.UpdateAsync(locationID, cert, VerifiedUserContext);
		}
	}
}
