using OrderCloud.SDK;
using Headstart.Common;
using OrderCloud.Catalyst;
using System.Threading.Tasks;
using Headstart.API.Commands;
using Headstart.Common.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Headstart.Models.Headstart;

namespace Headstart.API.Controllers
{
	[Route("payments")]
	public class PaymentController : CatalystController
	{
		private readonly IPaymentCommand _command;
		private readonly AppSettings _settings;

		/// <summary>
		/// The IOC based constructor method for the PaymentController class object with Dependency Injection
		/// </summary>
		/// <param name="command"></param>
		/// <param name="settings"></param>
		public PaymentController(IPaymentCommand command, AppSettings settings)
		{
			_command = command;
			_settings = settings;
		}

		/// <summary>
		/// Save payments. Creates or updates payments as needed for this order  (PUT method)
		/// </summary>
		/// <param name="orderId"></param>
		/// <param name="request"></param>
		/// <returns>The list of HSPayment response from the SavePayments action</returns>
		[HttpPut, Route("{orderId}/update"), OrderCloudUserAuth(ApiRole.Shopper)]
		public async Task<IList<HSPayment>> SavePayments(string orderId, [FromBody] PaymentUpdateRequest request)
		{
			return await _command.SavePayments(orderId, request.Payments, UserContext.AccessToken);
		}
	}
}