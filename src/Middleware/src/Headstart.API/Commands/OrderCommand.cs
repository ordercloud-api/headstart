using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Headstart.Common.Commands;
using Headstart.Common.Models;
using Headstart.Common.Services;
using OrderCloud.Catalyst;
using OrderCloud.Integrations.CosmosDB;
using OrderCloud.Integrations.Emails;
using OrderCloud.SDK;

namespace Headstart.API.Commands
{
    public interface IOrderCommand
    {
        Task<Order> AcknowledgeQuoteOrder(string orderID);

        Task<ListPage<HSOrder>> ListOrdersForLocation(string locationID, ListArgs<HSOrder> listArgs, DecodedToken decodedToken);

        Task<OrderDetails> GetOrderDetails(string orderID, DecodedToken decodedToken);

        Task<List<HSShipmentWithItems>> ListHSShipmentWithItems(string orderID, DecodedToken decodedToken);

        Task<HSOrder> AddPromotion(string orderID, string promoCode, DecodedToken decodedToken);

        Task<HSOrder> ApplyAutomaticPromotions(string orderID);

        Task PatchOrderRequiresApprovalStatus(string orderID);

        Task<HSLineItem> SendQuoteRequestToSupplier(string orderID, string lineItemID);

        Task<HSLineItem> OverrideQuotePrice(string orderID, string lineItemID, decimal quotePrice);

        Task<ListPage<HSOrder>> ListQuoteOrders(MeUser me, QuoteStatus quoteStatus);

        Task<HSOrder> GetQuoteOrder(MeUser me, string orderID);

        Task<HSOrder> CancelOrder(string orderID, DecodedToken decodedToken);
    }

    public class OrderCommand : IOrderCommand
    {
        private readonly IOrderCloudClient oc;
        private readonly ILocationPermissionCommand locationPermissionCommand;
        private readonly IPromotionCommand promotionCommand;
        private readonly AppSettings settings;
        private readonly IEmailServiceProvider emailServiceProvider;
        private readonly IHSCreditCardProcessor creditCardService;

        public OrderCommand(
            ILocationPermissionCommand locationPermissionCommand,
            IOrderCloudClient oc,
            IPromotionCommand promotionCommand,
            AppSettings settings,
            IEmailServiceProvider sendgridService,
            IHSCreditCardProcessor creditCardService)
        {
            this.oc = oc;
            this.locationPermissionCommand = locationPermissionCommand;
            this.promotionCommand = promotionCommand;
            this.settings = settings;
            this.emailServiceProvider = sendgridService;
            this.creditCardService = creditCardService;
        }

        public async Task<HSLineItem> SendQuoteRequestToSupplier(string orderID, string lineItemID)
        {
            var lineItem = await oc.LineItems.GetAsync<HSLineItem>(OrderDirection.All, orderID, lineItemID);
            var orderObject = await oc.Orders.GetAsync<HSOrder>(OrderDirection.All, orderID);

            // SEND EMAIL NOTIFICATION TO BUYER
            await emailServiceProvider.SendQuoteRequestConfirmationEmail(orderObject, lineItem, orderObject.xp?.QuoteBuyerContactEmail);
            return lineItem;
        }

        public async Task<HSLineItem> OverrideQuotePrice(string orderID, string lineItemID, decimal quotePrice)
        {
            var linePatch = new PartialLineItem { UnitPrice = quotePrice };
            var updatedLineItem = await oc.LineItems.PatchAsync<HSLineItem>(OrderDirection.All, orderID, lineItemID, linePatch);
            var orderPatch = new PartialOrder { xp = new { QuoteStatus = QuoteStatus.NeedsBuyerReview } };
            var updatedOrder = await oc.Orders.PatchAsync<HSOrder>(OrderDirection.All, orderID, orderPatch);

            // SEND EMAIL NOTIFICATION TO BUYER
            await emailServiceProvider.SendQuotePriceConfirmationEmail(updatedOrder, updatedLineItem, updatedOrder.xp?.QuoteBuyerContactEmail);
            return updatedLineItem;
        }

