using Microsoft.AspNetCore.Mvc;
using OrderCloud.SDK;
using System.Threading.Tasks;
using Headstart.Models.Attributes;
using ordercloud.integrations.avalara;
using ordercloud.integrations.library;
using Headstart.API.Commands;
using OrderCloud.Catalyst;
using ordercloud.integrations.library.intefaces;
using System.Collections.Generic;

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

		/// <summary>
		/// List Tax Codes
		/// </summary>
		[HttpGet, Route("code"), OrderCloudUserAuth(ApiRole.ProductAdmin)]
		public async Task<List<TaxCategorization>> ListTaxCodes([FromQuery] string search)
		{
			return await _avalara.ListTaxCodesAsync(search);
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
