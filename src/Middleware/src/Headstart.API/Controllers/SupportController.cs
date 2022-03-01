using OrderCloud.SDK;
using Headstart.Models;
using OrderCloud.Catalyst;
using System.Threading.Tasks;
using Headstart.API.Commands;
using Microsoft.AspNetCore.Mvc;
using Headstart.Common.Services;
using System.Collections.Generic;
using Headstart.Models.Headstart;
using Headstart.API.Commands.Zoho;
using Headstart.Common.Models.Misc;
using Headstart.Common.Services.ShippingIntegration.Models;

namespace Headstart.Common.Controllers
{
    [Route("support")]
    public class SupportController : CatalystController
    {
        private static ICheckoutIntegrationCommand _checkoutIntegrationCommand;
        private static IPostSubmitCommand _postSubmitCommand;
        private readonly IOrderCloudClient _oc;
        private readonly ISendgridService _sendgrid;

        /// <summary>
        /// The IOC based constructor method for the SupportController with Dependency Injection
        /// </summary>
        /// <param name="checkoutIntegrationCommand"></param>
        /// <param name="postSubmitCommand"></param>
        /// <param name="zoho"></param>
        /// <param name="oc"></param>
        /// <param name="supportAlertService"></param>
        /// <param name="sendgrid"></param>
        public SupportController(ICheckoutIntegrationCommand checkoutIntegrationCommand, IPostSubmitCommand postSubmitCommand, IZohoCommand zoho, IOrderCloudClient oc, ISupportAlertService supportAlertService, ISendgridService sendgrid)
        {
            _checkoutIntegrationCommand = checkoutIntegrationCommand;
            _postSubmitCommand = postSubmitCommand;
            _oc = oc;
            _sendgrid = sendgrid;
        }

        /// <summary>
        /// Gets the ShipEstimateResponse object from the form submitted ShipmentTestModel object (GET method)
        /// </summary>
        /// <param name="model"></param>
        /// <returns>The ShipEstimateResponse object from the form submitted ShipmentTestModel object</returns>
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

        /// <summary>
        /// Gets the OrderCalculateResponse object by the orderID (GET method)
        /// </summary>
        /// <param name="orderID"></param>
        /// <returns>The OrderCalculateResponse object by the orderID</returns>
        [HttpGet, Route("tax/{orderID}"), OrderCloudUserAuth(ApiRole.IntegrationEventAdmin)]
        public async Task<OrderCalculateResponse> CalculateOrder(string orderID)
        {
            var orderCalculationResponse = await _checkoutIntegrationCommand.CalculateOrder(orderID, UserContext);
            return orderCalculationResponse;
        }

        /// <summary>
        /// Gets the OrderSubmitResponse object by the orderID (GET method)
        /// </summary>
        /// <param name="orderID"></param>
        /// <returns>The OrderSubmitResponse object by the orderID</returns>
        [HttpGet, Route("zoho/{orderID}")]
        public async Task<OrderSubmitResponse> RetryOrderSubmit(string orderID)
        {
            var retry = await _postSubmitCommand.HandleZohoRetry(orderID);
            return retry;
        }

        /// <summary>
        /// Gets the OrderSubmitResponse object by the orderID (GET method)
        /// </summary>
        /// <param name="orderID"></param>
        /// <returns>The OrderSubmitResponse object by the orderID</returns>
        [HttpGet, Route("shipping/validate/{orderID}"), OrderCloudUserAuth(ApiRole.IntegrationEventAdmin)]
        public async Task<OrderSubmitResponse> RetryShippingValidate(string orderID)
        {
            var retry = await _postSubmitCommand.HandleShippingValidate(orderID, UserContext);
            return retry;
        }

        /// <summary>
        /// Gets the ShipEstimateResponse object by the orderID (GET method)
        /// </summary>
        /// <param name="orderID"></param>
        /// <returns>The ShipEstimateResponse object by the orderID</returns>
        [HttpGet, Route("shippingrates/{orderID}")]
        public async Task<ShipEstimateResponse> GetShippingRates(string orderID)
        {
            return await _checkoutIntegrationCommand.GetRatesAsync(orderID);
        }

        /// <summary>
        /// Post action to manually run the Post Order submission process (POST method)
        /// </summary>
        /// <param name="orderID"></param>
        /// <returns>The response from the post action to manually run the Post Order submission process</returns>
        [HttpPost, Route("postordersubmit/{orderID}"), OrderCloudUserAuth]
        public async Task<OrderSubmitResponse> ManuallyRunPostOrderSubmit(string orderID)
        {
            var worksheet = await _oc.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.Incoming, orderID);
            return await _postSubmitCommand.HandleBuyerOrderSubmit(worksheet);
        }

        /// <summary>
        /// Post action to send an email Support request (POST method)
        /// </summary>
        /// <param name="supportCase"></param>
        /// <returns></returns>
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