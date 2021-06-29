using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrderCloud.SDK;
using Headstart.Models;
using Headstart.Models.Misc;
using ordercloud.integrations.library;
using Headstart.Models.Extended;
using Headstart.Common.Models;
using Headstart.Common.Services.ShippingIntegration.Models;
using OrderCloud.Catalyst;

namespace Headstart.API.Commands
{
    public interface IOrderCommand
    {
        Task<Order> AcknowledgeQuoteOrder(string orderID);
        Task<ListPage<HSOrder>> ListOrdersForLocation(string locationID, ListArgs<HSOrder> listArgs, VerifiedUserContext verifiedUser);
        Task<OrderDetails> GetOrderDetails(string orderID, VerifiedUserContext verifiedUser);
        Task<List<HSShipmentWithItems>> ListHSShipmentWithItems(string orderID, VerifiedUserContext verifiedUser);
        Task<CosmosListPage<RMA>> ListRMAsForOrder(string orderID, VerifiedUserContext verifiedUser);
        Task<HSOrder> AddPromotion(string orderID, string promoCode, VerifiedUserContext verifiedUser);
        Task<HSOrder> ApplyAutomaticPromotions(string orderID);
        Task PatchOrderRequiresApprovalStatus(string orderID);
    }

    public class OrderCommand : IOrderCommand
    {
        private readonly IOrderCloudClient _oc;
        private readonly ILocationPermissionCommand _locationPermissionCommand;
        private readonly IPromotionCommand _promotionCommand;
        private readonly IRMACommand _rmaCommand;

        public OrderCommand(
            ILocationPermissionCommand locationPermissionCommand,
            IOrderCloudClient oc,
            IPromotionCommand promotionCommand,
            IRMACommand rmaCommand
            )
        {
			_oc = oc;
            _locationPermissionCommand = locationPermissionCommand;
            _promotionCommand = promotionCommand;
            _rmaCommand = rmaCommand;
		}

        public async Task<Order> AcknowledgeQuoteOrder(string orderID)
        {
            var orderPatch = new PartialOrder()
            {
                xp = new
                {
                    SubmittedOrderStatus = SubmittedOrderStatus.Completed
                }
            };
            //  Need to complete sales and purchase order and patch the xp.SubmittedStatus of both orders            
            var salesOrderID = orderID.Split('-')[0];
            var completeSalesOrder = _oc.Orders.CompleteAsync(OrderDirection.Incoming, salesOrderID);
            var patchSalesOrder = _oc.Orders.PatchAsync<HSOrder>(OrderDirection.Incoming, salesOrderID, orderPatch);
            var completedSalesOrder = await completeSalesOrder;
            var patchedSalesOrder = await patchSalesOrder;

            var purchaseOrderID = $"{salesOrderID}-{patchedSalesOrder?.xp?.SupplierIDs?.FirstOrDefault()}";
            var completePurchaseOrder = _oc.Orders.CompleteAsync(OrderDirection.Outgoing, purchaseOrderID);
            var patchPurchaseOrder = _oc.Orders.PatchAsync(OrderDirection.Outgoing, purchaseOrderID, orderPatch);
            var completedPurchaseOrder = await completePurchaseOrder;
            var patchedPurchaseOrder = await patchPurchaseOrder;

            return orderID == salesOrderID ? patchedSalesOrder : patchedPurchaseOrder;
        }
       
        public async Task PatchOrderRequiresApprovalStatus(string orderID)
        {
                await PatchOrderStatus(orderID, ShippingStatus.Processing, ClaimStatus.NoClaim);
        }

        private async Task PatchOrderStatus(string orderID, ShippingStatus shippingStatus, ClaimStatus claimStatus)
        {
            var partialOrder = new PartialOrder { xp = new { ShippingStatus = shippingStatus, ClaimStatus = claimStatus } };
            await _oc.Orders.PatchAsync(OrderDirection.Incoming, orderID, partialOrder);
        }

        public async Task<ListPage<HSOrder>> ListOrdersForLocation(string locationID, ListArgs<HSOrder> listArgs, VerifiedUserContext verifiedUser)
        {
            listArgs.Filters.Add(new ListFilter("BillingAddress.ID", locationID));
            await EnsureUserCanAccessLocationOrders(locationID, verifiedUser);
            return await _oc.Orders.ListAsync<HSOrder>(OrderDirection.Incoming,
                page: listArgs.Page,
                pageSize: listArgs.PageSize,
                search: listArgs.Search,
                sortBy: listArgs.SortBy.FirstOrDefault(),
                filters: listArgs.ToFilterString());
        }

