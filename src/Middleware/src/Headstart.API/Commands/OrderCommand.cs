using System;
using System.Net;
using System.Linq;
using OrderCloud.SDK;
using Headstart.Common;
using OrderCloud.Catalyst;
using Sitecore.Diagnostics;
using System.Threading.Tasks;
using Headstart.Common.Services;
using System.Collections.Generic;
using ordercloud.integrations.library;
using Headstart.Common.Models.Headstart;
using Headstart.Common.Repositories.Models;
using Headstart.Common.Models.Headstart.Extended;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Foundation.SitecoreExtensions.MVC.Extensions;

namespace Headstart.API.Commands
{
	public interface IOrderCommand
	{
		Task<Order> AcknowledgeQuoteOrder(string orderId);
		Task<ListPage<HsOrder>> ListOrdersForLocation(string locationId, ListArgs<HsOrder> listArgs, DecodedToken decodedToken);
		Task<OrderDetails> GetOrderDetails(string orderId, DecodedToken decodedToken);
		Task<List<HsShipmentWithItems>> ListHsShipmentWithItems(string orderId, DecodedToken decodedToken);
		Task<CosmosListPage<RMA>> ListRMAsForOrder(string orderId, DecodedToken decodedToken);
		Task<HsOrder> AddPromotion(string orderId, string promoCode, DecodedToken decodedToken);
		Task<HsOrder> ApplyAutomaticPromotions(string orderId);
		Task PatchOrderRequiresApprovalStatus(string orderId);
		Task<HsLineItem> SendQuoteRequestToSupplier(string orderId, string lineItemId);
		Task<HsLineItem> OverrideQuotePrice(string orderId, string lineItemId, decimal quotePrice);
		Task<ListPage<HsOrder>> ListQuoteOrders(MeUser me, QuoteStatus quoteStatus);
		Task<HsOrder> GetQuoteOrder(MeUser me, string orderId);
	}

	public class OrderCommand : IOrderCommand
	{
		private readonly IOrderCloudClient _oc;
		private readonly ILocationPermissionCommand _locationPermissionCommand;
		private readonly IPromotionCommand _promotionCommand;
		private readonly IRMACommand _rmaCommand;
		private readonly AppSettings _settings;
		private readonly ISendgridService _sendgridService;
		private readonly ConfigSettings _configSettings = ConfigSettings.Instance;

