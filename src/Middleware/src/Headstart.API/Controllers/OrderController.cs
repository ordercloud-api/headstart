using OrderCloud.SDK;
using OrderCloud.Catalyst;
using System.Threading.Tasks;
using Headstart.API.Commands;
using Headstart.Common.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using ordercloud.integrations.library;
using Headstart.Common.Models.Headstart;
using Headstart.Common.Repositories.Models;
using ordercloud.integrations.cardconnect;

namespace Headstart.API.Controllers
{
	[Route("order")]
	public class OrderController : CatalystController
	{
		private readonly IOrderCommand _command;
		private readonly IOrderSubmitCommand _orderSubmitCommand;
		private readonly ILineItemCommand _lineItemCommand;
		private readonly IOrderCloudClient _oc;

		/// <summary>
		/// The IOC based constructor method for the OrderController class object with Dependency Injection
		/// </summary>
		/// <param name="command"></param>
		/// <param name="lineItemCommand"></param>
		/// <param name="orderSubmitCommand"></param>
		/// <param name="oc"></param>
		public OrderController(IOrderCommand command, ILineItemCommand lineItemCommand, IOrderSubmitCommand orderSubmitCommand, IOrderCloudClient oc)
		{
			_command = command;
			_lineItemCommand = lineItemCommand;
			_orderSubmitCommand = orderSubmitCommand;
			_oc = oc;
		}

		/// <summary>
		/// Submit Order. Performs validation, submits credit card payment and finally submits order via OrderCloud (POST method)
		/// </summary>
		/// <param name="direction"></param>
		/// <param name="orderId"></param>
		/// <param name="payment"></param>
		/// <returns>The response from the order submission</returns>
		[HttpPost, Route("{direction}/{orderId}/submit"), OrderCloudUserAuth(ApiRole.Shopper)]
		public async Task<HsOrder> Submit(OrderDirection direction, string orderId, [FromBody] OrderCloudIntegrationsCreditCardPayment payment)
		{
			return await _orderSubmitCommand.SubmitOrderAsync(orderId, direction, payment, UserContext.AccessToken);
		}

		/// <summary>
		/// Posts the Acknowledge Quote Order action (POST method)
		/// </summary>
		/// <param name="orderId"></param>
		/// <returns>The response from the acknowledgement order submission</returns>
		[HttpPost, Route("acknowledgequote/{orderId}"), OrderCloudUserAuth(ApiRole.OrderAdmin)]
		public async Task<Order> AcknowledgeQuoteOrder(string orderId)
		{
			return await _command.AcknowledgeQuoteOrder(orderId);
		}

		/// <summary>
		/// Gets the ListPage of orders for a specific location as a buyer, ensuring user has access to location orders (GET method)
		/// </summary>
		/// <param name="locationId"></param>
		/// <param name="listArgs"></param>
		/// <returns>The ListPage of orders for a specific location as a buyer, ensureing user has access to location orders</returns>
		[HttpGet, Route("location/{locationId}"), OrderCloudUserAuth(ApiRole.Shopper)]
		public async Task<ListPage<HsOrder>> ListLocationOrders(string locationId, ListArgs<HsOrder> listArgs)
		{
			return await _command.ListOrdersForLocation(locationId, listArgs, UserContext);
		}

		/// <summary>
		/// Gets the order details as buyer, ensuring user has access to location orders or created the order themselves (GET method)
		/// </summary>
		/// <param name="orderId"></param>
		/// <returns>The order details as buyer, ensuring user has access to location orders or created the order themselves</returns>
		[HttpGet, Route("{orderId}/details"), OrderCloudUserAuth(ApiRole.Shopper)]
		public async Task<OrderDetails> GetOrderDetails(string orderId)
		{
			return await _command.GetOrderDetails(orderId, UserContext);
		}

		/// <summary>
		/// Gets the order shipments as buyer, ensuring user has access to location orders or created the order themselves (GET method)
		/// </summary>
		/// <param name="orderId"></param>
		/// <returns>The order shipments as buyer, ensuring user has access to location orders or created the order themselves</returns>
		[HttpGet, Route("{orderID}/shipmentswithitems"), OrderCloudUserAuth(ApiRole.Shopper)]
		public async Task<List<HsShipmentWithItems>> ListShipmentsWithItems(string orderId)
		{
			return await _command.ListHsShipmentWithItems(orderId, UserContext);
		}

		/// <summary>
		/// Gets the CosmosListPage of RMAs for an order (GET method)
		/// </summary>
		/// <param name="orderId"></param>
		/// <returns>The CosmosListPage of RMAs for an order</returns>
		[HttpGet, Route("rma/list/{orderId}"), OrderCloudUserAuth(ApiRole.Shopper)]
		public async Task<CosmosListPage<RMA>> ListRMAsForOrder(string orderId)
		{
			return await _command.ListRMAsForOrder(orderId, UserContext);
		}

		/// <summary>
		/// Creates/Updates a line item for an order (PUT method)
		/// </summary>
		/// <param name="orderId"></param>
		/// <param name="li"></param>
		/// <returns>The HsLineItem response from the UpsertLineItem action</returns>
		[HttpPut, Route("{orderId}/lineitems"), OrderCloudUserAuth(ApiRole.Shopper)]
		public async Task<HsLineItem> UpsertLineItem(string orderId, [FromBody] HsLineItem li)
		{
			return await _lineItemCommand.UpsertLineItem(orderId, li, UserContext);
		}

