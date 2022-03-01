using OrderCloud.SDK;
using Headstart.Models;
using OrderCloud.Catalyst;
using System.Threading.Tasks;
using Headstart.API.Commands;
using Headstart.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Headstart.Models.Headstart;
using System.Collections.Generic;
using ordercloud.integrations.library;
using ordercloud.integrations.cardconnect;
using Headstart.Common.Services.ShippingIntegration.Models;

namespace Headstart.Common.Controllers
{
    [Route("order")]
    public class OrderController : CatalystController
    {

        private readonly IOrderCommand _command;
        private readonly IOrderSubmitCommand _orderSubmitCommand;
        private readonly ILineItemCommand _lineItemCommand;
        private readonly IOrderCloudClient _oc;

        /// <summary>
        /// The IOC based constructor method for the OrderController with Dependency Injection
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
        /// <param name="orderID"></param>
        /// <param name="payment"></param>
        /// <returns>The response from the order submission</returns>
        [HttpPost, Route("{direction}/{orderID}/submit"), OrderCloudUserAuth(ApiRole.Shopper)]
        public async Task<HSOrder> Submit(OrderDirection direction, string orderID, [FromBody] OrderCloudIntegrationsCreditCardPayment payment)
        {
            return await _orderSubmitCommand.SubmitOrderAsync(orderID, direction, payment, UserContext.AccessToken);
        }

        /// <summary>
        /// Posts the Acknowledge Quote Order action (POST method)
        /// </summary>
        /// <param name="orderID"></param>
        /// <returns>The response from the acknowledgement order submission</returns>
        [HttpPost, Route("acknowledgequote/{orderID}"), OrderCloudUserAuth(ApiRole.OrderAdmin)]
        public async Task<Order> AcknowledgeQuoteOrder(string orderID)
        {
            return await _command.AcknowledgeQuoteOrder(orderID);
        }

        /// <summary>
        /// Gets the ListPage of orders for a specific location as a buyer, ensuring user has access to location orders (GET method)
        /// </summary>
        /// <param name="locationID"></param>
        /// <param name="listArgs"></param>
        /// <returns>The ListPage of orders for a specific location as a buyer, ensureing user has access to location orders</returns>
        [HttpGet, Route("location/{locationID}"), OrderCloudUserAuth(ApiRole.Shopper)]
        public async Task<ListPage<HSOrder>> ListLocationOrders(string locationID, ListArgs<HSOrder> listArgs)
        {
            return await _command.ListOrdersForLocation(locationID, listArgs, UserContext);
        }

        /// <summary>
        /// Gets the order details as buyer, ensuring user has access to location orders or created the order themselves (GET method)
        /// </summary>
        /// <param name="orderID"></param>
        /// <returns>The order details as buyer, ensuring user has access to location orders or created the order themselves</returns>
        [HttpGet, Route("{orderID}/details"), OrderCloudUserAuth(ApiRole.Shopper)]
        public async Task<OrderDetails> GetOrderDetails(string orderID)
        {
            return await _command.GetOrderDetails(orderID, UserContext);
        }

        /// <summary>
        /// Gets the order shipments as buyer, ensuring user has access to location orders or created the order themselves (GET method)
        /// </summary>
        /// <param name="orderID"></param>
        /// <returns>The order shipments as buyer, ensuring user has access to location orders or created the order themselves</returns>
        [HttpGet, Route("{orderID}/shipmentswithitems"), OrderCloudUserAuth(ApiRole.Shopper)]
        public async Task<List<HSShipmentWithItems>> ListShipmentsWithItems(string orderID)
        {
            return await _command.ListHSShipmentWithItems(orderID, UserContext);
        }

        /// <summary>
        /// Gets the CosmosListPage of RMAs for an order (GET method)
        /// </summary>
        /// <param name="orderID"></param>
        /// <returns>The CosmosListPage of RMAs for an order</returns>
        [HttpGet, Route("rma/list/{orderID}"), OrderCloudUserAuth(ApiRole.Shopper)]
        public async Task<CosmosListPage<RMA>> ListRMAsForOrder(string orderID)
        {
            return await _command.ListRMAsForOrder(orderID, UserContext);
        }

        /// <summary>
        /// Creates/Updates a line item for an order (PUT method)
        /// </summary>
        /// <param name="orderID"></param>
        /// <param name="li"></param>
        /// <returns>The HSLineItem response from the UpsertLineItem action</returns>
        [HttpPut, Route("{orderID}/lineitems"), OrderCloudUserAuth(ApiRole.Shopper)]
        public async Task<HSLineItem> UpsertLineItem(string orderID, [FromBody] HSLineItem li)
        {
            return await _lineItemCommand.UpsertLineItem(orderID, li, UserContext);
        }

        /// <summary>
        /// Removes/Deletes a line item from an order (DELETE method)
        /// </summary>
        /// <param name="orderID"></param>
        /// <param name="lineItemID"></param>
        /// <returns></returns>
        [HttpDelete, Route("{orderID}/lineitems/{lineItemID}"), OrderCloudUserAuth(ApiRole.Shopper)]
        public async Task DeleteLineItem(string orderID, string lineItemID)
        {
            await _lineItemCommand.DeleteLineItem(orderID, lineItemID, UserContext);
        }

