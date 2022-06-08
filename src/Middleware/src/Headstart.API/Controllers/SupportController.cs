using System.Threading.Tasks;
using Headstart.API.Commands;
using Headstart.Common.Models;
using Microsoft.AspNetCore.Mvc;
using OrderCloud.Catalyst;
using OrderCloud.Integrations.Emails;
using OrderCloud.SDK;

namespace Headstart.Common.Controllers
{
    [Route("support")]
    public class SupportController : CatalystController
    {
        private readonly ICheckoutIntegrationCommand checkoutIntegrationCommand;
        private readonly IPostSubmitCommand postSubmitCommand;
        private readonly IOrderCloudClient oc;
        private readonly IEmailServiceProvider emailServiceProvider;

        public SupportController(ICheckoutIntegrationCommand checkoutIntegrationCommand, IPostSubmitCommand postSubmitCommand, IOrderCloudClient oc, IEmailServiceProvider emailServiceProvider)
        {
            this.checkoutIntegrationCommand = checkoutIntegrationCommand;
            this.postSubmitCommand = postSubmitCommand;
            this.oc = oc;
            this.emailServiceProvider = emailServiceProvider;
        }

        [HttpGet, Route("shipping")]
        public async Task<ShipEstimateResponse> GetShippingRates([FromBody] ShipmentTestRequest model)
        {
            var payload = new HSOrderCalculatePayload()
            {
                ConfigData = null,
                OrderWorksheet = new HSOrderWorksheet()
                {
                    Order = model.Order,
                    LineItems = model.LineItems,
                },
            };
            return await checkoutIntegrationCommand.GetRatesAsync(payload);
        }

        [Route("tax/{orderID}")]
        [HttpGet, OrderCloudUserAuth(ApiRole.IntegrationEventAdmin)]
        public async Task<OrderCalculateResponse> CalculateOrder(string orderID)
        {
            var orderCalculationResponse = await checkoutIntegrationCommand.CalculateOrder(orderID, UserContext);
            return orderCalculationResponse;
        }

        [HttpGet, Route("shipping/validate/{orderID}"), OrderCloudUserAuth(ApiRole.IntegrationEventAdmin)]
        public async Task<OrderSubmitResponse> RetryShippingValidate(string orderID)
        {
            var retry = await postSubmitCommand.HandleShippingValidate(orderID, UserContext);
            return retry;
        }

        // good debug method for testing rates with orders
        [HttpGet, Route("shippingrates/{orderID}")]
        public async Task<ShipEstimateResponse> GetShippingRates(string orderID)
        {
            return await checkoutIntegrationCommand.GetRatesAsync(orderID);
        }

        [HttpPost, Route("postordersubmit/{orderID}"), OrderCloudUserAuth]
        public async Task<OrderSubmitResponse> ManuallyRunPostOrderSubmit(string orderID)
        {
            var worksheet = await oc.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.Incoming, orderID);
            return await postSubmitCommand.HandleBuyerOrderSubmit(worksheet);
        }

        [HttpPost, Route("submitcase")]
        public async Task SendSupportRequest([FromForm]SupportCase supportCase)
        {
            await emailServiceProvider.EmailGeneralSupportQueue(supportCase);
        }
    }
}
