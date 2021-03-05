using Headstart.Models;
using Microsoft.AspNetCore.Mvc;
using OrderCloud.SDK;
using System.Threading.Tasks;
using Headstart.Models.Attributes;
using System.Collections.Generic;
using ordercloud.integrations.library;
using Headstart.Models.Headstart;
using Headstart.Common.Services.ShippingIntegration.Models;
using ordercloud.integrations.cardconnect;
using Headstart.API.Commands;
using OrderCloud.Catalyst;

namespace Headstart.Common.Controllers
{
    [DocComments("\"Headstart Orders\" for handling order commands in Headstart")]
    [HSSection.Headstart(ListOrder = 2)]
    [Route("order")]
    public class OrderController : BaseController
    {
        
        private readonly IOrderCommand _command;
        private readonly IOrderSubmitCommand _orderSubmitCommand;
        private readonly ILineItemCommand _lineItemCommand;
        public OrderController(IOrderCommand command, ILineItemCommand lineItemCommand, IOrderSubmitCommand orderSubmitCommand)
        {
            _command = command;
            _lineItemCommand = lineItemCommand;
            _orderSubmitCommand = orderSubmitCommand;
        }

        [DocName("Submit Order")]
        [DocComments("Performs validation, submits credit card payment and finally submits order via OrderCloud")]
        [HttpPost, Route("{direction}/{orderID}/submit"), OrderCloudUserAuth(ApiRole.Shopper)]
        public async Task<HSOrder> Submit(OrderDirection direction, string orderID, [FromBody] OrderCloudIntegrationsCreditCardPayment payment)
        {
            return await _orderSubmitCommand.SubmitOrderAsync(orderID, direction, payment, UserContext.AccessToken);
        }

        [DocName("POST Acknowledge Quote Order")]
        [HttpPost, Route("acknowledgequote/{orderID}"), OrderCloudUserAuth(ApiRole.OrderAdmin)]
        public async Task<Order> AcknowledgeQuoteOrder(string orderID)
        {
            return await _command.AcknowledgeQuoteOrder(orderID);
        }

        [DocName("LIST orders for a specific location as a buyer, ensures user has access to location orders")]
        [HttpGet, Route("location/{locationID}"), OrderCloudUserAuth(ApiRole.Shopper)]
        public async Task<ListPage<HSOrder>> ListLocationOrders(string locationID, ListArgs<HSOrder> listArgs)
        {
            return await _command.ListOrdersForLocation(locationID, listArgs, UserContext);
        }

        [DocName("GET order details as buyer, ensures user has access to location orders or created the order themselves")]
        [HttpGet, Route("{orderID}/details"), OrderCloudUserAuth(ApiRole.Shopper)]
        public async Task<OrderDetails> GetOrderDetails(string orderID)
        {
            return await _command.GetOrderDetails(orderID, UserContext);
        }

        [DocName("GET order shipments as buyer, ensures user has access to location orders or created the order themselves")]
        [HttpGet, Route("{orderID}/shipmentswithitems"), OrderCloudUserAuth(ApiRole.Shopper)]
        public async Task<List<HSShipmentWithItems>> ListShipmentsWithItems(string orderID)
        {
            return await _command.ListHSShipmentWithItems(orderID, UserContext);
        }

        [DocName("Add or update a line item to an order")]
        [HttpPut, Route("{orderID}/lineitems"), OrderCloudUserAuth(ApiRole.Shopper)]
        public async Task<HSLineItem> UpsertLineItem(string orderID, [FromBody] HSLineItem li)
        {
            return await _lineItemCommand.UpsertLineItem(orderID, li, UserContext);
        }

        [DocName("Delete a line item")]
        [HttpDelete, Route("{orderID}/lineitems/{lineItemID}"), OrderCloudUserAuth(ApiRole.Shopper)]
        public async Task DeleteLineItem(string orderID, string lineItemID)
        {
            await _lineItemCommand.DeleteLineItem(orderID, lineItemID, UserContext);
        }

        [DocName("Apply a promotion to an order")]
        [HttpPost, Route("{orderID}/promotions/{promoCode}")]
        public async Task<HSOrder> AddPromotion(string orderID, string promoCode)
        {
            return await _command.AddPromotion(orderID, promoCode, UserContext);
        }

        [DocName("Seller or Supplier Set Line Item Statuses On Order with Related Notification")]
        [HttpPost, Route("{orderID}/{orderDirection}/lineitem/status"), OrderCloudUserAuth(ApiRole.OrderAdmin)]
        public async Task<List<HSLineItem>> SellerSupplierUpdateLineItemStatusesWithNotification(string orderID, OrderDirection orderDirection, [FromBody] LineItemStatusChanges lineItemStatusChanges)
        {
            return await _lineItemCommand.UpdateLineItemStatusesAndNotifyIfApplicable(orderDirection, orderID, lineItemStatusChanges, UserContext);
        }

        [DocName("Buyer Set Line Item Statuses On Order with Related Notification")]
        [HttpPost, Route("{orderID}/lineitem/status"), OrderCloudUserAuth(ApiRole.Shopper)]
        public async Task<List<HSLineItem>> BuyerUpdateLineItemStatusesWithNotification(string orderID, [FromBody] LineItemStatusChanges lineItemStatusChanges)
        {
            return await _lineItemCommand.UpdateLineItemStatusesAndNotifyIfApplicable(OrderDirection.Outgoing, orderID, lineItemStatusChanges, UserContext);
        }

        [DocName("Apply Automatic Promtions to order and remove promotions no longer valid on order")]
        [HttpPost, Route("{orderID}/applypromotions")]
        public async Task<HSOrder> ApplyAutomaticPromotions(string orderID)
        {
            return await _command.ApplyAutomaticPromotions(orderID);
        }
    }
}