        /// <summary>
        /// Apply a promotion to an order (POST method)
        /// </summary>
        /// <param name="orderID"></param>
        /// <param name="promoCode"></param>
        /// <returns>The response from the AddPromotion action</returns>
        [HttpPost, Route("{orderID}/promotions/{promoCode}")]
        public async Task<HSOrder> AddPromotion(string orderID, string promoCode)
        {
            return await _command.AddPromotion(orderID, promoCode, UserContext);
        }

        /// <summary>
        /// Seller or Supplier Set Line Item Statuses On Order with Related Notification (POST method)
        /// </summary>
        /// <param name="orderID"></param>
        /// <param name="orderDirection"></param>
        /// <param name="lineItemStatusChanges"></param>
        /// <returns>The response from the SellerSupplierUpdateLineItemStatusesWithNotification action</returns>
        [HttpPost, Route("{orderID}/{orderDirection}/lineitem/status"), OrderCloudUserAuth(ApiRole.OrderAdmin)]
        public async Task<List<HSLineItem>> SellerSupplierUpdateLineItemStatusesWithNotification(string orderID, OrderDirection orderDirection, [FromBody] LineItemStatusChanges lineItemStatusChanges)
        {
            return await _lineItemCommand.UpdateLineItemStatusesAndNotifyIfApplicable(orderDirection, orderID, lineItemStatusChanges, UserContext);
        }

        /// <summary>
        /// Buyer Set Line Item Statuses On Order with Related Notification (POST method)
        /// </summary>
        /// <param name="orderID"></param>
        /// <param name="lineItemStatusChanges"></param>
        /// <returns>The response from the BuyerUpdateLineItemStatusesWithNotification action</returns>
        [HttpPost, Route("{orderID}/lineitem/status"), OrderCloudUserAuth(ApiRole.Shopper)]
        public async Task<List<HSLineItem>> BuyerUpdateLineItemStatusesWithNotification(string orderID, [FromBody] LineItemStatusChanges lineItemStatusChanges)
        {
            return await _lineItemCommand.UpdateLineItemStatusesAndNotifyIfApplicable(OrderDirection.Outgoing, orderID, lineItemStatusChanges, UserContext);
        }

        /// <summary>
        /// Apply Automatic Promtions to order and remove promotions no longer valid on order (POST method)
        /// </summary>
        /// <param name="orderID"></param>
        /// <returns>The HSOrder response from the ApplyAutomaticPromotions action</returns>
        [HttpPost, Route("{orderID}/applypromotions")]
        public async Task<HSOrder> ApplyAutomaticPromotions(string orderID)
        {
            return await _command.ApplyAutomaticPromotions(orderID);
        }

        /// <summary>
        /// Send quote request details to supplier (POST method)
        /// </summary>
        /// <param name="orderID"></param>
        /// <param name="lineItemID"></param>
        /// <returns>The HSLineItem response from the SendQuoteRequestToSupplier action</returns>
        [HttpPost, Route("submitquoterequest/{orderID}/{lineItemID}"), OrderCloudUserAuth(ApiRole.Shopper)]
        public async Task<HSLineItem> SendQuoteRequestToSupplier(string orderID, string lineItemID)
        {
            return await _command.SendQuoteRequestToSupplier(orderID, lineItemID);
        }

        /// <summary>
        /// Override unit price on order for quote order process (POST method)
        /// </summary>
        /// <param name="orderID"></param>
        /// <param name="lineItemID"></param>
        /// <param name="quotePrice"></param>
        /// <returns>The HSLineItem response from the OverrideQuotePrice action</returns>
        [HttpPost, Route("overridequote/{orderID}/{lineItemID}"), OrderCloudUserAuth(ApiRole.OrderAdmin)]
        public async Task<HSLineItem> OverrideQuotePrice(string orderID, string lineItemID, [FromBody] decimal quotePrice)
        {
            return await _command.OverrideQuotePrice(orderID, lineItemID, quotePrice);
        }

        /// <summary>
        /// Gets the ListPage of quote orders, which are in an unsubmitted status
        /// </summary>
        /// <param name="quoteStatus"></param>
        /// <returns>The ListPage of quote orders, which are in an unsubmitted status</returns>
        [HttpGet, Route("listquoteorders/{quoteStatus}"), OrderCloudUserAuth(ApiRole.OrderReader, ApiRole.OrderAdmin)]
        public async Task<ListPage<HSOrder>> ListQuoteOrders(QuoteStatus quoteStatus)
        {
            var me = await _oc.Me.GetAsync(accessToken: UserContext.AccessToken);
            return await _command.ListQuoteOrders(me, quoteStatus);
        }

        /// <summary>
        /// Gets the single quote order
        /// </summary>
        /// <param name="orderID"></param>
        /// <returns>The HSOrder response from the GetQuoteOrder action</returns>
        [HttpGet, Route("getquoteorder/{orderID}"), OrderCloudUserAuth(ApiRole.OrderReader, ApiRole.OrderAdmin)]
        public async Task<HSOrder> GetQuoteOrder(string orderID)
        {
            var me = await _oc.Me.GetAsync(accessToken: UserContext.AccessToken);
            return await _command.GetQuoteOrder(me, orderID);
        }
    }
}