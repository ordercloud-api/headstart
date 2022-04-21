using System;
using System.Linq;
using OrderCloud.SDK;
using Headstart.Common;
using OrderCloud.Catalyst;
using Sitecore.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Headstart.Common.Models;
using Headstart.Common.Services;
using System.Collections.Generic;
using Headstart.Common.Repositories;
using Headstart.Common.Models.Misc;
using ordercloud.integrations.library;
using Headstart.Common.Models.Headstart;
using ordercloud.integrations.cardconnect;
using Headstart.Common.Repositories.Models;
using Headstart.Common.Models.Headstart.Extended;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Foundation.SitecoreExtensions.MVC.Extensions;

namespace Headstart.API.Commands
{
	public interface IRMACommand
	{
		Task BuildRMA(HsOrder order, List<string> supplierIds, LineItemStatusChanges lineItemStatusChanges, List<HsLineItem> lineItemsChanged, DecodedToken decodedToken);
		Task<RMA> PostRMA(RMA rma);
		Task<CosmosListPage<RMA>> ListBuyerRMAs(CosmosListOptions listOptions, string buyerId);
		Task<RMA> Get(ListArgs<RMA> args, DecodedToken decodedToken);
		Task<CosmosListPage<RMA>> ListRMAsByOrderId(string orderId, CommerceRole commerceRole, MeUser me, bool accessAllRMAsOnOrder = false);
		Task<CosmosListPage<RMA>> ListRMAs(CosmosListOptions listOptions, DecodedToken decodedToken);
		Task<RMAWithLineItemStatusByQuantity> ProcessRMA(RMA rma, DecodedToken decodedToken);
		Task<RMAWithLineItemStatusByQuantity> ProcessRefund(string rmaNumber, DecodedToken decodedToken);
	}

	public class RMACommand : IRMACommand
	{
		private const string QUEUED_FOR_CAPTURE = "Queued for Capture";
		private readonly IOrderCloudClient _oc;
		private readonly IRMARepo _rmaRepo;
		private readonly IOrderCloudIntegrationsCardConnectService _cardConnect;
		private readonly ISendgridService _sendgridService;
		private readonly AppSettings _settings;
		private readonly ConfigSettings _configSettings = ConfigSettings.Instance;