		/// <summary>
		/// Removes/Deletes a line item from an order (DELETE method)
		/// </summary>
		/// <param name="orderId"></param>
		/// <param name="lineItemId"></param>
		/// <returns></returns>
		[HttpDelete, Route("{orderID}/lineitems/{lineItemId}"), OrderCloudUserAuth(ApiRole.Shopper)]
		public async Task DeleteLineItem(string orderId, string lineItemId)
		{
			await _lineItemCommand.DeleteLineItem(orderId, lineItemId, UserContext);
		}

		/// <summary>
		/// Apply a promotion to an order (POST method)
		/// </summary>
		/// <param name="orderId"></param>
		/// <param name="promoCode"></param>
		/// <returns>The response from the AddPromotion action</returns>
		[HttpPost, Route("{orderId}/promotions/{promoCode}")]
		public async Task<HsOrder> AddPromotion(string orderId, string promoCode)
		{
			return await _command.AddPromotion(orderId, promoCode, UserContext);
		}

		/// <summary>
		/// Seller or Supplier Set Line Item Statuses On Order with Related Notification (POST method)
		/// </summary>
		/// <param name="orderId"></param>
		/// <param name="orderDirection"></param>
		/// <param name="lineItemStatusChanges"></param>
		/// <returns>The response from the SellerSupplierUpdateLineItemStatusesWithNotification action</returns>
		[HttpPost, Route("{orderID}/{orderDirection}/lineitem/status"), OrderCloudUserAuth(ApiRole.OrderAdmin)]
		public async Task<List<HsLineItem>> SellerSupplierUpdateLineItemStatusesWithNotification(string orderId, OrderDirection orderDirection, [FromBody] LineItemStatusChanges lineItemStatusChanges)
		{
			return await _lineItemCommand.UpdateLineItemStatusesAndNotifyIfApplicable(orderDirection, orderId, lineItemStatusChanges, UserContext);
		}

		/// <summary>
		/// Buyer Set Line Item Statuses On Order with Related Notification (POST method)
		/// </summary>
		/// <param name="orderId"></param>
		/// <param name="lineItemStatusChanges"></param>
		/// <returns>The response from the BuyerUpdateLineItemStatusesWithNotification action</returns>
		[HttpPost, Route("{orderId}/lineitem/status"), OrderCloudUserAuth(ApiRole.Shopper)]
		public async Task<List<HsLineItem>> BuyerUpdateLineItemStatusesWithNotification(string orderId, [FromBody] LineItemStatusChanges lineItemStatusChanges)
		{
			return await _lineItemCommand.UpdateLineItemStatusesAndNotifyIfApplicable(OrderDirection.Outgoing, orderId, lineItemStatusChanges, UserContext);
		}

		/// <summary>
		/// Apply Automatic Promtions to order and remove promotions no longer valid on order (POST method)
		/// </summary>
		/// <param name="orderId"></param>
		/// <returns>The HsOrder response from the ApplyAutomaticPromotions action</returns>
		[HttpPost, Route("{orderId}/applypromotions")]
		public async Task<HsOrder> ApplyAutomaticPromotions(string orderId)
		{
			return await _command.ApplyAutomaticPromotions(orderId);
		}

		/// <summary>
		/// Send quote request details to supplier (POST method)
		/// </summary>
		/// <param name="orderId"></param>
		/// <param name="lineItemId"></param>
		/// <returns>The HsLineItem response from the SendQuoteRequestToSupplier action</returns>
		[HttpPost, Route("submitquoterequest/{orderId}/{lineItemId}"), OrderCloudUserAuth(ApiRole.Shopper)]
		public async Task<HsLineItem> SendQuoteRequestToSupplier(string orderId, string lineItemId)
		{
			return await _command.SendQuoteRequestToSupplier(orderId, lineItemId);
		}

		/// <summary>
		/// Override unit price on order for quote order process (POST method)
		/// </summary>
		/// <param name="orderId"></param>
		/// <param name="lineItemId"></param>
		/// <param name="quotePrice"></param>
		/// <returns>The HsLineItem response from the OverrideQuotePrice action</returns>
		[HttpPost, Route("overridequote/{orderId}/{lineItemId}"), OrderCloudUserAuth(ApiRole.OrderAdmin)]
		public async Task<HsLineItem> OverrideQuotePrice(string orderId, string lineItemId, [FromBody] decimal quotePrice)
		{
			return await _command.OverrideQuotePrice(orderId, lineItemId, quotePrice);
		}

		/// <summary>
		/// Gets the ListPage of quote orders, which are in an unsubmitted status
		/// </summary>
		/// <param name="quoteStatus"></param>
		/// <returns>The ListPage of quote orders, which are in an unsubmitted status</returns>
		[HttpGet, Route("listquoteorders/{quoteStatus}"), OrderCloudUserAuth(ApiRole.OrderReader, ApiRole.OrderAdmin)]
		public async Task<ListPage<HsOrder>> ListQuoteOrders(QuoteStatus quoteStatus)
		{
			var me = await _oc.Me.GetAsync(accessToken: UserContext.AccessToken);
			return await _command.ListQuoteOrders(me, quoteStatus);
		}

		/// <summary>
		/// Gets the single quote order
		/// </summary>
		/// <param name="orderId"></param>
		/// <returns>The HsOrder response from the GetQuoteOrder action</returns>
		[HttpGet, Route("getquoteorder/{orderId}"), OrderCloudUserAuth(ApiRole.OrderReader, ApiRole.OrderAdmin)]
		public async Task<HsOrder> GetQuoteOrder(string orderId)
		{
			var me = await _oc.Me.GetAsync(accessToken: UserContext.AccessToken);
			return await _command.GetQuoteOrder(me, orderId);
		}
	}
}