using OrderCloud.SDK;
using OrderCloud.Catalyst;
using System.Threading.Tasks;
using Headstart.API.Commands;
using Microsoft.AspNetCore.Mvc;
using Headstart.Common.Models.Headstart;

namespace Headstart.API.Controllers
{
	public class CheckoutIntegrationController : CatalystController
	{
		private readonly ICheckoutIntegrationCommand _checkoutIntegrationCommand;
		private readonly IPostSubmitCommand _postSubmitCommand;

		/// <summary>
		/// The IOC based constructor method for the CheckoutIntegrationController class object with Dependency Injection
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
		public async Task<ShipEstimateResponse> GetShippingRates([FromBody] HsOrderCalculatePayload orderCalculatePayload)
		{
			return await _checkoutIntegrationCommand.GetRatesAsync(orderCalculatePayload);
		}

		/// <summary>
		/// Submits the Calculated Order, after posting the orderCalculatePayload object (POST method)
		/// </summary>
		/// <param name="orderCalculatePayload"></param>
		/// <returns>The calculated order response object</returns>
		[HttpPost, Route("ordercalculate"), OrderCloudWebhookAuth]
		public async Task<OrderCalculateResponse> CalculateOrder([FromBody] HsOrderCalculatePayload orderCalculatePayload)
		{
			var orderCalculationResponse = await _checkoutIntegrationCommand.CalculateOrder(orderCalculatePayload);
			return orderCalculationResponse;
		}

		/// <summary>
		/// Submits the Calculated Order, after posting the orderID string param (POST method)
		/// </summary>
		/// <param name="orderId"></param>
		/// <returns>The calculated order response object</returns>
		[HttpPost, Route("taxcalculate/{orderId}"), OrderCloudUserAuth(ApiRole.IntegrationEventAdmin)]
		public async Task<OrderCalculateResponse> CalculateOrder(string orderId)
		{
			var orderCalculationResponse = await _checkoutIntegrationCommand.CalculateOrder(orderId, UserContext); 
			return orderCalculationResponse;
		}

		/// <summary>
		/// Submits the Order Submission, after posting the payload object (POST method)
		/// </summary>
		/// <param name="payload"></param>
		/// <returns>The submitted order response object</returns>
		[HttpPost, Route("ordersubmit")]
		//[OrderCloudWebhookAuth]  TODO: Add this back in once platform fixes header issue
		public async Task<OrderSubmitResponse> HandleOrderSubmit([FromBody] HsOrderCalculatePayload payload)
		{
			var response = await _postSubmitCommand.HandleBuyerOrderSubmit(payload.OrderWorksheet);
			return response;
		}

		/// <summary>
		/// Submits the Order Submission, after posting the payload object (POST method)
		/// </summary>
		/// <param name="payload"></param>
		/// <returns>The submitted order response object</returns>
		[HttpPost, Route("ordersubmitforapproval")]
		//[OrderCloudWebhookAuth]  TODO: Add this back in once platform fixes header issue
		public async Task<OrderSubmitResponse> HandleOrderSubmitForApproval([FromBody] HsOrderCalculatePayload payload)
		{
			var response = await _postSubmitCommand.HandleBuyerOrderSubmit(payload.OrderWorksheet);
			return response;
		}

		/// <summary>
		/// Submits the Approved Order, after posting the payload object (POST method)
		/// </summary>
		/// <param name="payload"></param>
		/// <returns>The submitted order response object</returns>
		[HttpPost, Route("orderapproved")]
		//[OrderCloudWebhookAuth]  TODO: Add this back in once platform fixes header issue
		public async Task<OrderSubmitResponse> HandleOrderApproved([FromBody] HsOrderCalculatePayload payload)
		{
			var response = await _postSubmitCommand.HandleBuyerOrderSubmit(payload.OrderWorksheet);
			return response;
		}

		/// <summary>
		/// Retries the Order Submission, after posting the orderID string param (POST method)
		/// </summary>
		/// <param name="orderId"></param>
		/// <returns>The submitted order response object</returns>
		[HttpPost, Route("ordersubmit/retry/zoho/{orderId}"), OrderCloudUserAuth(ApiRole.IntegrationEventAdmin)]
		public async Task<OrderSubmitResponse> RetryOrderSubmit(string orderId)
		{
			var retry = await _postSubmitCommand.HandleZohoRetry(orderId);
			return retry;
		}
	}
}