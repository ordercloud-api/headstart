using OrderCloud.SDK;
using OrderCloud.Catalyst;
using System.Threading.Tasks;
using Headstart.API.Commands;
using Microsoft.AspNetCore.Mvc;
using Headstart.Common.Services;
using System.Collections.Generic;
using Headstart.API.Commands.Zoho;
using Headstart.Common.Models.Misc;
using Headstart.Common.Models.Headstart;

namespace Headstart.API.Controllers
{
	[Route("support")]
	public class SupportController : CatalystController
	{
		private static ICheckoutIntegrationCommand _checkoutIntegrationCommand;
		private static IPostSubmitCommand _postSubmitCommand;
		private readonly IZohoCommand _zoho;
		private readonly IOrderCloudClient _oc;
		private readonly ISendgridService _sendgrid;

		/// <summary>
		/// The IOC based constructor method for the SupportController class object with Dependency Injection
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
			_zoho = zoho;
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
			var payload = new HsOrderCalculatePayload()
			{
				ConfigData = null,
				OrderWorksheet = new HsOrderWorksheet()
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
		/// <param name="orderId"></param>
		/// <returns>The OrderCalculateResponse object by the orderID</returns>
		[HttpGet, Route("tax/{orderId}"), OrderCloudUserAuth(ApiRole.IntegrationEventAdmin)]
		public async Task<OrderCalculateResponse> CalculateOrder(string orderId)
		{
			var orderCalculationResponse = await _checkoutIntegrationCommand.CalculateOrder(orderId, UserContext);
			return orderCalculationResponse;
		}

		/// <summary>
		/// Gets the OrderSubmitResponse object by the orderID (GET method)
		/// </summary>
		/// <param name="orderId"></param>
		/// <returns>The OrderSubmitResponse object by the orderID</returns>
		[HttpGet, Route("zoho/{orderId}")]
		public async Task<OrderSubmitResponse> RetryOrderSubmit(string orderId)
		{
			var retry = await _postSubmitCommand.HandleZohoRetry(orderId);
			return retry;
		}

		/// <summary>
		/// Gets the OrderSubmitResponse object by the orderID (GET method)
		/// </summary>
		/// <param name="orderId"></param>
		/// <returns>The OrderSubmitResponse object by the orderID</returns>
		[HttpGet, Route("shipping/validate/{orderId}"), OrderCloudUserAuth(ApiRole.IntegrationEventAdmin)]
		public async Task<OrderSubmitResponse> RetryShippingValidate(string orderId)
		{
			var retry = await _postSubmitCommand.HandleShippingValidate(orderId, UserContext);
			return retry;
		}

		/// <summary>
		/// Gets the ShipEstimateResponse object by the orderID (GET method)
		/// </summary>
		/// <param name="orderId"></param>
		/// <returns>The ShipEstimateResponse object by the orderID</returns>
		[HttpGet, Route("shippingrates/{orderId}")]
		public async Task<ShipEstimateResponse> GetShippingRates(string orderId)
		{
			return await _checkoutIntegrationCommand.GetRatesAsync(orderId);
		}

		/// <summary>
		/// Post action to manually run the Post Order submission process (POST method)
		/// </summary>
		/// <param name="orderId"></param>
		/// <returns>The response from the post action to manually run the Post Order submission process</returns>
		[HttpPost, Route("postordersubmit/{orderId}"), OrderCloudUserAuth]
		public async Task<OrderSubmitResponse> ManuallyRunPostOrderSubmit(string orderId)
		{
			var worksheet = await _oc.IntegrationEvents.GetWorksheetAsync<HsOrderWorksheet>(OrderDirection.Incoming, orderId);
			return await _postSubmitCommand.HandleBuyerOrderSubmit(worksheet);
		}

		/// <summary>
		/// Post action to send an email Support request (POST method)
		/// </summary>
		/// <param name="supportCase"></param>
		/// <returns></returns>
		[HttpPost, Route("submitcase")]
		public async Task SendSupportRequest([FromForm] SupportCase supportCase)
		{
			await _sendgrid.EmailGeneralSupportQueue(supportCase);
		}
	}

	public class ShipmentTestModel
	{
		public HsOrder Order { get; set; } = new HsOrder();
		public List<HsLineItem> LineItems { get; set; } = new List<HsLineItem>();
	}
}