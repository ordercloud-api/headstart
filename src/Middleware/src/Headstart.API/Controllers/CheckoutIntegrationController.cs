using OrderCloud.SDK;
using OrderCloud.Catalyst;
using System.Threading.Tasks;
using Headstart.API.Commands;
using Microsoft.AspNetCore.Mvc;
using Headstart.Common.Services.ShippingIntegration.Models;

namespace Headstart.Common.Controllers
{
	public class CheckoutIntegrationController : CatalystController
	{
		private readonly ICheckoutIntegrationCommand _checkoutIntegrationCommand;
		private readonly IPostSubmitCommand _postSubmitCommand;

		/// <summary>
		/// The IOC based constructor method for the CheckoutIntegrationController with Dependency Injection
		/// </summary>
		/// <param name="checkoutIntegrationCommand"></param>
		/// <param name="postSubmitCommand"></param>
		public CheckoutIntegrationController(ICheckoutIntegrationCommand checkoutIntegrationCommand, IPostSubmitCommand postSubmitCommand)
		{
			_checkoutIntegrationCommand = checkoutIntegrationCommand;
			_postSubmitCommand = postSubmitCommand;
		}

		/// <summary>
		/// Submits the Shipping Rate Estimates, after posting the orderCalculatePayload object (POST method)
		/// </summary>
		/// <param name="orderCalculatePayload"></param>
		/// <returns>The shipping rate estimates response object</returns>
		[HttpPost, Route("shippingrates"), OrderCloudWebhookAuth]
		public async Task<ShipEstimateResponse> GetShippingRates([FromBody] HSOrderCalculatePayload orderCalculatePayload)
		{
			return await _checkoutIntegrationCommand.GetRatesAsync(orderCalculatePayload);
		}

		/// <summary>
		/// Submits the Calculated Order, after posting the orderCalculatePayload object (POST method)
		/// </summary>
		/// <param name="orderCalculatePayload"></param>
		/// <returns>The calculated order repsonse object</returns>
		[HttpPost, Route("ordercalculate"), OrderCloudWebhookAuth]
		public async Task<OrderCalculateResponse> CalculateOrder([FromBody] HSOrderCalculatePayload orderCalculatePayload)
		{
			var orderCalculationResponse = await _checkoutIntegrationCommand.CalculateOrder(orderCalculatePayload);
			return orderCalculationResponse;
		}

		/// <summary>
		/// Submits the Calculated Order, after posting the orderID string param (POST method)
		/// </summary>
		/// <param name="orderID"></param>
		/// <returns>The calculated order repsonse object</returns>
		[HttpPost, Route("taxcalculate/{orderID}"), OrderCloudUserAuth(ApiRole.IntegrationEventAdmin)]
		public async Task<OrderCalculateResponse> CalculateOrder(string orderID)
		{
			var orderCalculationResponse = await _checkoutIntegrationCommand.CalculateOrder(orderID, UserContext);
			return orderCalculationResponse;
		}

		/// <summary>
		/// Submits the Order Submission, after posting the payload object (POST method)
		/// </summary>
		/// <param name="payload"></param>
		/// <returns>The submitted order repsonse object</returns>
		[HttpPost, Route("ordersubmit")]
		//[OrderCloudWebhookAuth]  TODO: Add this back in once platform fixes header issue
		public async Task<OrderSubmitResponse> HandleOrderSubmit([FromBody] HSOrderCalculatePayload payload)
		{
			var response = await _postSubmitCommand.HandleBuyerOrderSubmit(payload.OrderWorksheet);
			return response;
		}

		/// <summary>
		/// Retries the Order Submission, after posting the orderID string param (POST method)
		/// </summary>
		/// <param name="orderID"></param>
		/// <returns>The submitted order repsonse object</returns>
		[HttpPost, Route("ordersubmit/retry/zoho/{orderID}"), OrderCloudUserAuth(ApiRole.IntegrationEventAdmin)]
		public async Task<OrderSubmitResponse> RetryOrderSubmit(string orderID)
		{
			var retry = await _postSubmitCommand.HandleZohoRetry(orderID);
			return retry;
		}

		/// <summary>
		/// Submits the Approved Order, after posting the payload object (POST method)
		/// </summary>
		/// <param name="payload"></param>
		/// <returns>The submitted order repsonse object</returns>
		[HttpPost, Route("orderapproved"), OrderCloudWebhookAuth]
		public async Task<OrderSubmitResponse> HandleOrderApproved([FromBody] HSOrderCalculatePayload payload)
		{
			var response = await _postSubmitCommand.HandleBuyerOrderSubmit(payload.OrderWorksheet);
			return response;
		}
	}
}