		/// <summary>
		/// The IOC based constructor method for the RMACommand class object with Dependency Injection
		/// </summary>
		/// <param name="oc"></param>
		/// <param name="rmaRepo"></param>
		/// <param name="cardConnect"></param>
		/// <param name="sendgridService"></param>
		/// <param name="settings"></param>
		public RMACommand(IOrderCloudClient oc, IRMARepo rmaRepo, IOrderCloudIntegrationsCardConnectService cardConnect, ISendgridService sendgridService, AppSettings settings)
		{
			try
			{
				_oc = oc;
				_rmaRepo = rmaRepo;
				_cardConnect = cardConnect;
				_sendgridService = sendgridService;
				_settings = settings;
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Public re-usable BuildRMA task method
		/// </summary>
		/// <param name="order"></param>
		/// <param name="supplierIds"></param>
		/// <param name="lineItemStatusChanges"></param>
		/// <param name="lineItemsChanged"></param>
		/// <param name="decodedToken"></param>
		/// <returns></returns>
		public async Task BuildRMA(HsOrder order, List<string> supplierIds, LineItemStatusChanges lineItemStatusChanges, List<HsLineItem> lineItemsChanged, DecodedToken decodedToken)
		{
			try
			{
				foreach (var supplierId in supplierIds)
				{
					var sellerId = supplierId;
					var sellerName = string.IsNullOrEmpty(supplierId) ? _settings.OrderCloudSettings.MarketplaceName : (await _oc.Suppliers.GetAsync<HsSupplier>(supplierId)).Name;

					var rma = new RMA()
					{
						PartitionKey = @"PartitionValue",
						SourceOrderId = order.ID,
						TotalCredited = 0M,
						ShippingCredited = 0M,
						RMANumber = await BuildRMANumber(order),
						SupplierId = sellerId,
						SupplierName = sellerName,
						Type = lineItemStatusChanges.Status == LineItemStatus.CancelRequested ? RMAType.Cancellation : RMAType.Return,
						DateCreated = DateTime.Now,
						DateComplete = null,
						Status = RMAStatus.Requested,
						LineItems = BuildLineItemRMA(supplierId, lineItemStatusChanges, lineItemsChanged, order),
						Logs = new List<RMALog>(),
						FromBuyerId = order.FromCompanyID,
						FromBuyerUserId = order.FromUser.ID
					};
					await PostRMA(rma);
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}    
		}

		/// <summary>
		/// Private re-usable BuildRMANumber task method
		/// </summary>
		/// <param name="order"></param>
		/// <returns>The RMANumber string value from the BuildRMANumber process</returns>
		private async Task<string> BuildRMANumber(HsOrder order)
		{
			var rmaNumber = string.Empty;
			try
			{
				var args = new CosmosListOptions()
				{
					PageSize = 100,
					Filters = new List<ListFilter>() { new ListFilter(@"SourceOrderID", order.ID) }
				};

				var existingRMAsOnOrder = await ListBuyerRMAs(args, order.FromCompanyID);
				var lastRMANumber = existingRMAsOnOrder.Items.OrderBy(rma => rma.RMANumber).Select(rma => int.Parse(rma.RMANumber.Substring(rma.RMANumber.LastIndexOf(@"-") + 1))).LastOrDefault();
				var rmaSuffix = $@"{lastRMANumber + 1}".PadLeft(2, '0');
				rmaNumber = $@"{order.ID}-RMA-{rmaSuffix}";
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return rmaNumber;
		}

		/// <summary>
		/// Private re-usable BuildLineItemRMA method
		/// </summary>
		/// <param name="supplierId"></param>
		/// <param name="lineItemStatusChanges"></param>
		/// <param name="lineItemsChanged"></param>
		/// <param name="order"></param>
		/// <returns>The list of RMALineItem response objects from the BuildLineItemRMA process</returns>
		private List<RMALineItem> BuildLineItemRMA(string supplierId, LineItemStatusChanges lineItemStatusChanges, List<HsLineItem> lineItemsChanged, HsOrder order)
		{
			var rmaLineItems = new List<RMALineItem>();
			try
			{
				foreach (var lineItem in lineItemsChanged)
				{
					if (lineItem.SupplierID == supplierId)
					{
						var rmaLineItem = new RMALineItem()
						{
							Id = lineItem.ID,
							QuantityRequested = (int) lineItemStatusChanges.Changes.FirstOrDefault(change => change.Id == lineItem.ID)?.Quantity,
							QuantityProcessed = 0,
							Status = RMALineItemStatus.Requested,
							Reason = lineItemStatusChanges.Changes.FirstOrDefault(change => change.Id == lineItem.ID)?.Reason,
							PercentToRefund = null,
							RefundableViaCreditCard = (order.xp.PaymentMethod.Equals($@"Credit Card", StringComparison.OrdinalIgnoreCase)),
							IsResolved = false,
							IsRefunded = false,
							LineTotalRefund = 0
						};
						rmaLineItems.Add(rmaLineItem);
					}
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return rmaLineItems;
		}

		/// <summary>
		/// Public re-usable PostRMA task method
		/// </summary>
		/// <param name="rma"></param>
		/// <returns>The RMA response object from the PostRMA process</returns>
		public virtual async Task<RMA> PostRMA(RMA rma)
		{
			var resp = new RMA(); 
			try
			{
				resp = await _rmaRepo.AddItemAsync(rma);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Public re-usable ListBuyerRMAs task method
		/// </summary>
		/// <param name="listOptions"></param>
		/// <param name="buyerId"></param>
		/// <returns>The CosmosListPage of RMA response objects from the ListBuyerRMAs process</returns>
		public async Task<CosmosListPage<RMA>> ListBuyerRMAs(CosmosListOptions listOptions, string buyerId)
		{
			var rmas = new CosmosListPage<RMA>();
			try
			{
				var queryable = _rmaRepo.GetQueryable().Where(rma => rma.FromBuyerId == buyerId);
				rmas = await GenerateRMAList(queryable, listOptions);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return rmas;
		}

		/// <summary>
		/// Public re-usable Get task method to get the RMA object
		/// </summary>
		/// <param name="args"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The RMA response object from the ListArgs of RMA response objects</returns>
		public async Task<RMA> Get(ListArgs<RMA> args, DecodedToken decodedToken)
		{
			var resp = new RMA();
			try
			{
				var listOptions = new CosmosListOptions()
				{
					PageSize = 100,
					Search = args.Search,
					SearchOn = @"RMANumber"
				};

				var queryable = _rmaRepo.GetQueryable().Where(rma => rma.PartitionKey.Equals($@"PartitionValue", StringComparison.OrdinalIgnoreCase));
				if (decodedToken.CommerceRole == CommerceRole.Supplier)
				{
					var me = await _oc.Me.GetAsync(accessToken: decodedToken.AccessToken);
					queryable = queryable.Where(rma => rma.SupplierId == me.Supplier.ID);
				}
				var rmas = await GenerateRMAList(queryable, listOptions);
				resp = rmas.Items[0];
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Private re-usable GenerateRMAList task method
		/// </summary>
		/// <param name="queryable"></param>
		/// <param name="listOptions"></param>
		/// <returns>The CosmosListPage of RMA response objects from the GenerateRMAList process</returns>
		private async Task<CosmosListPage<RMA>> GenerateRMAList(IQueryable<RMA> queryable, CosmosListOptions listOptions)
		{
			var rmas = new CosmosListPage<RMA>();
			try
			{
				QueryRequestOptions requestOptions = new QueryRequestOptions();
				requestOptions.MaxItemCount = listOptions.PageSize;
				rmas = await _rmaRepo.GetItemsAsync(queryable, requestOptions, listOptions);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return rmas;
		}

		/// <summary>
		/// Public re-usable ListRMAsByOrderId task method
		/// </summary>
		/// <param name="orderId"></param>
		/// <param name="commerceRole"></param>
		/// <param name="me"></param>
		/// <param name="accessAllRMAsOnOrder"></param>
		/// <returns>The CosmosListPage of RMA response objects from the ListRMAsByOrderId process</returns>
		public virtual async Task<CosmosListPage<RMA>> ListRMAsByOrderId(string orderId, CommerceRole commerceRole, MeUser me, bool accessAllRMAsOnOrder = false)
		{
			var rmas = new CosmosListPage<RMA>();
			try
			{
				var sourceOrderId = orderId.Split("-")[0];
				var listOptions = new CosmosListOptions() { PageSize = 100 };
				var queryable = _rmaRepo.GetQueryable().Where(rma => rma.PartitionKey.Equals(@"PartitionValue", StringComparison.OrdinalIgnoreCase) && rma.SourceOrderId == sourceOrderId);
				if (commerceRole == CommerceRole.Supplier && !accessAllRMAsOnOrder)
				{
					queryable = QueryOnlySupplierRMAs(queryable, me.Supplier.ID);
				}
				rmas = await GenerateRMAList(queryable, listOptions);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return rmas;
		}

		/// <summary>
		/// Public re-usable QueryOnlySupplierRMAs method
		/// </summary>
		/// <param name="queryable"></param>
		/// <param name="supplierId"></param>
		/// <returns>The IQueryable list of RMA response object from the QueryOnlySupplierRMAs process</returns>
		public virtual IQueryable<RMA> QueryOnlySupplierRMAs(IQueryable<RMA> queryable, string supplierId)
		{
			var resp = queryable;
			try
			{
				resp = queryable.Where(rma => rma.SupplierId == supplierId);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Public re-usable ListRMAs task method
		/// </summary>
		/// <param name="listOptions"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The CosmosListPage of RMA response objects from the ListRMAs process</returns>
		public async Task<CosmosListPage<RMA>> ListRMAs(CosmosListOptions listOptions, DecodedToken decodedToken)
		{
			var rmas = new CosmosListPage<RMA>();
			try
			{
				var queryable = _rmaRepo.GetQueryable().Where(rma => rma.PartitionKey.Equals($@"PartitionValue", StringComparison.OrdinalIgnoreCase));
				if (decodedToken.CommerceRole == CommerceRole.Supplier)
				{
					var me = await _oc.Me.GetAsync(accessToken: decodedToken.AccessToken);
					queryable = queryable.Where(rma => rma.SupplierId == me.Supplier.ID);
				}
				rmas = await GenerateRMAList(queryable, listOptions);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return rmas;
		}

		/// <summary>
		/// Public re-usable ProcessRMA task method
		/// </summary>
		/// <param name="rma"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The RMAWithLineItemStatusByQuantity response object from the ProcessRMA process</returns>
		/// <exception cref="Exception"></exception>
		public async Task<RMAWithLineItemStatusByQuantity> ProcessRMA(RMA rma, DecodedToken decodedToken)
		{
			var rmaWithStatusByQuantityChanges = new RMAWithLineItemStatusByQuantity();
			try
			{
				// Get the RMA from the last time it was saved.
				var me = await _oc.Me.GetAsync(accessToken: decodedToken.AccessToken);
				var currentRMA = await GetRMA(rma.RMANumber, decodedToken);
				if (decodedToken.CommerceRole == CommerceRole.Supplier && currentRMA.SupplierId != me.Supplier.ID)
				{
					var ex = new Exception($@"You do not have permission to process this RMA.");
					LogExt.LogException(_configSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
					throw ex;
				}

				// Should the Status and IsResolved proprerties of an RMALineItem change?
				rma.LineItems = UpdateRMALineItemStatusesAndCheckIfResolved(rma.LineItems);
				// Should status of RMA change?
				rma = UpdateRMAStatus(rma);
				// If the status on the new RMA differs from the old RMA, create an RMALog
				if (rma.Status != currentRMA.Status)
				{
					RMALog log = new RMALog() { Status = rma.Status, Date = DateTime.Now, FromUserId = me.ID };
					rma.Logs.Insert(0, log);
				}

				var worksheet = await _oc.IntegrationEvents.GetWorksheetAsync<HsOrderWorksheet>(OrderDirection.Incoming, rma.SourceOrderId);
				var deniedLineItems = rma.LineItems.Where(li => !li.IsRefunded && li.Status == RMALineItemStatus.Denied)
					.Where(li => currentRMA.LineItems.FirstOrDefault(currentLi => currentLi.Id == li.Id).Status != RMALineItemStatus.Denied).ToList();
				var lineItemStatusChangesList = BuildLineItemStatusChanges(deniedLineItems, worksheet, rma.Type, true);
				var updatedRMA = await _rmaRepo.ReplaceItemAsync(currentRMA.id, rma);
				await HandlePendingApprovalEmails(currentRMA, rma.LineItems, worksheet);
				rmaWithStatusByQuantityChanges = new RMAWithLineItemStatusByQuantity()
				{
					SupplierOrderId = $"{rma.SourceOrderId}-{rma.SupplierId}",
					RMA = updatedRMA.Resource,
					LineItemStatusChangesList = lineItemStatusChangesList
				};
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return rmaWithStatusByQuantityChanges;
		}

		/// <summary>
		/// Private re-usable GetRMA task method
		/// </summary>
		/// <param name="rmaNumber"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The RMA response object from the GetRMA process</returns>
		private async Task<RMA> GetRMA(string rmaNumber, DecodedToken decodedToken)
		{
			var currentRMA = new RMA();
			try
			{
				var currentRMAFilter = new ListFilter("RMANumber", rmaNumber);
				var currentRMAListOptions = new CosmosListOptions() { PageSize = 1, ContinuationToken = null, Filters = { currentRMAFilter } };
				var currentRMAListPage = await ListRMAs(currentRMAListOptions, decodedToken);
				currentRMA = currentRMAListPage.Items[0];
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return currentRMA;
		}

		/// <summary>
		/// Private re-usable UpdateRMALineItemStatusesAndCheckIfResolved task method
		/// </summary>
		/// <param name="rmaLineItems"></param>
		/// <returns>The list of RMALineItem response objects from the UpdateRMALineItemStatusesAndCheckIfResolved process</returns>
		private List<RMALineItem> UpdateRMALineItemStatusesAndCheckIfResolved(List<RMALineItem> rmaLineItems)
		{
			var resp = rmaLineItems;
			try
			{
				foreach (var lineItem in rmaLineItems)
				{
					// Check each LineItem on the RMA.  If Status is Approved, change to PartialQtyApproved if there is a discrepancy between QuantityRequested and Quantitiy Approved.
					if (lineItem.Status == RMALineItemStatus.Approved || lineItem.Status == RMALineItemStatus.PartialQtyApproved)
					{
						if (lineItem.QuantityProcessed < lineItem.QuantityRequested)
						{
							lineItem.Status = RMALineItemStatus.PartialQtyApproved;
						}
						else
						{
							lineItem.Status = RMALineItemStatus.Approved;
						}
					}
					// Check each LineItem on the RMA.  If the new Status is Approved, PartialQtyApproved, or Denied, IsResolved should be true.  Else, it should be false.
					if (lineItem.Status == RMALineItemStatus.Approved || lineItem.Status == RMALineItemStatus.PartialQtyApproved || lineItem.Status == RMALineItemStatus.Denied)
					{
						lineItem.IsResolved = true;
					}
					else
					{
						lineItem.IsResolved = false;
					}
				}
				resp = rmaLineItems;
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Private re-usable UpdateRMAStatus method
		/// </summary>
		/// <param name="rma"></param>
		/// <returns>The RMA response objects from the UpdateRMAStatus process</returns>
		private RMA UpdateRMAStatus(RMA rma)
		{
			var resp = rma;
			try
			{
				// If any line items have a status of requested or processing, the status should be Processing.
				if (rma.LineItems.Any(li => li.Status == RMALineItemStatus.Requested || li.Status == RMALineItemStatus.Processing))
				{
					rma.Status = RMAStatus.Processing;
				}
				// If all line items have a status of Denied, the status should be Denied.
				else if (rma.LineItems.All(li => li.Status == RMALineItemStatus.Denied))
				{
					rma.Status = RMAStatus.Denied;
					rma.DateComplete = DateTime.Now;
				}
				// If all line items have a status of Complete, PartialQtyComplete, and/or Denied, the status should be Complete.
				else if (rma.LineItems.All(li => li.Status == RMALineItemStatus.Complete || li.Status == RMALineItemStatus.PartialQtyComplete || li.Status == RMALineItemStatus.Denied))
				{
					rma.Status = RMAStatus.Complete;
					rma.DateComplete = DateTime.Now;
				}
				// If RMAs have a mixture of statuses at this point, at least one line item is approved but awaiting processing.  Set to Approved.
				else if (rma.LineItems.Any(li => li.Status == RMALineItemStatus.Approved || li.Status == RMALineItemStatus.PartialQtyApproved))
				{
					rma.Status = RMAStatus.Approved;
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Private re-usable BuildLineItemStatusChanges method
		/// </summary>
		/// <param name="rmaLineItemsToUpdate"></param>
		/// <param name="worksheet"></param>
		/// <param name="rmaType"></param>
		/// <param name="isDenyingAll"></param>
		/// <returns>The list of LineItemStatusChanges response objects from the BuildLineItemStatusChanges process</returns>
		private List<LineItemStatusChanges> BuildLineItemStatusChanges(IEnumerable<RMALineItem> rmaLineItemsToUpdate, HsOrderWorksheet worksheet, RMAType rmaType, bool isDenyingAll)
		{
			var lineItemStatusChangesList = new List<LineItemStatusChanges>();
			try
			{
				if ((!rmaLineItemsToUpdate.Any()))
				{
					return lineItemStatusChangesList;
				}

				var orderWorksheetLineItems = worksheet.LineItems.Where(li => rmaLineItemsToUpdate.Any(itemToUpdate => itemToUpdate.Id == li.ID));
				var actionCompleteType = rmaType == RMAType.Cancellation ? LineItemStatus.Canceled : LineItemStatus.Returned;
				var actionDeniedType = rmaType == RMAType.Cancellation ? LineItemStatus.CancelDenied : LineItemStatus.ReturnDenied;
				foreach (var lineItem in orderWorksheetLineItems)
				{
					var correspondingRMALineItem = rmaLineItemsToUpdate.FirstOrDefault(li => li.Id == lineItem.ID);
					if (correspondingRMALineItem.QuantityProcessed == correspondingRMALineItem.QuantityRequested)
					{
						if (isDenyingAll)
						{
							var statusChangeToAdjust = lineItemStatusChangesList.FirstOrDefault(change => change.Status == actionDeniedType);
							BuildStatusChangeToAdjust(statusChangeToAdjust, actionDeniedType, lineItemStatusChangesList, correspondingRMALineItem, isDenyingAll);
						}
						else
						{
							var statusChangeToAdjust = lineItemStatusChangesList.FirstOrDefault(change => change.Status == actionCompleteType);
							BuildStatusChangeToAdjust(statusChangeToAdjust, actionCompleteType, lineItemStatusChangesList, correspondingRMALineItem, isDenyingAll);
						}
					}
					else
					{
						var quantityToComplete = correspondingRMALineItem.QuantityProcessed;
						var quantityToDeny = correspondingRMALineItem.QuantityRequested - correspondingRMALineItem.QuantityProcessed;
						var statusChangeForComplete = lineItemStatusChangesList.FirstOrDefault(change => change.Status == actionCompleteType);
						var statusChangeForDeny = lineItemStatusChangesList.FirstOrDefault(change => change.Status == actionDeniedType);
						BuildStatusChangeToAdjust(statusChangeForComplete, actionCompleteType, lineItemStatusChangesList, correspondingRMALineItem, isDenyingAll, quantityToComplete);
						BuildStatusChangeToAdjust(statusChangeForDeny, actionDeniedType, lineItemStatusChangesList, correspondingRMALineItem, isDenyingAll, quantityToDeny);
					}
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return lineItemStatusChangesList;
		}

		/// <summary>
		/// Private re-usable BuildStatusChangeToAdjust method
		/// </summary>
		/// <param name="statusChangeToAdjust"></param>
		/// <param name="lineItemStatus"></param>
		/// <param name="lineItemStatusChangesList"></param>
		/// <param name="correspondingRMALineItem"></param>
		/// <param name="isDenyingAll"></param>
		/// <param name="overriddenQuantity"></param>
		private void BuildStatusChangeToAdjust(LineItemStatusChanges statusChangeToAdjust, LineItemStatus lineItemStatus, List<LineItemStatusChanges> lineItemStatusChangesList, RMALineItem correspondingRMALineItem, bool isDenyingAll, int? overriddenQuantity = null)
		{
			try
			{
				int? quantityToChange = overriddenQuantity != null ? overriddenQuantity : correspondingRMALineItem.QuantityProcessed;
				if ((int)quantityToChange == 0)
				{
					return;
				}

				if (statusChangeToAdjust == null)
				{
					var newStatusChange = new LineItemStatusChanges() { Status = lineItemStatus, Changes = new List<LineItemStatusChange>() };
					lineItemStatusChangesList.Add(newStatusChange);
					statusChangeToAdjust = lineItemStatusChangesList.FirstOrDefault(change => change.Status == lineItemStatus);
				}

				decimal? refundAmount = null;
				if (!isDenyingAll)
				{
					refundAmount = correspondingRMALineItem.LineTotalRefund;
				}
				statusChangeToAdjust.Changes.Add(new LineItemStatusChange()
				{
					Id = correspondingRMALineItem.Id,
					Quantity = (int)quantityToChange,
					Comment = correspondingRMALineItem.Comment,
					Refund = refundAmount,
					QuantityRequestedForRefund = correspondingRMALineItem.QuantityRequested,
				});
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Private re-usable HandlePendingApprovalEmails task method
		/// </summary>
		/// <param name="rma"></param>
		/// <param name="rmaLineItems"></param>
		/// <param name="worksheet"></param>
		/// <returns></returns>
		private async Task HandlePendingApprovalEmails(RMA rma, List<RMALineItem> rmaLineItems, HsOrderWorksheet worksheet)
		{
			try
			{
				var pendingApprovalLineItemsWithNewComments = rmaLineItems.Where(li => li.Status == RMALineItemStatus.Processing).Where(li => li.Comment != rma.LineItems.FirstOrDefault(item => item.Id == li.Id).Comment);
				if (!pendingApprovalLineItemsWithNewComments.Any())
				{
					return;
				}

				var lineItemStatusChanges = new LineItemStatusChanges()
				{
					Status = rma.Type == RMAType.Cancellation ? LineItemStatus.CancelRequested : LineItemStatus.ReturnRequested,
					Changes = new List<LineItemStatusChange>()
				};
				foreach (var rmaLineItem in pendingApprovalLineItemsWithNewComments)
				{
					lineItemStatusChanges.Changes.Add(new LineItemStatusChange()
					{
						Id = rmaLineItem.Id,
						Quantity = rmaLineItem.QuantityProcessed,
						Comment = rmaLineItem.Comment,
						QuantityRequestedForRefund = rmaLineItem.QuantityRequested
					});
				}

				var lineItemsChanged = worksheet.LineItems.Where(li => lineItemStatusChanges.Changes.Select(li => li.Id).Contains(li.ID)).ToList();
				var supplier = await _oc.Suppliers.GetAsync(rma.SupplierId);
				var emailText = new EmailDisplayText()
				{
					EmailSubject = $@"New message available from {supplier.Name}",
					DynamicText = $@"{supplier.Name} has contacted you regarding your request for {rma.Type.ToString().ToLower()}",
					DynamicText2 = @"The following items have new messages"
				};
				await _sendgridService.SendLineItemStatusChangeEmail(worksheet.Order, lineItemStatusChanges, lineItemsChanged, worksheet.Order.FromUser.FirstName, worksheet.Order.FromUser.LastName, worksheet.Order.FromUser.Email, emailText);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Public re-usable ProcessRefund task method
		/// </summary>
		/// <param name="rmaNumber"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The RMAWithLineItemStatusByQuantity response object from the ProcessRefund process</returns>
		public async Task<RMAWithLineItemStatusByQuantity> ProcessRefund(string rmaNumber, DecodedToken decodedToken)
		{
			var rmaWithStatusByQuantityChanges = new RMAWithLineItemStatusByQuantity();
			try
			{
				var me = await _oc.Me.GetAsync(accessToken: decodedToken.AccessToken);
				var rma = await GetRMA(rmaNumber, decodedToken);
				ValidateRMA(rma, me, decodedToken);

				var initialAmountRefunded = rma.TotalCredited;
				var rmaLineItemsToUpdate = rma.LineItems.Where(li => !li.IsRefunded && (li.Status == RMALineItemStatus.Approved || li.Status == RMALineItemStatus.PartialQtyApproved)).ToList();
				var worksheet = await _oc.IntegrationEvents.GetWorksheetAsync<HsOrderWorksheet>(OrderDirection.Incoming, rma.SourceOrderId);
				var allRMAsOnThisOrder = await ListRMAsByOrderId(worksheet.Order.ID, decodedToken.CommerceRole, me, true);
				CalculateAndUpdateLineTotalRefund(rmaLineItemsToUpdate, worksheet, allRMAsOnThisOrder, rma.SupplierId);

				// UPDATE RMA LINE ITEM STATUSES
				SetRMALineItemStatusesToComplete(rmaLineItemsToUpdate);
				// UPDATE RMA STATUS
				UpdateRMAStatus(rma);
				await HandleRefund(rma, allRMAsOnThisOrder, worksheet, decodedToken);
				MarkRMALineItemsAsRefunded(rmaLineItemsToUpdate);
				decimal totalRefundedForThisTransaction = rma.TotalCredited - initialAmountRefunded;
				var log = new RMALog() { Status = rma.Status, Date = DateTime.Now, AmountRefunded = totalRefundedForThisTransaction, FromUserId = me.ID };
				rma.Logs.Insert(0, log);

				var lineItemStatusChangesList = BuildLineItemStatusChanges(rmaLineItemsToUpdate, worksheet, rma.Type, false);
				// SAVE RMA
				var updatedRMA = await _rmaRepo.ReplaceItemAsync(rma.id, rma);
				rmaWithStatusByQuantityChanges = new RMAWithLineItemStatusByQuantity()
				{
					SupplierOrderId = $@"{rma.SourceOrderId}-{rma.SupplierId}", 
					RMA = updatedRMA.Resource, 
					LineItemStatusChangesList = lineItemStatusChangesList
				};
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return rmaWithStatusByQuantityChanges;
		}

		/// <summary>
		/// Private re-usable ValidateRMA task method
		/// </summary>
		/// <param name="rma"></param>
		/// <param name="me"></param>
		/// <param name="decodedToken"></param>
		/// <exception cref="Exception"></exception>
		private void ValidateRMA(RMA rma, MeUser me, DecodedToken decodedToken)
		{
			try
			{
				if (decodedToken.CommerceRole == CommerceRole.Supplier && rma.SupplierId != me.Supplier.ID)
				{
					var ex = new Exception(@"You do not have permission to process a refund for this RMA.");
					LogExt.LogException(_configSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
					throw ex;
				}

				if (rma.Status == RMAStatus.Complete || rma.Status == RMAStatus.Denied || rma.Status == RMAStatus.Requested)
				{
					var ex = new Exception(@"This RMA is not in a valid status to be refunded.");
					LogExt.LogException(_configSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
					throw ex;
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Public re-usable CalculateAndUpdateLineTotalRefund task method
		/// </summary>
		/// <param name="lineItemsToUpdate"></param>
		/// <param name="orderWorksheet"></param>
		/// <param name="allRMAsOnThisOrder"></param>
		/// <param name="supplierId"></param>
		public void CalculateAndUpdateLineTotalRefund(IEnumerable<RMALineItem> lineItemsToUpdate, HsOrderWorksheet orderWorksheet, CosmosListPage<RMA> allRMAsOnThisOrder, string supplierId)
		{
			try
			{
				var taxLines = orderWorksheet?.OrderCalculateResponse?.xp?.TaxCalculation?.LineItems;
				foreach (var rmaLineItem in lineItemsToUpdate)
				{
					if (!rmaLineItem.RefundableViaCreditCard)
					{
						var lineItemFromOrder = orderWorksheet.LineItems.FirstOrDefault(li => li.ID == rmaLineItem.Id);
						rmaLineItem.LineTotalRefund = lineItemFromOrder.LineTotal / lineItemFromOrder.Quantity * rmaLineItem.QuantityProcessed;
					}
					else
					{
						var quantityToRefund = rmaLineItem.QuantityProcessed;
						var lineItem = orderWorksheet.LineItems.First(li => li.ID == rmaLineItem.Id);
						var taxLine = taxLines == null ? null : taxLines.FirstOrDefault(taxLine => taxLine.LineItemID == rmaLineItem.Id);
						var lineItemTotalTax = taxLine?.LineItemTotalTax ?? 0;

						// Exempt products will have an exempt amount instead of a taxable amount.
						var lineItemBaseCost = lineItem.xp.LineTotalWithProportionalDiscounts;
						var totalRefundIfReturningAllLineItems = lineItemBaseCost + lineItemTotalTax;
						var taxableAmountPerSingleLineItem = (double)(lineItemBaseCost / lineItem.Quantity);
						var taxPerSingleLineItem = (double)(lineItemTotalTax / lineItem.Quantity);
						var singleQuantityLineItemRefund = Math.Round(taxableAmountPerSingleLineItem + taxPerSingleLineItem, 2);
						var expectedLineTotalRefund = (decimal)singleQuantityLineItemRefund * quantityToRefund;
						rmaLineItem.LineTotalRefund = ValidateExpectedLineTotalRefund(expectedLineTotalRefund, totalRefundIfReturningAllLineItems, allRMAsOnThisOrder, rmaLineItem, orderWorksheet, supplierId);
					}

					ApplyPercentToDiscount(rmaLineItem);
					rmaLineItem.Status = rmaLineItem.QuantityProcessed == rmaLineItem.QuantityRequested ? RMALineItemStatus.Complete : RMALineItemStatus.PartialQtyComplete;
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}            
		}

		/// <summary>
		/// Private re-usable ValidateExpectedLineTotalRefund method
		/// </summary>
		/// <param name="expectedLineTotalRefund"></param>
		/// <param name="totalRefundIfReturningAllLineItems"></param>
		/// <param name="allRMAsOnThisOrder"></param>
		/// <param name="rmaLineItem"></param>
		/// <param name="orderWorksheet"></param>
		/// <param name="supplierId"></param>
		/// <returns>The ExpectedLineTotalRefund decimal value from ValidateExpectedLineTotalRefund process</returns>
		private decimal ValidateExpectedLineTotalRefund(decimal expectedLineTotalRefund, decimal totalRefundIfReturningAllLineItems, CosmosListPage<RMA> allRMAsOnThisOrder, RMALineItem rmaLineItem, HsOrderWorksheet orderWorksheet, string supplierId)
		{
			var resp = expectedLineTotalRefund;
			try
			{
				// If minor rounding error occurs during singleQuantityLineItemRefund calculation, ensure we don't refund more than the full line item cost on the order
				// Would only occur for full quantity cancellations/returns
				if (expectedLineTotalRefund > totalRefundIfReturningAllLineItems || ShouldIssueFullLineItemRefund(rmaLineItem, allRMAsOnThisOrder, orderWorksheet, supplierId))
				{
					return totalRefundIfReturningAllLineItems;
				}

				// If previous RMAs on this order for the same line item
				if (allRMAsOnThisOrder.Items.Count > 1)
				{
					var previouslyRefundedAmountForThisLineItem = 0M;
					// Find previously refunded total for line items on this order...
					foreach (var previouslyRefundedRMA in allRMAsOnThisOrder.Items)
					{
						var previouslyRefundedLineItem = previouslyRefundedRMA.LineItems.FirstOrDefault(li => li.Id == rmaLineItem.Id);
						if (previouslyRefundedLineItem != null)
						{
							if (previouslyRefundedLineItem.IsRefunded && previouslyRefundedLineItem.RefundableViaCreditCard)
							{
								previouslyRefundedAmountForThisLineItem += previouslyRefundedLineItem.LineTotalRefund;
							}
						}
					}

					// If previous total + new line total > totalRefundIfReturningAllLineItems, then totalRefundIfReturningAllLineItems - previousTotal = newLineTotal
					if (previouslyRefundedAmountForThisLineItem + expectedLineTotalRefund > totalRefundIfReturningAllLineItems)
					{
						var totalAfterPossibleRoundingErrors = totalRefundIfReturningAllLineItems - previouslyRefundedAmountForThisLineItem;
						return totalAfterPossibleRoundingErrors;
					}
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}


		/// <summary>
		/// Private re-usable ShouldIssueFullLineItemRefund method
		/// </summary>
		/// <param name="rmaLineItem"></param>
		/// <param name="allRMAsOnThisOrder"></param>
		/// <param name="orderWorksheet"></param>
		/// <param name="supplierId"></param>
		/// <returns>The Should IssueFullLineItemRefund boolean status value from the ShouldIssueFullLineItemRefund process</returns>
		private bool ShouldIssueFullLineItemRefund(RMALineItem rmaLineItem, CosmosListPage<RMA> allRMAsOnThisOrder, HsOrderWorksheet orderWorksheet, string supplierId)
		{
			var resp = false;
			try
			{
				var lineItem = orderWorksheet.LineItems.First(li => li.ID == rmaLineItem.Id);
				var rmasFromThisSupplier = allRMAsOnThisOrder.Items.Where(r => r.SupplierId == supplierId);
				// If this is the only RMA for this line item and all requested RMA quantity are approved, and the quantity equals the original order quantity, issue a full refund (line item cost + tax).
				if (rmaLineItem.Status == RMALineItemStatus.Approved && rmaLineItem.QuantityProcessed == lineItem.Quantity && rmasFromThisSupplier.Count() == 1)
				{
					resp = true;
				}
				else
				{
					resp = false;
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Public re-usable ApplyPercentToDiscount method
		/// </summary>
		/// <param name="rmaLineItem"></param>
		/// <exception cref="Exception"></exception>
		public virtual void ApplyPercentToDiscount(RMALineItem rmaLineItem)
		{
			try
			{
				if (rmaLineItem.PercentToRefund <= 0 || rmaLineItem.PercentToRefund > 100)
				{
					var ex = new Exception($@"The refund percentage for the {rmaLineItem.Id} must be greater than 0 and no higher than 100 percent.");
					LogExt.LogException(_configSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
					throw ex;
				}
				if (rmaLineItem.PercentToRefund != null)
				{
					// Math.Round() by default would round 13.745 to 13.74 based on banker's rounding (going to the nearest even number when the final digit is 5).
					// This errs on the side of rounding up (away from zero) when only a percentage of a refund is applied.
					rmaLineItem.LineTotalRefund = Math.Round((decimal)(rmaLineItem.LineTotalRefund / 100 * rmaLineItem.PercentToRefund), 2, MidpointRounding.AwayFromZero);
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Private re-usable SetRMALineItemStatusesToComplete method
		/// </summary>
		/// <param name="rmaLineItemsToUpdate"></param>
		private void SetRMALineItemStatusesToComplete(IEnumerable<RMALineItem> rmaLineItemsToUpdate)
		{
			try
			{
				foreach (var lineItem in rmaLineItemsToUpdate)
				{
					if (lineItem.QuantityRequested == lineItem.QuantityProcessed)
					{
						lineItem.Status = RMALineItemStatus.Complete;
					}
					else
					{
						lineItem.Status = RMALineItemStatus.PartialQtyComplete;
					}
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Public re-usable HandleRefund task method
		/// </summary>
		/// <param name="rma"></param>
		/// <param name="allRMAsOnThisOrder"></param>
		/// <param name="worksheet"></param>
		/// <param name="decodedToken"></param>
		/// <returns></returns>
		public virtual async Task HandleRefund(RMA rma, CosmosListPage<RMA> allRMAsOnThisOrder, HsOrderWorksheet worksheet, DecodedToken decodedToken)
		{
			try
			{
				// Get payment info from the order
				var paymentResponse = await _oc.Payments.ListAsync<HsPayment>(OrderDirection.Incoming, rma.SourceOrderId);
				var creditCardPayment = paymentResponse.Items.FirstOrDefault(payment => payment.Type == OrderCloud.SDK.PaymentType.CreditCard);
				if (creditCardPayment == null)
				{
					// Items were not paid for with a credit card.  No refund to process via CardConnect.
					return;
				}

				var creditCardPaymentTransaction = creditCardPayment.Transactions.OrderBy(x => x.DateExecuted).LastOrDefault(x => x.Type.Equals($@"CreditCard", StringComparison.OrdinalIgnoreCase) && x.Succeeded);
				var purchaseOrderTotal = (decimal) paymentResponse.Items.Where(payment => payment.Type == PaymentType.PurchaseOrder).Select(payment => payment.Amount).Sum();
				// Refund via CardConnect
				var inquiry = await _cardConnect.Inquire(new CardConnectInquireRequest
				{
					merchid = creditCardPaymentTransaction.xp.CardConnectResponse.merchid,
					orderid = rma.SourceOrderId,
					set = @"1",
					currency = worksheet.Order.xp.Currency.ToString(),
					retref = creditCardPaymentTransaction.xp.CardConnectResponse.retref
				});

				var shippingRefund = rma.Type == RMAType.Cancellation ? GetShippingRefundIfCancellingAll(worksheet, rma, allRMAsOnThisOrder) : 0M;
				var lineTotalToRefund = rma.LineItems.Where(li => li.IsResolved && !li.IsRefunded && li.RefundableViaCreditCard && (li.Status == RMALineItemStatus.PartialQtyComplete || li.Status == RMALineItemStatus.Complete))
					.Select(li => li.LineTotalRefund).Sum();
				var totalToRefund = (lineTotalToRefund + shippingRefund);
				// Update Total Credited on RMA
				rma.TotalCredited += totalToRefund;

				// Transactions that are queued for capture can only be fully voided, and we are only allowing partial voids moving forward.
				if (inquiry.voidable.Equals(@"Y", StringComparison.OrdinalIgnoreCase) && inquiry.setlstat == QUEUED_FOR_CAPTURE)
				{
					var ex = new CatalystBaseException(new ApiError
					{
						ErrorCode = @"Payment.FailedToVoidAuthorization",
						Message = @"This customer's credit card transaction is currently queued for capture and cannot be refunded at this time.  Please try again later."
					});
					LogExt.LogException(_configSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
					throw ex;
				}

				// If voidable, but not refundable, void the refund amount off the original order total
				if (inquiry.voidable.Equals(@"Y", StringComparison.OrdinalIgnoreCase))
				{
					await HandleVoidTransaction(worksheet, creditCardPayment, creditCardPaymentTransaction, totalToRefund, rma);
				}
				// If refundable, but not voidable, do a refund
				if (inquiry.voidable.Equals(@"N", StringComparison.OrdinalIgnoreCase))
				{
					await HandleRefundTransaction(worksheet, creditCardPayment, creditCardPaymentTransaction, totalToRefund, rma);
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Public re-usable GetShippingRefundIfCancellingAll task method
		/// </summary>
		/// <param name="worksheet"></param>
		/// <param name="rma"></param>
		/// <param name="allRMAsOnThisOrder"></param>
		/// <returns>The decimal value from the GetShippingRefundIfCancellingAll process</returns>
		public virtual decimal GetShippingRefundIfCancellingAll(HsOrderWorksheet worksheet, RMA rma, CosmosListPage<RMA> allRMAsOnThisOrder)
		{
			var resp = 0M;
			try
			{
				// What are all the line items on this order for this supplier and their quantities?
				var allLineItemsShippedFromThisSupplier = worksheet.LineItems.Where(li => li.SupplierID == rma.SupplierId && worksheet.Order.xp.PaymentMethod.Equals("Credit Card", StringComparison.OrdinalIgnoreCase));
				var allLineItemsDictionary = new Dictionary<string, int>();
				foreach (var li in allLineItemsShippedFromThisSupplier)
				{
					allLineItemsDictionary.Add(li.ID, li.Quantity);
				}

				// Including this RMA and previous RMAs for this supplier, get everything that has been refunded or is about to be refunded.
				var rmasFromThisSupplier = allRMAsOnThisOrder.Items.Where(r => r.SupplierId == rma.SupplierId);
				var allCompleteRMALineItemsDictionary = new Dictionary<string, int>();
				foreach (var existingRMA in rmasFromThisSupplier)
				{
					var rmaToAnalyze = existingRMA.RMANumber == rma.RMANumber ? rma : existingRMA;
					foreach (var rmaLineItem in rmaToAnalyze.LineItems)
					{
						if (rmaLineItem.Status == RMALineItemStatus.Complete && rmaLineItem.RefundableViaCreditCard)
						{
							if (!allCompleteRMALineItemsDictionary.ContainsKey(rmaLineItem.Id))
							{
								allCompleteRMALineItemsDictionary.Add(rmaLineItem.Id, rmaLineItem.QuantityProcessed);
							}
							else
							{
								allCompleteRMALineItemsDictionary[rmaLineItem.Id] += rmaLineItem.QuantityProcessed;
							}
						}
					}
				}

				// If these are the same, the supplier hasn't shipped anything, and shipping should be credited back to the buyer.
				var shouldShippingBeCanceled = allLineItemsDictionary.OrderBy(kvp => kvp.Key).SequenceEqual(allCompleteRMALineItemsDictionary.OrderBy(kvp => kvp.Key));
				// Figure out what the buyer paid for shipping for this supplier on this order.
				if (shouldShippingBeCanceled)
				{
					var shipEstimateId = worksheet.ShipEstimateResponse.ShipEstimates.FirstOrDefault(estimate => estimate.xp.SupplierID == rma.SupplierId)?.ID;
					var shippingLine = worksheet.OrderCalculateResponse.xp.TaxCalculation.OrderLevelTaxes.First(tax => tax.ShipEstimateID == shipEstimateId);
					var shippingCostToRefund = (decimal)(shippingLine.Taxable + shippingLine.Tax + shippingLine.Exempt);
					rma.ShippingCredited += shippingCostToRefund;
					return shippingCostToRefund;
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Public re-usable HandleVoidTransaction task method
		/// </summary>
		/// <param name="worksheet"></param>
		/// <param name="creditCardPayment"></param>
		/// <param name="creditCardPaymentTransaction"></param>
		/// <param name="totalToRefund"></param>
		/// <param name="rma"></param>
		/// <returns></returns>
		/// <exception cref="CatalystBaseException"></exception>
		public virtual async Task HandleVoidTransaction(HsOrderWorksheet worksheet, HsPayment creditCardPayment, HsPaymentTransaction creditCardPaymentTransaction, decimal totalToRefund, RMA rma)
		{
			try
			{
				var newCreditCardVoid = new HsPayment() { Amount = totalToRefund };
				try
				{
					var response = await _cardConnect.VoidAuthorization(new CardConnectVoidRequest
					{
						currency = worksheet.Order.xp.Currency.ToString(),
						merchid = creditCardPaymentTransaction.xp.CardConnectResponse.merchid,
						retref = creditCardPaymentTransaction.xp.CardConnectResponse.retref,
						amount = totalToRefund.ToString("F2"),
					});
					await _oc.Payments.CreateTransactionAsync(OrderDirection.Incoming, rma.SourceOrderId, creditCardPayment.ID, CardConnectMapper.Map(newCreditCardVoid, response));
				}
				catch (CreditCardVoidException ex)
				{
					await _oc.Payments.CreateTransactionAsync(OrderDirection.Incoming, rma.SourceOrderId, creditCardPayment.ID, CardConnectMapper.Map(newCreditCardVoid, ex.Response));
					var ex1 = new CatalystBaseException(new ApiError
					{
						ErrorCode = @"Payment.FailedToVoidAuthorization",
						Message = ex.ApiError.Message
					});
					LogExt.LogException(_configSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", $@"{ex.Message}. {ex1.Message}.", ex.StackTrace, this, true);
					throw ex1;
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Public re-usable HandleRefundTransaction task method
		/// </summary>
		/// <param name="worksheet"></param>
		/// <param name="creditCardPayment"></param>
		/// <param name="creditCardPaymentTransaction"></param>
		/// <param name="totalToRefund"></param>
		/// <param name="rma"></param>
		/// <returns></returns>
		/// <exception cref="CatalystBaseException"></exception>
		public virtual async Task HandleRefundTransaction(HsOrderWorksheet worksheet, HsPayment creditCardPayment, HsPaymentTransaction creditCardPaymentTransaction, decimal totalToRefund, RMA rma)
		{
			try
			{
				var requestedRefund = new CardConnectRefundRequest()
				{
					currency = worksheet.Order.xp.Currency.ToString(),
					merchid = creditCardPaymentTransaction.xp.CardConnectResponse.merchid,
					retref = creditCardPaymentTransaction.xp.CardConnectResponse.retref,
					amount = totalToRefund.ToString(@"F2"),
				};
				var response = await _cardConnect.Refund(requestedRefund);
				var newCreditCardPayment = new HsPayment() { Amount = response.amount };
				await _oc.Payments.CreateTransactionAsync(OrderDirection.Incoming, rma.SourceOrderId, creditCardPayment.ID, CardConnectMapper.Map(newCreditCardPayment, response));
			}
			catch (CreditCardRefundException ex)
			{
				var ex1 = new CatalystBaseException(new ApiError
				{
					ErrorCode = @"Payment.FailedToRefund",
					Message = ex.ApiError.Message
				});
				LogExt.LogException(_configSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", $@"{ex.Message}. {ex1.Message}.", ex.StackTrace, this, true);
				throw ex1;
			}
		}

		/// <summary>
		/// Public re-usable MarkRMALineItemsAsRefunded task method
		/// </summary>
		/// <param name="rmaLineItemsToUpdate"></param>
		private void MarkRMALineItemsAsRefunded(IEnumerable<RMALineItem> rmaLineItemsToUpdate)
		{
			try
			{
				foreach (var rmaLineItem in rmaLineItemsToUpdate)
				{
					rmaLineItem.IsRefunded = true;
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}
	}
}