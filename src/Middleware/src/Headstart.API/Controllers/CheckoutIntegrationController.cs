using System.Threading.Tasks;
using Headstart.API.Commands;
using Headstart.Common.Services.ShippingIntegration.Models;
using Microsoft.AspNetCore.Mvc;
using OrderCloud.Catalyst;
using OrderCloud.SDK;


namespace Headstart.Common.Controllers
{
    public class CheckoutIntegrationController : CatalystController
    {
        private readonly ICheckoutIntegrationCommand checkoutIntegrationCommand;
        private readonly IPostSubmitCommand postSubmitCommand;

        public CheckoutIntegrationController(ICheckoutIntegrationCommand checkoutIntegrationCommand, IPostSubmitCommand postSubmitCommand)
        {
            this.checkoutIntegrationCommand = checkoutIntegrationCommand;
            this.postSubmitCommand = postSubmitCommand;
        }

        [Route("shippingrates")]
        [HttpPost]
        [OrderCloudWebhookAuth]
        public async Task<ShipEstimateResponse> GetShippingRates([FromBody] HSOrderCalculatePayload orderCalculatePayload)
        {
            return await checkoutIntegrationCommand.GetRatesAsync(orderCalculatePayload);
        }

        [Route("ordercalculate")]
        [HttpPost]
        [OrderCloudWebhookAuth]
        public async Task<OrderCalculateResponse> CalculateOrder([FromBody] HSOrderCalculatePayload orderCalculatePayload)
        {
            var orderCalculationResponse = await checkoutIntegrationCommand.CalculateOrder(orderCalculatePayload);
            return orderCalculationResponse;
        }

        [Route("taxcalculate/{orderID}")]
        [HttpPost, OrderCloudUserAuth(ApiRole.IntegrationEventAdmin)]
        public async Task<OrderCalculateResponse> CalculateOrder(string orderID)
        {
            var orderCalculationResponse = await checkoutIntegrationCommand.CalculateOrder(orderID, UserContext);
            return orderCalculationResponse;
        }

        // [OrderCloudWebhookAuth]  TODO: Add this back in once platform fixes header issue
        [HttpPost, Route("ordersubmit")]
        public async Task<OrderSubmitResponse> HandleOrderSubmit([FromBody] HSOrderCalculatePayload payload)
        {
            var response = await postSubmitCommand.HandleBuyerOrderSubmit(payload.OrderWorksheet);
            return response;
        }

        // [OrderCloudWebhookAuth]  TODO: Add this back in once platform fixes header issue
        [HttpPost, Route("ordersubmitforapproval")]
        public async Task<OrderSubmitResponse> HandleOrderSubmitForApproval([FromBody] HSOrderCalculatePayload payload)
        {
            var response = await postSubmitCommand.HandleBuyerOrderSubmit(payload.OrderWorksheet);
            return response;
        }

        // [OrderCloudWebhookAuth]  TODO: Add this back in once platform fixes header issue
        [HttpPost, Route("orderapproved")]
        public async Task<OrderSubmitResponse> HandleOrderApproved([FromBody] HSOrderCalculatePayload payload)
        {
            var response = await postSubmitCommand.HandleBuyerOrderSubmit(payload.OrderWorksheet);
            return response;
        }

        [HttpPost, Route("ordersubmit/retry/zoho/{orderID}"), OrderCloudUserAuth(ApiRole.IntegrationEventAdmin)]
        public async Task<OrderSubmitResponse> RetryOrderSubmit(string orderID)
        {
            var retry = await postSubmitCommand.HandleZohoRetry(orderID);
            return retry;
        }
    }
}
