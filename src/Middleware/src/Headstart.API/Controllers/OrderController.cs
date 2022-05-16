using System.Collections.Generic;
using System.Threading.Tasks;
using Headstart.API.Commands;
using Headstart.Common.Models;
using Headstart.Common.Services.ShippingIntegration.Models;
using Headstart.Models;
using Headstart.Models.Headstart;
using Microsoft.AspNetCore.Mvc;
using OrderCloud.Catalyst;
using OrderCloud.Integrations.CardConnect.Models;
using OrderCloud.Integrations.Library;
using OrderCloud.SDK;

namespace Headstart.Common.Controllers
{
    /// <summary>
    /// Order commands in Headstart.
    /// </summary>
    [Route("order")]
    public class OrderController : CatalystController
    {
        private readonly IOrderCommand command;
        private readonly IOrderSubmitCommand orderSubmitCommand;
        private readonly ILineItemCommand lineItemCommand;
        private readonly IOrderCloudClient oc;

        public OrderController(IOrderCommand command, ILineItemCommand lineItemCommand, IOrderSubmitCommand orderSubmitCommand, IOrderCloudClient oc)
        {
            this.command = command;
            this.lineItemCommand = lineItemCommand;
            this.orderSubmitCommand = orderSubmitCommand;
            this.oc = oc;
        }

        /// <summary>
        /// Submit Order. Performs validation, submits credit card payment and finally submits order via OrderCloud.
        /// </summary>
        [HttpPost, Route("{direction}/{orderID}/submit"), OrderCloudUserAuth(ApiRole.Shopper)]
        public async Task<HSOrder> Submit(OrderDirection direction, string orderID, [FromBody] OrderCloudIntegrationsCreditCardPayment payment)
        {
            return await orderSubmitCommand.SubmitOrderAsync(orderID, direction, payment, UserContext.AccessToken);
        }

        /// <summary>
        /// POST Acknowledge Quote Order.
        /// </summary>
        [HttpPost, Route("acknowledgequote/{orderID}"), OrderCloudUserAuth(ApiRole.OrderAdmin)]
        public async Task<Order> AcknowledgeQuoteOrder(string orderID)
        {
            return await command.AcknowledgeQuoteOrder(orderID);
        }

        /// <summary>
        /// LIST orders for a specific location as a buyer, ensures user has access to location orders.
        /// </summary>
        [HttpGet, Route("location/{locationID}"), OrderCloudUserAuth(ApiRole.Shopper)]
        public async Task<ListPage<HSOrder>> ListLocationOrders(string locationID, ListArgs<HSOrder> listArgs)
        {
            return await command.ListOrdersForLocation(locationID, listArgs, UserContext);
        }

        /// <summary>
        /// GET order details as buyer, ensures user has access to location orders or created the order themselves.
        /// </summary>
        [HttpGet, Route("{orderID}/details"), OrderCloudUserAuth(ApiRole.Shopper)]
        public async Task<OrderDetails> GetOrderDetails(string orderID)
        {
            return await command.GetOrderDetails(orderID, UserContext);
        }

        /// <summary>
        /// GET order shipments as buyer, ensures user has access to location orders or created the order themselves.
        /// </summary>
        [HttpGet, Route("{orderID}/shipmentswithitems"), OrderCloudUserAuth(ApiRole.Shopper)]
        public async Task<List<HSShipmentWithItems>> ListShipmentsWithItems(string orderID)
        {
            return await command.ListHSShipmentWithItems(orderID, UserContext);
        }

        [HttpGet, Route("rma/list/{orderID}"), OrderCloudUserAuth(ApiRole.Shopper)]
        public async Task<CosmosListPage<RMA>> ListRMAsForOrder(string orderID)
        {
            return await command.ListRMAsForOrder(orderID, UserContext);
        }

        [HttpPut, Route("{orderID}/lineitems"), OrderCloudUserAuth(ApiRole.Shopper)]
        public async Task<HSLineItem> UpsertLineItem(string orderID, [FromBody] HSLineItem li)
        {
            return await lineItemCommand.UpsertLineItem(orderID, li, UserContext);
        }