        public async Task<ListPage<HSOrder>> ListQuoteOrders(MeUser me, QuoteStatus quoteStatus)
        {
            var supplierID = me.Supplier?.ID;
            var filters = new Dictionary<string, object>
            {
                ["xp.QuoteSupplierID"] = supplierID != null ? supplierID : "*",
                ["IsSubmitted"] = false,
                ["xp.OrderType"] = OrderType.Quote,
                ["xp.QuoteStatus"] = quoteStatus,
            };
            var quoteOrders = await oc.Orders.ListAllAsync<HSOrder>(OrderDirection.Incoming, filters: filters);
            var quoteOrdersList = new ListPage<HSOrder>()
            {
                Meta = new ListPageMeta()
                {
                    Page = 1,
                    PageSize = 1,
                    TotalCount = quoteOrders.Count,
                    TotalPages = 1,
                    ItemRange = new[] { 1, quoteOrders.Count },
                },
                Items = quoteOrders,
            };
            return quoteOrdersList;
        }

        public async Task<HSOrder> GetQuoteOrder(MeUser me, string orderID)
        {
            var supplierID = me.Supplier?.ID;
            var order = await oc.Orders.GetAsync<HSOrder>(OrderDirection.Incoming, orderID);
            if (supplierID != null && order.xp?.QuoteSupplierID != supplierID)
            {
                throw new Exception("You are not authorized to view this order.");
            }

            return order;
        }

        public async Task<Order> AcknowledgeQuoteOrder(string orderID)
        {
            var orderPatch = new PartialOrder()
            {
                xp = new
                {
                    SubmittedOrderStatus = SubmittedOrderStatus.Completed,
                },
            };

            // Need to complete sales and purchase order and patch the xp.SubmittedStatus of both orders
            var salesOrderID = orderID.Split('-')[0];
            var completeSalesOrder = oc.Orders.CompleteAsync(OrderDirection.Incoming, salesOrderID);
            var patchSalesOrder = oc.Orders.PatchAsync<HSOrder>(OrderDirection.Incoming, salesOrderID, orderPatch);
            var completedSalesOrder = await completeSalesOrder;
            var patchedSalesOrder = await patchSalesOrder;

            var purchaseOrderID = $"{salesOrderID}-{patchedSalesOrder?.xp?.SupplierIDs?.FirstOrDefault()}";
            var completePurchaseOrder = oc.Orders.CompleteAsync(OrderDirection.Outgoing, purchaseOrderID);
            var patchPurchaseOrder = oc.Orders.PatchAsync(OrderDirection.Outgoing, purchaseOrderID, orderPatch);
            var completedPurchaseOrder = await completePurchaseOrder;
            var patchedPurchaseOrder = await patchPurchaseOrder;

            return orderID == salesOrderID ? patchedSalesOrder : patchedPurchaseOrder;
        }

        public async Task PatchOrderRequiresApprovalStatus(string orderID)
        {
                await PatchOrderStatus(orderID, ShippingStatus.Processing);
        }

        public async Task<ListPage<HSOrder>> ListOrdersForLocation(string locationID, ListArgs<HSOrder> listArgs, DecodedToken decodedToken)
        {
            listArgs.Filters.Add(new ListFilter("BillingAddress.ID", locationID));
            await EnsureUserCanAccessLocationOrders(locationID, decodedToken);
            return await oc.Orders.ListAsync<HSOrder>(
                OrderDirection.Incoming,
                page: listArgs.Page,
                pageSize: listArgs.PageSize,
                search: listArgs.Search,
                sortBy: listArgs.SortBy.FirstOrDefault(),
                filters: listArgs.ToFilterString());
        }

