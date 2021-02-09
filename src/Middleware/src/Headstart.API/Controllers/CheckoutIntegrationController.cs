using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Headstart.Common.Services.ShippingIntegration.Models;
using OrderCloud.SDK;
using ordercloud.integrations.library;
using Headstart.API.Controllers;
using Headstart.API.Commands;

namespace Headstart.Common.Controllers
{
	public class CheckoutIntegrationController: BaseController
	{
		private readonly ICheckoutIntegrationCommand _checkoutIntegrationCommand;
		private readonly IPostSubmitCommand _postSubmitCommand;
		public CheckoutIntegrationController(AppSettings settings, ICheckoutIntegrationCommand checkoutIntegrationCommand, IPostSubmitCommand postSubmitCommand) : base(settings) 
		{
			_checkoutIntegrationCommand = checkoutIntegrationCommand;
			_postSubmitCommand = postSubmitCommand;
		}

		[Route("shippingrates")]
		[HttpPost]
		[OrderCloudWebhookAuth]
		public async Task<ShipEstimateResponse> GetShippingRates([FromBody] HSOrderCalculatePayload orderCalculatePayload)
		{
			return await _checkoutIntegrationCommand.GetRatesAsync(orderCalculatePayload);
		}

        [Route("ordercalculate")]
		[HttpPost]
		[OrderCloudWebhookAuth]
		public async Task<OrderCalculateResponse> CalculateOrder([FromBody] HSOrderCalculatePayload orderCalculatePayload)
		{
			var orderCalculationResponse = await _checkoutIntegrationCommand.CalculateOrder(orderCalculatePayload);
			return orderCalculationResponse;
		}

        [Route("taxcalculate/{orderID}")]
        [HttpPost, OrderCloudIntegrationsAuth(ApiRole.IntegrationEventAdmin)]
        public async Task<OrderCalculateResponse> CalculateOrder(string orderID)
        {
            var orderCalculationResponse = await _checkoutIntegrationCommand.CalculateOrder(orderID, this.VerifiedUserContext);
            return orderCalculationResponse;
        }

		[HttpPost, Route("ordersubmit")]
		[OrderCloudWebhookAuth]
		public async Task<OrderSubmitResponse> HandleOrderSubmit([FromBody] HSOrderCalculatePayload payload)
		{
			var response = await _postSubmitCommand.HandleBuyerOrderSubmit(payload.OrderWorksheet);
			return response;
		}

        [HttpPost, Route("ordersubmit/retry/zoho/{orderID}"), OrderCloudIntegrationsAuth(ApiRole.IntegrationEventAdmin)]
        public async Task<OrderSubmitResponse> RetryOrderSubmit(string orderID)
        {
            var retry = await _postSubmitCommand.HandleZohoRetry(orderID);
            return retry;
        }

        [HttpPost, Route("orderapproved")]
		[OrderCloudWebhookAuth]
		public async Task<OrderSubmitResponse> HandleOrderApproved([FromBody] HSOrderCalculatePayload payload)
		{
			var response = await _postSubmitCommand.HandleBuyerOrderSubmit(payload.OrderWorksheet);
			return response;
		}
	}
}