        public async Task<OrderDetails> GetOrderDetails(string orderID, VerifiedUserContext verifiedUser)
        {
            var order = await _oc.Orders.GetAsync<HSOrder>(OrderDirection.Incoming, orderID);
            await EnsureUserCanAccessOrder(order, verifiedUser);

            var lineItems = _oc.LineItems.ListAllAsync(OrderDirection.Incoming, orderID);
            var promotions = _oc.Orders.ListAllPromotionsAsync(OrderDirection.Incoming, orderID);
            var payments = _oc.Payments.ListAllAsync(OrderDirection.Incoming, order.ID);
            var approvals = _oc.Orders.ListAllApprovalsAsync(OrderDirection.Incoming, orderID);
            return new OrderDetails
            {
                Order = order,
                LineItems = await lineItems,
                Promotions = await promotions,
                Payments = await payments,
                Approvals = await approvals
            };
        }

        public async Task<List<HSShipmentWithItems>> ListHSShipmentWithItems(string orderID, VerifiedUserContext verifiedUser)
        {
            var order = await _oc.Orders.GetAsync<HSOrder>(OrderDirection.Incoming, orderID);
            await EnsureUserCanAccessOrder(order, verifiedUser);

            var lineItems = await _oc.LineItems.ListAllAsync(OrderDirection.Incoming, orderID);
            var shipments = await _oc.Orders.ListShipmentsAsync<HSShipmentWithItems>(OrderDirection.Incoming, orderID);
            var shipmentsWithItems = await Throttler.RunAsync(shipments.Items, 100, 5, (HSShipmentWithItems shipment) => GetShipmentWithItems(shipment, lineItems.ToList()));
            return shipmentsWithItems.ToList();
        }

        public async Task<CosmosListPage<RMA>> ListRMAsForOrder(string orderID, VerifiedUserContext verifiedUser)
        {
            HSOrder order = await _oc.Orders.GetAsync<HSOrder>(OrderDirection.Incoming, orderID);
            await EnsureUserCanAccessOrder(order, verifiedUser);

            var listFilter = new ListFilter("SourceOrderID", orderID);
            CosmosListOptions listOptions = new CosmosListOptions() { PageSize = 100, ContinuationToken = null, Filters = { listFilter } };

            CosmosListPage<RMA> rmasOnOrder = await _rmaCommand.ListBuyerRMAs(listOptions, verifiedUser.Buyer.ID);
            return rmasOnOrder;
        }

        private async Task<HSShipmentWithItems> GetShipmentWithItems(HSShipmentWithItems shipment, List<LineItem> lineItems)
        {
            var shipmentItems = await _oc.Shipments.ListItemsAsync<HSShipmentItemWithLineItem>(shipment.ID);
            shipment.ShipmentItems = shipmentItems.Items.Select(shipmentItem =>
            {
                shipmentItem.LineItem = lineItems.First(li => li.ID == shipmentItem.LineItemID);
                return shipmentItem;
            }).ToList();
            return shipment;
        }

        public async Task<HSOrder> AddPromotion(string orderID, string promoCode, VerifiedUserContext verifiedUser)
        {
            var orderPromo = await _oc.Orders.AddPromotionAsync(OrderDirection.Incoming, orderID, promoCode);
            return await _oc.Orders.GetAsync<HSOrder>(OrderDirection.Incoming, orderID);
        }

        public async Task<HSOrder> ApplyAutomaticPromotions(string orderID)
        {
            await _promotionCommand.AutoApplyPromotions(orderID);
            return await _oc.Orders.GetAsync<HSOrder>(OrderDirection.Incoming, orderID);
        }

        private async Task EnsureUserCanAccessLocationOrders(string locationID, VerifiedUserContext verifiedUser, string overrideErrorMessage = "")
        {
            var hasAccess = await _locationPermissionCommand.IsUserInAccessGroup(locationID, UserGroupSuffix.ViewAllOrders.ToString(), verifiedUser);
            Require.That(hasAccess, new ErrorCode("Insufficient Access", 403, $"User cannot access orders from this location: {locationID}"));
        }

        private async Task EnsureUserCanAccessOrder(HSOrder order, VerifiedUserContext verifiedUser)
        {
            /* ensures user has access to order through at least 1 of 3 methods
             * 1) user submitted the order
             * 2) user has access to all orders from the location of the billingAddressID 
             * 3) the order is awaiting approval and the user is in the approving group 
             */ 

            var isOrderSubmitter = order.FromUserID == verifiedUser.ID;
            if (isOrderSubmitter)
            {
                return;
            }
            
            var isUserInLocationOrderAccessGroup = await _locationPermissionCommand.IsUserInAccessGroup(order.BillingAddressID, UserGroupSuffix.ViewAllOrders.ToString(), verifiedUser);
            if (isUserInLocationOrderAccessGroup)
            {
                return;
            } 
            
            if(order.Status == OrderStatus.AwaitingApproval)
            {
                // logic assumes there is only one approving group per location
                var isUserInApprovalGroup = await _locationPermissionCommand.IsUserInAccessGroup(order.BillingAddressID, UserGroupSuffix.OrderApprover.ToString(), verifiedUser);
                if(isUserInApprovalGroup)
                {
                    return;
                }
            }

            // if function has not been exited yet we throw an insufficient access error
            Require.That(false, new ErrorCode("Insufficient Access", 403, $"User cannot access order {order.ID}"));
        }
    };
}