        public async Task<OrderDetails> GetOrderDetails(string orderID, DecodedToken decodedToken)
        {
            var order = await oc.Orders.GetAsync<HSOrder>(OrderDirection.Incoming, orderID);
            await EnsureUserCanAccessOrder(order, decodedToken);

            var payments = oc.Payments.ListAllAsync(OrderDirection.Incoming, order.ID);
            var orderReturns = oc.OrderReturns.ListAllAsync(filters: new { OrderID = order.ID });
            var lineItems = oc.LineItems.ListAllAsync(OrderDirection.Incoming, orderID);
            var promotions = oc.Orders.ListAllPromotionsAsync(OrderDirection.Incoming, orderID);
            var approvals = oc.Orders.ListAllApprovalsAsync(OrderDirection.Incoming, orderID);

            await Task.WhenAll(payments, orderReturns, lineItems, promotions, approvals);

            // catalyst doesn't sort entities by DateCreated due to this issue  https://github.com/ordercloud-api/ordercloud-dotnet-catalyst/issues/117
            // so we need to manually sort after retrieving results. If this issue gets fixed then remove these sort functions
            var paymentsSorted = await payments;
            paymentsSorted.Sort((a, b) => a.DateCreated.CompareTo(b.DateCreated));

            var orderReturnsSorted = await orderReturns;
            orderReturnsSorted.Sort((a, b) => Nullable.Compare(a.DateCreated, b.DateCreated));

            var lineItemsSorted = await lineItems;
            lineItemsSorted.Sort((a, b) => a.DateAdded.CompareTo(b.DateAdded));

            var approvalsSorted = await approvals;
            approvalsSorted.Sort((a, b) => a.DateCreated.CompareTo(b.DateCreated));

            return new OrderDetails
            {
                Order = order,
                Payments = paymentsSorted,
                OrderReturns = orderReturnsSorted,
                LineItems = lineItemsSorted,
                Promotions = await promotions,
                Approvals = approvalsSorted,
            };
        }

        public async Task<List<HSShipmentWithItems>> ListHSShipmentWithItems(string orderID, DecodedToken decodedToken)
        {
            var order = await oc.Orders.GetAsync<HSOrder>(OrderDirection.Incoming, orderID);
            await EnsureUserCanAccessOrder(order, decodedToken);

            var lineItems = await oc.LineItems.ListAllAsync(OrderDirection.Incoming, orderID);
            var shipments = await oc.Orders.ListShipmentsAsync<HSShipmentWithItems>(OrderDirection.Incoming, orderID);
            var shipmentsWithItems = await Throttler.RunAsync(shipments.Items, 100, 5, (HSShipmentWithItems shipment) => GetShipmentWithItems(shipment, lineItems.ToList()));
            return shipmentsWithItems.ToList();
        }

        public async Task<HSOrder> AddPromotion(string orderID, string promoCode, DecodedToken decodedToken)
        {
            await oc.Orders.AddPromotionAsync(OrderDirection.Incoming, orderID, promoCode);
            return await oc.Orders.GetAsync<HSOrder>(OrderDirection.Incoming, orderID);
        }

        public async Task<HSOrder> ApplyAutomaticPromotions(string orderID)
        {
            await promotionCommand.AutoApplyPromotions(orderID);
            return await oc.Orders.GetAsync<HSOrder>(OrderDirection.Incoming, orderID);
        }

