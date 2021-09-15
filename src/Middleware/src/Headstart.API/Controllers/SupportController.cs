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
using ordercloud.integrations.library;
using OrderCloud.Catalyst;
using OrderCloud.SDK;

namespace Headstart.Common.Controllers
{
    [Route("support")]
    public class SupportController : CatalystController
    {
        private static ICheckoutIntegrationCommand _checkoutIntegrationCommand;
        private static IPostSubmitCommand _postSubmitCommand;
        private readonly IOrderCloudClient _oc;
        private readonly ISendgridService _sendgrid;

        public SupportController(ICheckoutIntegrationCommand checkoutIntegrationCommand, IPostSubmitCommand postSubmitCommand, IZohoCommand zoho, IOrderCloudClient oc, ISupportAlertService supportAlertService, ISendgridService sendgrid)
        {
            _checkoutIntegrationCommand = checkoutIntegrationCommand;
            _postSubmitCommand = postSubmitCommand;
            _oc = oc;
            _sendgrid = sendgrid;
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
                    LineItems = model.LineItems
                }
            };
            return await _checkoutIntegrationCommand.GetRatesAsync(payload);
        }

        [Route("tax/{orderID}")]
        [HttpGet, OrderCloudUserAuth(ApiRole.IntegrationEventAdmin)]
        public async Task<OrderCalculateResponse> CalculateOrder(string orderID)
        {
            var orderCalculationResponse = await _checkoutIntegrationCommand.CalculateOrder(orderID, UserContext);
            return orderCalculationResponse;
        }

        [HttpGet, Route("zoho/{orderID}")]
        public async Task<OrderSubmitResponse> RetryOrderSubmit(string orderID)
        {
            var retry = await _postSubmitCommand.HandleZohoRetry(orderID);
            return retry;
        }

        [HttpGet, Route("shipping/validate/{orderID}"), OrderCloudUserAuth(ApiRole.IntegrationEventAdmin)]
        public async Task<OrderSubmitResponse> RetryShippingValidate(string orderID)
        {
            var retry = await _postSubmitCommand.HandleShippingValidate(orderID, UserContext);
            return retry;
        }

        // good debug method for testing rates with orders
        [HttpGet, Route("shippingrates/{orderID}")]
        public async Task<ShipEstimateResponse> GetShippingRates(string orderID)
        {
            return await _checkoutIntegrationCommand.GetRatesAsync(orderID);
        }

        [HttpPost, Route("postordersubmit/{orderID}"), OrderCloudUserAuth]
        public async Task<OrderSubmitResponse> ManuallyRunPostOrderSubmit(string orderID)
        {
            var worksheet = await _oc.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.Incoming, orderID);
            return await _postSubmitCommand.HandleBuyerOrderSubmit(worksheet);
        }

        [HttpPost, Route("submitcase")]
        public async Task SendSupportRequest([FromForm]SupportCase supportCase)
        {
            await _sendgrid.EmailGeneralSupportQueue(supportCase);
        }
    }

    public class ShipmentTestModel
    {
        public HSOrder Order { get; set; }
        public List<HSLineItem> LineItems { get; set; }
    }
}
