using System.Collections.Generic;
using System.Threading.Tasks;
using Headstart.API.Commands;
using Headstart.API.Commands.Zoho;
using Headstart.Common.Models.Misc;
using Headstart.Common.Services;
using Headstart.Common.Services.ShippingIntegration.Models;
using Headstart.Models;
using Headstart.Models.Headstart;
using Microsoft.AspNetCore.Mvc;
using OrderCloud.Catalyst;
using OrderCloud.SDK;

namespace Headstart.Common.Controllers
{
    [Route("support")]
    public class SupportController : CatalystController
    {
        private static ICheckoutIntegrationCommand checkoutIntegrationCommand;
        private static IPostSubmitCommand postSubmitCommand;
        private readonly IOrderCloudClient oc;
        private readonly ISendgridService sendgrid;

        public SupportController(ICheckoutIntegrationCommand checkoutIntegrationCommand, IPostSubmitCommand postSubmitCommand, IZohoCommand zoho, IOrderCloudClient oc, ISupportAlertService supportAlertService, ISendgridService sendgrid)
        {
            SupportController.checkoutIntegrationCommand = checkoutIntegrationCommand;
            SupportController.postSubmitCommand = postSubmitCommand;
            this.oc = oc;
            this.sendgrid = sendgrid;
        }

        [HttpGet, Route("shipping")]
        public async Task<ShipEstimateResponse> GetShippingRates([FromBody] ShipmentTestModel model)
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

        [HttpGet, Route("zoho/{orderID}")]
        public async Task<OrderSubmitResponse> RetryOrderSubmit(string orderID)
        {
            var retry = await postSubmitCommand.HandleZohoRetry(orderID);
            return retry;
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
            await sendgrid.EmailGeneralSupportQueue(supportCase);
        }
    }

    public class ShipmentTestModel
    {
        public HSOrder Order { get; set; }

        public List<HSLineItem> LineItems { get; set; }
    }
}