        public async Task<HSOrder> CancelOrder(string orderID, DecodedToken decodedToken)
        {
            var order = await oc.Orders.GetAsync<HSOrder>(OrderDirection.Incoming, orderID);
            await EnsureUserCanAccessOrder(order, decodedToken);

            if (order.Status != OrderStatus.Open)
            {
                throw new Exception("Can only cancel open orders");
            }

            if (order.xp.OrderType == OrderType.Quote)
            {
                throw new Exception("Can not cancel quote orders");
            }

            var lineItems = await oc.LineItems.ListAllAsync(OrderDirection.All, orderID);
            if (lineItems.Any(li => li.QuantityShipped > 0))
            {
                // In headstart we are only allowing a wholesale cancel of the entire order see more info here
                throw new Exception("Can not cancel an order that has already shipped");
            }

            // get payment to refund, there should only be one payment on the order in headstart
            var paymentList = await oc.Payments.ListAsync<HSPayment>(OrderDirection.All, orderID);
            var payment = paymentList.Items.FirstOrDefault();

            if (payment.Type == PaymentType.CreditCard)
            {
                var creditCardPaymentTransaction = payment.Transactions
                    .OrderBy(x => x.DateExecuted)
                    .LastOrDefault(x => x.Type == "CreditCard" && x.Succeeded);

                // make inquiry to determine the current capture capture state
                var inquiryResult = await creditCardService.Inquire(order, creditCardPaymentTransaction);

                // Transactions that are queued for capture can only be fully voided, and we are only allowing partial voids moving forward.
                if (inquiryResult.PendingCapture)
                {
                    throw new CatalystBaseException(new ApiError
                    {
                        ErrorCode = "Payment.FailedToVoidAuthorization",
                        Message = "This customer's credit card transaction is currently queued for capture and cannot be refunded at this time.  Please try again later.",
                    });
                }

                // If voidable, but not refundable, void the refund amount off the original order total
                if (inquiryResult.CanVoid)
                {
                    await creditCardService.VoidAuthorization(order, payment, creditCardPaymentTransaction, order.Total);
                }

                // If refundable, but not voidable, do a refund
                if (inquiryResult.CanRefund)
                {
                    await creditCardService.Refund(order, payment, creditCardPaymentTransaction, order.Total);
                }
            }
            else
            {
                await oc.Payments.CreateTransactionAsync(OrderDirection.All, orderID, payment.ID, new HSPaymentTransaction
                {
                    Amount = order.Total * -1,
                    Succeeded = true,
                    Type = "Refund",
                });
            }

            await oc.Orders.CancelAsync<HSOrder>(OrderDirection.Incoming, orderID);
            return await oc.Orders.PatchAsync<HSOrder>(OrderDirection.Incoming, orderID, new PartialOrder { xp = new { SubmittedOrderStatus = "Canceled" } });
        }

        private async Task PatchOrderStatus(string orderID, ShippingStatus shippingStatus)
        {
            var partialOrder = new PartialOrder { xp = new { ShippingStatus = shippingStatus } };
            await oc.Orders.PatchAsync(OrderDirection.Incoming, orderID, partialOrder);
        }

        private async Task<HSShipmentWithItems> GetShipmentWithItems(HSShipmentWithItems shipment, List<LineItem> lineItems)
        {
            var shipmentItems = await oc.Shipments.ListItemsAsync<HSShipmentItemWithLineItem>(shipment.ID);
            shipment.ShipmentItems = shipmentItems.Items.Select(shipmentItem =>
            {
                shipmentItem.LineItem = lineItems.First(li => li.ID == shipmentItem.LineItemID);
                return shipmentItem;
            }).ToList();
            return shipment;
        }

        private async Task EnsureUserCanAccessLocationOrders(string locationID, DecodedToken decodedToken, string overrideErrorMessage = "")
        {
            var hasAccess = await locationPermissionCommand.IsUserInAccessGroup(locationID, UserGroupSuffix.ViewAllOrders.ToString(), decodedToken);
            Require.That(hasAccess, new ErrorCode("Insufficient Access", $"User cannot access orders from this location: {locationID}", HttpStatusCode.Forbidden));
        }

        private async Task EnsureUserCanAccessOrder(HSOrder order, DecodedToken decodedToken)
        {
            /* ensures user has access to order through at least 1 of 3 methods
             * 1) user submitted the order
             * 2) user has access to all orders from the location of the billingAddressID
             * 3) the order is awaiting approval and the user is in the approving group
             */

            var isOrderSubmitter = order.FromUser.Username == decodedToken.Username;
            if (isOrderSubmitter)
            {
                return;
            }

            var isUserInLocationOrderAccessGroup = await locationPermissionCommand.IsUserInAccessGroup(order.BillingAddressID, UserGroupSuffix.ViewAllOrders.ToString(), decodedToken);
            if (isUserInLocationOrderAccessGroup)
            {
                return;
            }

            if (order.Status == OrderStatus.AwaitingApproval)
            {
                // logic assumes there is only one approving group per location
                var isUserInApprovalGroup = await locationPermissionCommand.IsUserInAccessGroup(order.BillingAddressID, UserGroupSuffix.OrderApprover.ToString(), decodedToken);
                if (isUserInApprovalGroup)
                {
                    return;
                }
            }

            // if function has not been exited yet we throw an insufficient access error
            Require.That(false, new ErrorCode("Insufficient Access", $"User cannot access order {order.ID}", HttpStatusCode.Forbidden));
        }
    }
}