        /// <summary>
        /// Delete a line item.
        /// </summary>
        [HttpDelete, Route("{orderID}/lineitems/{lineItemID}"), OrderCloudUserAuth(ApiRole.Shopper)]
        public async Task DeleteLineItem(string orderID, string lineItemID)
        {
            await lineItemCommand.DeleteLineItem(orderID, lineItemID, UserContext);
        }

        /// <summary>
        /// Apply a promotion to an order.
        /// </summary>
        [HttpPost, Route("{orderID}/promotions/{promoCode}")]
        public async Task<HSOrder> AddPromotion(string orderID, string promoCode)
        {
            return await command.AddPromotion(orderID, promoCode, UserContext);
        }

        /// <summary>
        /// Seller or Supplier Set Line Item Statuses On Order with Related Notification.
        /// </summary>
        [HttpPost, Route("{orderID}/{orderDirection}/lineitem/status"), OrderCloudUserAuth(ApiRole.OrderAdmin)]
        public async Task<List<HSLineItem>> SellerSupplierUpdateLineItemStatusesWithNotification(string orderID, OrderDirection orderDirection, [FromBody] LineItemStatusChanges lineItemStatusChanges)
        {
            return await lineItemCommand.UpdateLineItemStatusesAndNotifyIfApplicable(orderDirection, orderID, lineItemStatusChanges, UserContext);
        }

        /// <summary>
        /// Buyer Set Line Item Statuses On Order with Related Notification.
        /// </summary>
        [HttpPost, Route("{orderID}/lineitem/status"), OrderCloudUserAuth(ApiRole.Shopper)]
        public async Task<List<HSLineItem>> BuyerUpdateLineItemStatusesWithNotification(string orderID, [FromBody] LineItemStatusChanges lineItemStatusChanges)
        {
            return await lineItemCommand.UpdateLineItemStatusesAndNotifyIfApplicable(OrderDirection.Outgoing, orderID, lineItemStatusChanges, UserContext);
        }

        /// <summary>
        /// Apply Automatic Promtions to order and remove promotions no longer valid on order.
        /// </summary>
        [HttpPost, Route("{orderID}/applypromotions")]
        public async Task<HSOrder> ApplyAutomaticPromotions(string orderID)
        {
            return await command.ApplyAutomaticPromotions(orderID);
        }

        /// <summary>
        /// Send quote request details to supplier.
        /// </summary>
        [HttpPost, Route("submitquoterequest/{orderID}/{lineItemID}"), OrderCloudUserAuth(ApiRole.Shopper)]
        public async Task<HSLineItem> SendQuoteRequestToSupplier(string orderID, string lineItemID)
        {
            return await command.SendQuoteRequestToSupplier(orderID, lineItemID);
        }

        /// <summary>
        /// Override unit price on order for quote order process.
        /// </summary>
        [HttpPost, Route("overridequote/{orderID}/{lineItemID}"), OrderCloudUserAuth(ApiRole.OrderAdmin)]
        public async Task<HSLineItem> OverrideQuotePrice(string orderID, string lineItemID, [FromBody] decimal quotePrice)
        {
            return await command.OverrideQuotePrice(orderID, lineItemID, quotePrice);
        }

        /// <summary>
        /// Lists quote orders, which are in an unsubmitted status.
        /// </summary>
        [HttpGet, Route("listquoteorders/{quoteStatus}"), OrderCloudUserAuth(ApiRole.OrderReader, ApiRole.OrderAdmin)]
        public async Task<ListPage<HSOrder>> ListQuoteOrders(QuoteStatus quoteStatus)
        {
            var me = await oc.Me.GetAsync(accessToken: UserContext.AccessToken);
            return await command.ListQuoteOrders(me, quoteStatus);
        }

        /// <summary>
        /// Gets a single quote order.
        /// </summary>
        [HttpGet, Route("getquoteorder/{orderID}"), OrderCloudUserAuth(ApiRole.OrderReader, ApiRole.OrderAdmin)]
        public async Task<HSOrder> GetQuoteOrder(string orderID)
        {
            var me = await oc.Me.GetAsync(accessToken: UserContext.AccessToken);
            return await command.GetQuoteOrder(me, orderID);
        }
    }
}