		/// <summary>
		/// The IOC based constructor method for the OrderCommand class object with Dependency Injection
		/// </summary>
		/// <param name="locationPermissionCommand"></param>
		/// <param name="oc"></param>
		/// <param name="promotionCommand"></param>
		/// <param name="rmaCommand"></param>
		/// <param name="settings"></param>
		/// <param name="sendgridService"></param>
		public OrderCommand(ILocationPermissionCommand locationPermissionCommand, IOrderCloudClient oc, IPromotionCommand promotionCommand, IRMACommand rmaCommand, AppSettings settings, ISendgridService sendgridService)
		{
			try
			{
				_oc = oc;
				_locationPermissionCommand = locationPermissionCommand;
				_promotionCommand = promotionCommand;
				_rmaCommand = rmaCommand;
				_settings = settings;
				_sendgridService = sendgridService;
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Public re-usable SendQuoteRequestToSupplier task method
		/// </summary>
		/// <param name="orderId"></param>
		/// <param name="lineItemId"></param>
		/// <returns>The HsLineItem response object from the SendQuoteRequestToSupplier process</returns>
		public async Task<HsLineItem> SendQuoteRequestToSupplier(string orderId, string lineItemId)
		{
			var lineItem = new HsLineItem();
			try
			{
				lineItem = await _oc.LineItems.GetAsync<HsLineItem>(OrderDirection.All, orderId, lineItemId);
				var orderObject = await _oc.Orders.GetAsync<HsOrder>(OrderDirection.All, orderId);
				// SEND EMAIL NOTIFICATION TO BUYER
				await _sendgridService.SendQuoteRequestConfirmationEmail(orderObject, lineItem, orderObject.xp?.QuoteBuyerContactEmail);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return lineItem;
		}

		/// <summary>
		/// Public re-usable OverrideQuotePrice task method
		/// </summary>
		/// <param name="orderId"></param>
		/// <param name="lineItemId"></param>
		/// <param name="quotePrice"></param>
		/// <returns>The HsLineItem response object from the OverrideQuotePrice process</returns>
		public async Task<HsLineItem> OverrideQuotePrice(string orderId, string lineItemId, decimal quotePrice)
		{
			var updatedLineItem = new HsLineItem();
			try
			{
				var linePatch = new PartialLineItem { UnitPrice = quotePrice };
				updatedLineItem = await _oc.LineItems.PatchAsync<HsLineItem>(OrderDirection.All, orderId, lineItemId, linePatch);
				var orderPatch = new PartialOrder { xp = new { QuoteStatus = QuoteStatus.NeedsBuyerReview } };
				var updatedOrder = await _oc.Orders.PatchAsync<HsOrder>(OrderDirection.All, orderId, orderPatch);
				// SEND EMAIL NOTIFICATION TO BUYER
				await _sendgridService.SendQuotePriceConfirmationEmail(updatedOrder, updatedLineItem, updatedOrder.xp?.QuoteBuyerContactEmail);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return updatedLineItem;
		}

		/// <summary>
		/// Public re-usable ListQuoteOrders task method
		/// </summary>
		/// <param name="me"></param>
		/// <param name="quoteStatus"></param>
		/// <returns>The ListPage of HsOrder response objects from the ListQuoteOrders process</returns>
		public async Task<ListPage<HsOrder>> ListQuoteOrders(MeUser me, QuoteStatus quoteStatus)
		{
			var quoteOrdersList = new ListPage<HsOrder>();
			try
			{
				var supplierId = me.Supplier?.ID;
				var filters = new Dictionary<string, object>
				{
					[@"xp.QuoteSupplierID"] = !string.IsNullOrEmpty(supplierId) ? supplierId : @"*",
					[@"IsSubmitted"] = false,
					[@"xp.OrderType"] = OrderType.Quote,
					[@"xp.QuoteStatus"] = quoteStatus
				};
				var quoteOrders = await _oc.Orders.ListAllAsync<HsOrder>(OrderDirection.Incoming, filters: filters);
				quoteOrdersList = new ListPage<HsOrder>()
				{
					Meta = new ListPageMeta()
					{
						Page = 1,
						PageSize = 1,
						TotalCount = quoteOrders.Count,
						TotalPages = 1,
						ItemRange = new[] { 1, quoteOrders.Count }
					},
					Items = quoteOrders
				};
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return quoteOrdersList;
		}

		/// <summary>
		/// Public re-usable GetQuoteOrder task method
		/// </summary>
		/// <param name="me"></param>
		/// <param name="orderId"></param>
		/// <returns>The HsOrder response objects from the GetQuoteOrder process</returns>
		public async Task<HsOrder> GetQuoteOrder(MeUser me, string orderId)
		{
			var order = new HsOrder();
			try
			{
				var supplierId = me.Supplier?.ID;
				order = await _oc.Orders.GetAsync<HsOrder>(OrderDirection.Incoming, orderId);
				if (!string.IsNullOrEmpty(supplierId) && order.xp?.QuoteSupplierId != supplierId)
				{
					throw new Exception("You are not authorized to view this order.");
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return order;
		}

		/// <summary>
		/// Public re-usable AcknowledgeQuoteOrder task method
		/// </summary>
		/// <param name="orderId"></param>
		/// <returns>The Order response object from the AcknowledgeQuoteOrder process</returns>
		public async Task<Order> AcknowledgeQuoteOrder(string orderId)
		{
			var resp = new Order();
			try
			{
				var orderPatch = new PartialOrder()
				{
					xp = new
					{
						SubmittedOrderStatus = SubmittedOrderStatus.Completed
					}
				};
				//  Need to complete sales and purchase order and patch the xp.SubmittedStatus of both orders            
				var salesorderId = orderId.Split('-')[0];
				var completeSalesOrder = _oc.Orders.CompleteAsync(OrderDirection.Incoming, salesorderId);
				var patchSalesOrder = _oc.Orders.PatchAsync<HsOrder>(OrderDirection.Incoming, salesorderId, orderPatch);
				var completedSalesOrder = await completeSalesOrder;
				var patchedSalesOrder = await patchSalesOrder;

				var purchaseorderId = $"{salesorderId}-{patchedSalesOrder?.xp?.SupplierIds?.FirstOrDefault()}";
				var completePurchaseOrder = _oc.Orders.CompleteAsync(OrderDirection.Outgoing, purchaseorderId);
				var patchPurchaseOrder = _oc.Orders.PatchAsync(OrderDirection.Outgoing, purchaseorderId, orderPatch);
				var completedPurchaseOrder = await completePurchaseOrder;
				var patchedPurchaseOrder = await patchPurchaseOrder;
				resp = (orderId == salesorderId) ? patchedSalesOrder : patchedPurchaseOrder;
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Public re-usable PatchOrderRequiresApprovalStatus task method
		/// </summary>
		/// <param name="orderId"></param>
		/// <returns></returns>
		public async Task PatchOrderRequiresApprovalStatus(string orderId)
		{
			try
			{
				await PatchOrderStatus(orderId, ShippingStatus.Processing, ClaimStatus.NoClaim);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Private re-usable PatchOrderStatus task method
		/// </summary>
		/// <param name="orderId"></param>
		/// <param name="shippingStatus"></param>
		/// <param name="claimStatus"></param>
		/// <returns></returns>
		private async Task PatchOrderStatus(string orderId, ShippingStatus shippingStatus, ClaimStatus claimStatus)
		{
			try
			{
				var partialOrder = new PartialOrder { xp = new { ShippingStatus = shippingStatus, ClaimStatus = claimStatus } };
				await _oc.Orders.PatchAsync(OrderDirection.Incoming, orderId, partialOrder);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Public re-usable ListOrdersForLocation task method
		/// </summary>
		/// <param name="locationId"></param>
		/// <param name="listArgs"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The ListPage of HsOrder response objects from the ListOrdersForLocation process</returns>
		public async Task<ListPage<HsOrder>> ListOrdersForLocation(string locationId, ListArgs<HsOrder> listArgs, DecodedToken decodedToken)
		{
			var resp = new ListPage<HsOrder>();
			try
			{
				listArgs.Filters.Add(new ListFilter("BillingAddress.ID", locationId));
				await EnsureUserCanAccessLocationOrders(locationId, decodedToken);
				resp = await _oc.Orders.ListAsync<HsOrder>(OrderDirection.Incoming, page: listArgs.Page, pageSize: listArgs.PageSize, search: listArgs.Search,
					sortBy: listArgs.SortBy.FirstOrDefault(), filters: listArgs.ToFilterString());
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Public re-usable GetOrderDetails task method
		/// </summary>
		/// <param name="orderId"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The OrderDetails response object from the GetOrderDetails process</returns>
		public async Task<OrderDetails> GetOrderDetails(string orderId, DecodedToken decodedToken)
		{
			var resp = new OrderDetails();
			try
			{
				var order = await _oc.Orders.GetAsync<HsOrder>(OrderDirection.Incoming, orderId);
				await EnsureUserCanAccessOrder(order, decodedToken);

				var lineItems = _oc.LineItems.ListAllAsync(OrderDirection.Incoming, orderId);
				var promotions = _oc.Orders.ListAllPromotionsAsync(OrderDirection.Incoming, orderId);
				var payments = _oc.Payments.ListAllAsync(OrderDirection.Incoming, order.ID);
				// bug in catalyst tries to list all by ID but ID doesn't exist on approval rules
				// https://github.com/ordercloud-api/ordercloud-dotnet-catalyst/issues/33
				// var approvals = _oc.Orders.ListAllApprovalsAsync(OrderDirection.Incoming, orderId);
				var approvals = await _oc.Orders.ListApprovalsAsync(OrderDirection.Incoming, orderId, pageSize: 100);
				resp = new OrderDetails
				{
					Order = order,
					LineItems = await lineItems,
					Promotions = await promotions,
					Payments = await payments,
					Approvals = approvals.Items
				};
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Public re-usable ListHsShipmentWithItems task method
		/// </summary>
		/// <param name="orderId"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The list of HsShipmentWithItems response object from the ListHsShipmentWithItems process</returns>
		public async Task<List<HsShipmentWithItems>> ListHsShipmentWithItems(string orderId, DecodedToken decodedToken)
		{
			var resp = new List<HsShipmentWithItems>();
			try
			{
				var order = await _oc.Orders.GetAsync<HsOrder>(OrderDirection.Incoming, orderId);
				await EnsureUserCanAccessOrder(order, decodedToken);

				var lineItems = await _oc.LineItems.ListAllAsync(OrderDirection.Incoming, orderId);
				var shipments = await _oc.Orders.ListShipmentsAsync<HsShipmentWithItems>(OrderDirection.Incoming, orderId);
				var shipmentsWithItems = await Throttler.RunAsync(shipments.Items, 100, 5, (HsShipmentWithItems shipment) => GetShipmentWithItems(shipment, lineItems.ToList()));
				resp = shipmentsWithItems.ToList();
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Public re-usable ListRMAsForOrder task method
		/// </summary>
		/// <param name="orderId"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The CosmosListPage of RMA response object from the ListRMAsForOrder process</returns>
		public async Task<CosmosListPage<RMA>> ListRMAsForOrder(string orderId, DecodedToken decodedToken)
		{
			var rmasOnOrder = new CosmosListPage<RMA>();
			try
			{
				var me = await _oc.Me.GetAsync(accessToken: decodedToken.AccessToken);
				HsOrder order = await _oc.Orders.GetAsync<HsOrder>(OrderDirection.Incoming, orderId);
				await EnsureUserCanAccessOrder(order, decodedToken);

				var listFilter = new ListFilter("SourceorderId", orderId);
				CosmosListOptions listOptions = new CosmosListOptions() { PageSize = 100, ContinuationToken = null, Filters = { listFilter } };
				rmasOnOrder = await _rmaCommand.ListBuyerRMAs(listOptions, me.Buyer.ID);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return rmasOnOrder;
		}

		/// <summary>
		/// Private re-usable GetShipmentWithItems task method
		/// </summary>
		/// <param name="shipment"></param>
		/// <param name="lineItems"></param>
		/// <returns>The HsShipmentWithItems response object from the GetShipmentWithItems process</returns>
		private async Task<HsShipmentWithItems> GetShipmentWithItems(HsShipmentWithItems shipment, List<LineItem> lineItems)
		{
			try
			{
				var shipmentItems = await _oc.Shipments.ListItemsAsync<HsShipmentItemWithLineItem>(shipment.ID);
				shipment.ShipmentItems = shipmentItems.Items.Select(shipmentItem =>
				{
					shipmentItem.LineItem = lineItems.First(li => li.ID == shipmentItem.LineItemID);
					return shipmentItem;
				}).ToList();
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return shipment;
		}

		/// <summary>
		/// Public re-usable AddPromotion task method
		/// </summary>
		/// <param name="orderId"></param>
		/// <param name="promoCode"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The HsOrder response object from the AddPromotion process</returns>
		public async Task<HsOrder> AddPromotion(string orderId, string promoCode, DecodedToken decodedToken)
		{
			var resp = new HsOrder();
			try
			{
				var orderPromo = await _oc.Orders.AddPromotionAsync(OrderDirection.Incoming, orderId, promoCode);
				resp = await _oc.Orders.GetAsync<HsOrder>(OrderDirection.Incoming, orderId);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Public re-usable ApplyAutomaticPromotions task method
		/// </summary>
		/// <param name="orderId"></param>
		/// <returns>The HsOrder response object from the ApplyAutomaticPromotions process</returns>
		public async Task<HsOrder> ApplyAutomaticPromotions(string orderId)
		{
			var resp = new HsOrder();
			try
			{
				await _promotionCommand.AutoApplyPromotions(orderId);
				resp = await _oc.Orders.GetAsync<HsOrder>(OrderDirection.Incoming, orderId);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Private re-usable EnsureUserCanAccessLocationOrders task method
		/// </summary>
		/// <param name="locationId"></param>
		/// <param name="decodedToken"></param>
		/// <param name="overrideErrorMessage"></param>
		/// <returns></returns>
		private async Task EnsureUserCanAccessLocationOrders(string locationId, DecodedToken decodedToken, string overrideErrorMessage = "")
		{
			try
			{
				var hasAccess = await _locationPermissionCommand.IsUserInAccessGroup(locationId, UserGroupSuffix.ViewAllOrders.ToString(), decodedToken);
				Require.That(hasAccess, new ErrorCode(@"Insufficient Access", $@"This User cannot access orders from this location: {locationId}.", (int)HttpStatusCode.Forbidden));
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Private re-usable EnsureUserCanAccessOrder task method
		/// </summary>
		/// <param name="order"></param>
		/// <param name="decodedToken"></param>
		/// <returns></returns>
		private async Task EnsureUserCanAccessOrder(HsOrder order, DecodedToken decodedToken)
		{
			try
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
				var isUserInLocationOrderAccessGroup = await _locationPermissionCommand.IsUserInAccessGroup(order.BillingAddressID, UserGroupSuffix.ViewAllOrders.ToString(), decodedToken);
				if (isUserInLocationOrderAccessGroup)
				{
					return;
				}

				if (order.Status == OrderStatus.AwaitingApproval)
				{
					// logic assumes there is only one approving group per location
					var isUserInApprovalGroup = await _locationPermissionCommand.IsUserInAccessGroup(order.BillingAddressID, UserGroupSuffix.OrderApprover.ToString(), decodedToken);
					if (isUserInApprovalGroup)
					{
						return;
					}
				}
				// if function has not been exited yet we throw an insufficient access error
				Require.That(false, new ErrorCode(@"Insufficient Access", $@"This User cannot access this order {order.ID}.", (int)HttpStatusCode.Forbidden));
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}
	}
}