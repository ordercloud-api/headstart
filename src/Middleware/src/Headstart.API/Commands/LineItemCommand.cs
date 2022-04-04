using System;
using System.Linq;
using OrderCloud.SDK;
using Newtonsoft.Json;
using OrderCloud.Catalyst;
using Sitecore.Diagnostics;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;
using Headstart.Common.Models;
using Headstart.Common.Services;
using Headstart.Models.Headstart;
using System.Collections.Generic;
using Headstart.Common.Constants;
using Headstart.Common.Extensions;
using Headstart.Common.Models.Misc;
using Microsoft.ApplicationInsights;
using ordercloud.integrations.library;
using Headstart.Common.Models.Headstart;
using Headstart.Common.Repositories.Models;
using Headstart.Common.Models.Headstart.Extended;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Foundation.SitecoreExtensions.MVC.Extensions;

namespace Headstart.API.Commands
{
	public interface ILineItemCommand
	{
		Task<HsLineItem> UpsertLineItem(string orderId, HsLineItem li, DecodedToken decodedToken);
		Task<List<HsLineItem>> UpdateLineItemStatusesAndNotifyIfApplicable(OrderDirection orderDirection, string orderId, LineItemStatusChanges lineItemStatusChanges, DecodedToken decodedToken = null);
		Task<List<HsLineItem>> SetInitialSubmittedLineItemStatuses(string buyerOrderId);
		Task DeleteLineItem(string orderId, string lineItemId, DecodedToken decodedToken);
		Task<decimal> ValidateLineItemUnitCost(string orderId, SuperHsMeProduct product, List<HsLineItem> existingLineItems, HsLineItem li);
		Task HandleRMALineItemStatusChanges(OrderDirection orderDirection, RMAWithLineItemStatusByQuantity rmaWithLineItemStatusByQuantity, DecodedToken decodedToken);
	}

	public class LineItemCommand : ILineItemCommand
	{
		private readonly IOrderCloudClient _oc;
		private readonly ISendgridService _sendgridService;
		private readonly IMeProductCommand _meProductCommand;
		private readonly IPromotionCommand _promotionCommand;
		private readonly IRMACommand _rmaCommand;
		private readonly TelemetryClient _telemetry;
		private readonly ConfigSettings _configSettings = ConfigSettings.Instance;

		/// <summary>
		/// The IOC based constructor method for the LineItemCommand class object with Dependency Injection
		/// </summary>
		/// <param name="sendgridService"></param>
		/// <param name="oc"></param>
		/// <param name="meProductCommand"></param>
		/// <param name="promotionCommand"></param>
		/// <param name="rmaCommand"></param>
		/// <param name="telemetry"></param>
		public LineItemCommand(ISendgridService sendgridService, IOrderCloudClient oc, IMeProductCommand meProductCommand, IPromotionCommand promotionCommand, IRMACommand rmaCommand, TelemetryClient telemetry)
		{
			try
			{
				_oc = oc;
				_sendgridService = sendgridService;
				_meProductCommand = meProductCommand;
				_promotionCommand = promotionCommand;
				_rmaCommand = rmaCommand;
				_telemetry = telemetry;
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Public re-usable SetInitialSubmittedLineItemStatuses task method
		/// </summary>
		/// <param name="buyerOrderId"></param>
		/// <returns>The list of HsLineItem response objects from the SetInitialSubmittedLineItemStatuses process</returns>
		public async Task<List<HsLineItem>> SetInitialSubmittedLineItemStatuses(string buyerOrderId)
		{
			var resp = new List<HsLineItem>();
			try
			{
				var lineItems = await _oc.LineItems.ListAllAsync<HsLineItem>(OrderDirection.Incoming, buyerOrderId);
				var updatedLineItems = await Throttler.RunAsync(lineItems, 100, 5, li =>
				{
					var partial = new PartialLineItem()
					{
						xp = new
						{
							StatusByQuantity = new Dictionary<LineItemStatus, int>() {
								{
									LineItemStatus.Submitted, 
									li.Quantity
								},
								{
									LineItemStatus.Open, 
									0
								},
								{
									LineItemStatus.BackOrdered,
									0
								},
								{
									LineItemStatus.Canceled, 
									0
								},
								{
									LineItemStatus.CancelRequested, 
									0
								},
								{
									LineItemStatus.CancelDenied, 
									0
								},
								{
									LineItemStatus.Returned, 
									0
								},
								{
									LineItemStatus.ReturnRequested, 
									0
								},
								{
									LineItemStatus.ReturnDenied, 
									0
								},
								{
									LineItemStatus.Complete, 
									0
								}
							},
							Returns = new List<LineItemClaim>(),
							Cancelations = new List<LineItemClaim>()
						}
					};
					return _oc.LineItems.PatchAsync<HsLineItem>(OrderDirection.Incoming, buyerOrderId, li.ID, partial);
				});
				resp = updatedLineItems.ToList();
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}            
			return resp;
		}

		/// <summary>
		/// Public re-usable UpdateLineItemStatusesAndNotifyIfApplicable task method
		/// Validates LineItemStatus Change, Updates Line Item Statuses, Updates Order Statuses, Sends Necessary Emails
		/// </summary>
		/// <param name="orderDirection"></param>
		/// <param name="orderId"></param>
		/// <param name="lineItemStatusChanges"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The list of HsLineItem response objects from the UpdateLineItemStatusesAndNotifyIfApplicable process</returns>
		public async Task<List<HsLineItem>> UpdateLineItemStatusesAndNotifyIfApplicable(OrderDirection orderDirection, string orderId, LineItemStatusChanges lineItemStatusChanges, DecodedToken decodedToken = null)
		{
			var resp = new List<HsLineItem>();
			try
			{
				var userType = decodedToken?.CommerceRole.ToString().ToLower() ?? "noUser";
				userType = userType == "seller" ? "admin" : userType;
				var verifiedUserType = userType.Reserialize<VerifiedUserType>();
				var buyerOrderId = orderId.Split('-')[0];
				var previousLineItemsStates = await _oc.LineItems.ListAllAsync<HsLineItem>(OrderDirection.Incoming, buyerOrderId);

				ValidateLineItemStatusChange(previousLineItemsStates.ToList(), lineItemStatusChanges, verifiedUserType);
				var updatedLineItems = await Throttler.RunAsync(lineItemStatusChanges.Changes, 100, 5, (lineItemStatusChange) =>
				{
					var newPartialLineItem = BuildNewPartialLineItem(lineItemStatusChange, previousLineItemsStates.ToList(), lineItemStatusChanges.Status);
					// if there is no verified user passed in it has been called from somewhere else in the code base and will be done with the client grant access
					return decodedToken != null ? _oc.LineItems.PatchAsync<HsLineItem>(orderDirection, orderId, lineItemStatusChange.Id, newPartialLineItem, decodedToken.AccessToken) : _oc.LineItems.PatchAsync<HsLineItem>(orderDirection, orderId, lineItemStatusChange.Id, newPartialLineItem);
				});

				var buyerOrder = await _oc.Orders.GetAsync<HsOrder>(OrderDirection.Incoming, buyerOrderId);
				var allLineItemsForOrder = await _oc.LineItems.ListAllAsync<HsLineItem>(OrderDirection.Incoming, buyerOrderId);
				var lineItemsChanged = allLineItemsForOrder.Where(li => lineItemStatusChanges.Changes.Select(li => li.Id).Contains(li.ID)).ToList();
				var sellerIDsRelatingToChange = lineItemsChanged.Select(li => li.SupplierID).Distinct().ToList();
				if (lineItemStatusChanges.Status == LineItemStatus.CancelRequested || lineItemStatusChanges.Status == LineItemStatus.ReturnRequested)
				{
					await _rmaCommand.BuildRMA(buyerOrder, sellerIDsRelatingToChange, lineItemStatusChanges, lineItemsChanged, decodedToken);
				}

				var supplierIDsRelatingToChange = sellerIDsRelatingToChange.Where(s => s != null).ToList(); // filter out MPO
				var relatedSupplierOrderIds = (userType == "admin") ? null : supplierIDsRelatingToChange.Select(supplierID => supplierID == null ? supplierID : $@"{buyerOrderId}-{supplierID}").ToList();
				var statusSync = SyncOrderStatuses(buyerOrder, relatedSupplierOrderIds, allLineItemsForOrder.ToList());
				await statusSync;

				var notificationSender = HandleLineItemStatusChangeNotification(verifiedUserType, buyerOrder, supplierIDsRelatingToChange, lineItemsChanged, lineItemStatusChanges);
				await notificationSender;
				resp = updatedLineItems.ToList();
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Private re-usable SyncOrderStatuses task method
		/// </summary>
		/// <param name="buyerOrder"></param>
		/// <param name="relatedSupplierOrderIds"></param>
		/// <param name="allOrderLineItems"></param>
		/// <returns></returns>
		private async Task SyncOrderStatuses(HsOrder buyerOrder, List<string> relatedSupplierOrderIds, List<HsLineItem> allOrderLineItems)
		{
			try
			{
				await SyncOrderStatus(OrderDirection.Incoming, buyerOrder.ID, allOrderLineItems);
				if (relatedSupplierOrderIds != null)
				{
					foreach (var supplierOrderId in relatedSupplierOrderIds)
					{
						var supplierId = supplierOrderId.Split('-')[1];
						var allOrderLineItemsForSupplierOrder = allOrderLineItems.Where(li => li.SupplierID == supplierId).ToList();
						await SyncOrderStatus(OrderDirection.Outgoing, supplierOrderId, allOrderLineItemsForSupplierOrder);
					}
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Private re-usable SyncOrderStatus task method
		/// </summary>
		/// <param name="orderDirection"></param>
		/// <param name="orderID"></param>
		/// <param name="changedLineItems"></param>
		/// <returns></returns>
		private async Task SyncOrderStatus(OrderDirection orderDirection, string orderID, List<HsLineItem> changedLineItems)
		{
			try
			{
				var (SubmittedOrderStatus, ShippingStatus, ClaimStatus) = LineItemStatusConstants.GetOrderStatuses(changedLineItems);
				var partialOrder = new PartialOrder()
				{
					xp = new
					{
						SubmittedOrderStatus,
						ShippingStatus,
						ClaimStatus
					}
				};
				await _oc.Orders.PatchAsync(orderDirection, orderID, partialOrder);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Private re-usable BuildNewPartialLineItem method
		/// </summary>
		/// <param name="lineItemStatusChange"></param>
		/// <param name="previousLineItemStates"></param>
		/// <param name="newLineItemStatus"></param>
		/// <returns>The PartialLineItem response object from the BuildNewPartialLineItem process</returns>
		private PartialLineItem BuildNewPartialLineItem(LineItemStatusChange lineItemStatusChange, List<HsLineItem> previousLineItemStates, LineItemStatus newLineItemStatus)
		{
			var resp = new PartialLineItem();
			try
			{
				var existingLineItem = previousLineItemStates.First(li => li.ID == lineItemStatusChange.Id);
				var StatusByQuantity = BuildNewLineItemStatusByQuantity(lineItemStatusChange, existingLineItem, newLineItemStatus);
				if (newLineItemStatus == LineItemStatus.ReturnRequested || newLineItemStatus == LineItemStatus.Returned)
				{
					var returnRequests = existingLineItem.xp.Returns ?? new List<LineItemClaim>();
					resp = new PartialLineItem()
					{
						xp = new
						{
							Returns = GetUpdatedChangeRequests(returnRequests, lineItemStatusChange, lineItemStatusChange.Quantity, newLineItemStatus, StatusByQuantity),
							StatusByQuantity
						}
					};
				}
				else if (newLineItemStatus == LineItemStatus.CancelRequested || newLineItemStatus == LineItemStatus.Canceled)
				{
					var cancelRequests = existingLineItem.xp.Cancellations ?? new List<LineItemClaim>();
					resp = new PartialLineItem()
					{
						xp = new
						{
							Cancelations = GetUpdatedChangeRequests(cancelRequests, lineItemStatusChange, lineItemStatusChange.Quantity, newLineItemStatus, StatusByQuantity),
							StatusByQuantity
						}
					};
				}
				else
				{
					resp = new PartialLineItem()
					{
						xp = new
						{
							StatusByQuantity
						}
					};
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;            
		}

		/// <summary>
		/// Private re-usable GetUpdatedChangeRequests method
		/// </summary>
		/// <param name="existinglineItemStatusChangeRequests"></param>
		/// <param name="lineItemStatusChange"></param>
		/// <param name="QuantitySetting"></param>
		/// <param name="newLineItemStatus"></param>
		/// <param name="lineItemStatuses"></param>
		/// <returns>The list of LineItemClaim response objects from the GetUpdatedChangeRequests process</returns>
		private List<LineItemClaim> GetUpdatedChangeRequests(List<LineItemClaim> existinglineItemStatusChangeRequests, LineItemStatusChange lineItemStatusChange, int QuantitySetting, LineItemStatus newLineItemStatus, Dictionary<LineItemStatus, int> lineItemStatuses)
		{
			try
			{
				if (newLineItemStatus == LineItemStatus.Returned || newLineItemStatus == LineItemStatus.Canceled)
				{
					// go through the return requests and resolve each request until there aren't enough returned or canceled items 
					// to resolve an additional request
					var numberReturnedOrCanceled = lineItemStatuses[newLineItemStatus];
					var currentClaimIndex = 0;
					while (numberReturnedOrCanceled > 0 && currentClaimIndex < existinglineItemStatusChangeRequests.Count())
					{
						if (existinglineItemStatusChangeRequests[currentClaimIndex].Quantity <= numberReturnedOrCanceled)
						{
							existinglineItemStatusChangeRequests[currentClaimIndex].IsResolved = true;
							numberReturnedOrCanceled -= existinglineItemStatusChangeRequests[currentClaimIndex].Quantity;
							currentClaimIndex++;
						}
						else
						{
							currentClaimIndex++;
						}
					}
				}
				else
				{
					existinglineItemStatusChangeRequests.Add(new LineItemClaim()
					{
						Comment = lineItemStatusChange.Comment,
						Reason = lineItemStatusChange.Reason,
						IsResolved = false,
						Quantity = QuantitySetting
					});
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return existinglineItemStatusChangeRequests;
		}

		/// <summary>
		/// Private re-usable BuildNewLineItemStatusByQuantity method
		/// </summary>
		/// <param name="lineItemStatusChange"></param>
		/// <param name="existingLineItem"></param>
		/// <param name="newLineItemStatus"></param>
		/// <returns>The Dictionary response object from the BuildNewLineItemStatusByQuantity process</returns>
		private Dictionary<LineItemStatus, int> BuildNewLineItemStatusByQuantity(LineItemStatusChange lineItemStatusChange, HsLineItem existingLineItem, LineItemStatus newLineItemStatus)
		{
			var statusDictionary = new Dictionary<LineItemStatus, int>();
			try
			{
				statusDictionary = existingLineItem.xp.StatusByQuantity;
				var quantitySetting = lineItemStatusChange.Quantity;

				// increment
				statusDictionary[newLineItemStatus] += quantitySetting;
				var validPreviousStates = LineItemStatusConstants.ValidPreviousStateLineItemChangeMap[newLineItemStatus];

				// decrement
				foreach (LineItemStatus status in validPreviousStates)
				{
					if (statusDictionary[status] != 0)
					{
						if (statusDictionary[status] <= quantitySetting)
						{
							quantitySetting -= statusDictionary[status];
							statusDictionary[status] = 0;
						}
						else
						{
							statusDictionary[status] -= quantitySetting;
							quantitySetting = 0;
						}
					}
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return statusDictionary;
		}

		/// <summary>
		/// Private re-usable HandleLineItemStatusChangeNotification task method
		/// </summary>
		/// <param name="setterUserType"></param>
		/// <param name="buyerOrder"></param>
		/// <param name="supplierIDsRelatedToChange"></param>
		/// <param name="lineItemsChanged"></param>
		/// <param name="lineItemStatusChanges"></param>
		/// <returns></returns>
		private async Task HandleLineItemStatusChangeNotification(VerifiedUserType setterUserType, HsOrder buyerOrder, List<string> supplierIdsRelatedToChange, List<HsLineItem> lineItemsChanged, LineItemStatusChanges lineItemStatusChanges)
		{
			try
			{
				var suppliers = await Throttler.RunAsync(supplierIdsRelatedToChange, 100, 5, supplierID => _oc.Suppliers.GetAsync<HsSupplier>(supplierID));

				// currently the only place supplier name is used is when there should be lineitems from only one supplier included on the change, so we can just take the first supplier
				var statusChangeTextDictionary = LineItemStatusConstants.GetStatusChangeEmailText(suppliers.First().Name);

				foreach (KeyValuePair<VerifiedUserType, EmailDisplayText> entry in statusChangeTextDictionary[lineItemStatusChanges.Status])
				{
					var userType = entry.Key;
					var emailText = entry.Value;
					var firstName = string.Empty;
					var lastName = string.Empty;
					var email = string.Empty;

					if (userType == VerifiedUserType.buyer)
					{
						firstName = buyerOrder.FromUser.FirstName;
						lastName = buyerOrder.FromUser.LastName;
						email = buyerOrder.FromUser.Email;
						await _sendgridService.SendLineItemStatusChangeEmail(buyerOrder, lineItemStatusChanges, lineItemsChanged.ToList(), firstName, lastName, email, emailText);
					}
					else if (userType == VerifiedUserType.admin)
					{
						// Loop over seller users, pull out THEIR boolean, as well as the List<string> of AddtlRcpts
						var sellerUsers = await _oc.AdminUsers.ListAsync<HsSellerUser>();
						var tos = new List<EmailAddress>();
						foreach (var seller in sellerUsers.Items)
						{
							if (seller?.xp?.OrderEmails ?? false)
							{
								tos.Add(new EmailAddress(seller.Email));
							};
							if (seller?.xp?.AddtlRcpts?.Any() ?? false)
							{
								foreach (var rcpt in seller.xp.AddtlRcpts)
								{
									tos.Add(new EmailAddress(rcpt));
								};
							};
						};

						var shouldNotify = !(LineItemStatusConstants.LineItemStatusChangesDontNotifySetter.Contains(lineItemStatusChanges.Status) && setterUserType == VerifiedUserType.admin);
						if (shouldNotify)
						{
							await _sendgridService.SendLineItemStatusChangeEmailMultipleRcpts(buyerOrder, lineItemStatusChanges, lineItemsChanged.ToList(), tos, emailText);
						}
					}
					else
					{
						var shouldNotify = !(LineItemStatusConstants.LineItemStatusChangesDontNotifySetter.Contains(lineItemStatusChanges.Status) && setterUserType == VerifiedUserType.supplier);
						if (shouldNotify)
						{
							await Throttler.RunAsync(suppliers, 100, 5, async supplier =>
							{
								if (supplier?.xp?.NotificationRcpts?.Any() ?? false)
								{
									var tos = new List<EmailAddress>();
									foreach (var rcpt in supplier.xp.NotificationRcpts)
									{
										tos.Add(new EmailAddress(rcpt));
									};
									await _sendgridService.SendLineItemStatusChangeEmailMultipleRcpts(buyerOrder, lineItemStatusChanges, lineItemsChanged.ToList(), tos, emailText);
								}
							});
						}
					}
				}
			} 
			catch (Exception ex)
			{
				// track in app insights
				// to find go to Transaction Search > Event Type = Event > Filter by any of these custom properties or event name "Email.LineItemEmailFailed"
				var customProperties = new Dictionary<string, string>
				{
					{ $@"Message", $@"The Attempt to email line item changes failed." },
					{ $@"BuyerOrderID", $@"{buyerOrder.ID.Trim()}" },
					{ $@"BuyerID", $@"{buyerOrder.FromCompanyID.Trim()}" },
					{ $@"UserEmail", $@"{buyerOrder.FromUser.Email.Trim()}" },
					{ $@"UserType", $@"{setterUserType.ToString().Trim()}" },
					{ $@"ErrorResponse", JsonConvert.SerializeObject(ex.Message, Formatting.Indented)}
				};
				_telemetry.TrackEvent("Email.LineItemEmailFailed", customProperties);
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
				return;
			}
		}

		/// <summary>
		/// Private re-usable ValidateLineItemStatusChange task method
		/// </summary>
		/// <param name="previousLineItemStates"></param>
		/// <param name="lineItemStatusChanges"></param>
		/// <param name="userType"></param>
		private void ValidateLineItemStatusChange(List<HsLineItem> previousLineItemStates, LineItemStatusChanges lineItemStatusChanges, VerifiedUserType userType)
		{
			try
			{
				/* need to validate 2 things on a lineitem status change
				 * 1) user making the request has the ability to make that line item change based on usertype
				 * 2) there are sufficient amount of the previous quantities for each lineitem
				*/

				// 1) 
				var allowedLineItemStatuses = LineItemStatusConstants.ValidLineItemStatusSetByUserType[userType];
				Require.That(allowedLineItemStatuses.Contains(lineItemStatusChanges.Status), new ErrorCode($@"Not authorized to set this status on a lineItem", $@"This is Not authorized to set line items to {lineItemStatusChanges.Status}."));

				// 2)
				var areCurrentQuantitiesToSupportChange = lineItemStatusChanges.Changes.All(lineItemChange =>
				{
					return ValidateCurrentQuantities(previousLineItemStates, lineItemChange, lineItemStatusChanges.Status);
				});
				Require.That(areCurrentQuantitiesToSupportChange, new ErrorCode($@"Invalid lineItem status change", $@"The Current lineitem quantity statuses on the order are not sufficient to support the requested change."));
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Public re-usable ValidateCurrentQuantities method
		/// </summary>
		/// <param name="previousLineItemStates"></param>
		/// <param name="lineItemStatusChange"></param>
		/// <param name="lineItemStatusChangingTo"></param>
		/// <returns>The boolean status for the ValidateCurrentQuantities process</returns>
		public bool ValidateCurrentQuantities(List<HsLineItem> previousLineItemStates, LineItemStatusChange lineItemStatusChange, LineItemStatus lineItemStatusChangingTo)
		{
			var countCanBeChanged = 0;
			try
			{
				var relatedLineItems = previousLineItemStates.Where(previousState => previousState.ID == lineItemStatusChange.Id);
				if (relatedLineItems.Count() != 1)
				{
					// if the lineitem is not found on the order, invalid change
					return false;
				}

				var existingLineItem = relatedLineItems.First();
				var existingStatusByQuantity = existingLineItem.xp.StatusByQuantity;
				if (existingStatusByQuantity == null)
				{
					return false;
				}

				var validPreviousStates = LineItemStatusConstants.ValidPreviousStateLineItemChangeMap[lineItemStatusChangingTo];
				foreach (KeyValuePair<LineItemStatus, int> entry in existingStatusByQuantity)
				{
					if (validPreviousStates.Contains(entry.Key))
					{
						countCanBeChanged += entry.Value;
					}
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return (countCanBeChanged >= lineItemStatusChange.Quantity);
		}

		/// <summary>
		/// Public re-usable ValidateCurrentQuantities task method
		/// </summary>
		/// <param name="orderID"></param>
		/// <param name="liReq"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The newly updated HsLineItem response object from the UpsertLineItem process</returns>
		public async Task<HsLineItem> UpsertLineItem(string orderID, HsLineItem liReq, DecodedToken decodedToken)
		{
			var li = new HsLineItem();
			try
			{
				// get me product with markedup prices correct currency and the existing line items in parellel
				var productRequest = _meProductCommand.Get(liReq.ProductID, decodedToken);
				var existingLineItemsRequest = _oc.LineItems.ListAllAsync<HsLineItem>(OrderDirection.Outgoing, orderID, filters: $"Product.ID={liReq.ProductID}", accessToken: decodedToken.AccessToken);
				var orderRequest = _oc.Orders.GetAsync(OrderDirection.Incoming, orderID);
				await Task.WhenAll(productRequest, existingLineItemsRequest, orderRequest);

				var existingLineItems = await existingLineItemsRequest;
				var product = await productRequest;
				var order = await orderRequest;
				var markedUpPrice = ValidateLineItemUnitCost(orderID, product, existingLineItems, liReq);

				liReq.UnitPrice = liReq.Product != null ? liReq.UnitPrice : await markedUpPrice;
				Require.That(!order.IsSubmitted, new ErrorCode($@"Invalid Order Status", "The Order has already been submitted."));
				liReq.xp.StatusByQuantity = LineItemStatusConstants.EmptyStatuses;
				liReq.xp.StatusByQuantity[LineItemStatus.Open] = liReq.Quantity;

				var preExistingLi = ((List<HsLineItem>)existingLineItems).Find(eli => LineItemsMatch(eli, liReq));
				if (preExistingLi != null)
				{
					liReq.ID = preExistingLi.ID; //ensure we do not change the line item id when updating
					li = await _oc.LineItems.SaveAsync<HsLineItem>(OrderDirection.Incoming, orderID, preExistingLi.ID, liReq);
				}
				else
				{
					li = await _oc.LineItems.CreateAsync<HsLineItem>(OrderDirection.Incoming, orderID, liReq);
				}
				await _promotionCommand.AutoApplyPromotions(orderID);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return li;
		}

		/// <summary>
		/// Public re-usable DeleteLineItem task method
		/// </summary>
		/// <param name="orderID"></param>
		/// <param name="lineItemId"></param>
		/// <param name="decodedToken"></param>
		/// <returns></returns>
		public async Task DeleteLineItem(string orderID, string lineItemId, DecodedToken decodedToken)
		{
			try
			{
				LineItem lineItem = await _oc.LineItems.GetAsync(OrderDirection.Incoming, orderID, lineItemId);
				await _oc.LineItems.DeleteAsync(OrderDirection.Incoming, orderID, lineItemId);
				List<HsLineItem> existingLineItems = await _oc.LineItems.ListAllAsync<HsLineItem>(OrderDirection.Outgoing, orderID, filters: $@"Product.ID={lineItem.ProductID}", accessToken: decodedToken.AccessToken);
				if (existingLineItems != null && existingLineItems.Count > 0)
				{
					var product = await _meProductCommand.Get(lineItem.ProductID, decodedToken);
					await ValidateLineItemUnitCost(orderID, product, existingLineItems, null);
				}
				await _promotionCommand.AutoApplyPromotions(orderID);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Public re-usable ValidateLineItemUnitCost task method
		/// </summary>
		/// <param name="orderID"></param>
		/// <param name="product"></param>
		/// <param name="existingLineItems"></param>
		/// <param name="li"></param>
		/// <returns>The lineItemTotal decimal value from the ValidateLineItemUnitCost process</returns>
		public async Task<decimal> ValidateLineItemUnitCost(string orderID, SuperHsMeProduct product, List<HsLineItem> existingLineItems, HsLineItem li)
		{
			decimal respLineItemTotal = 0;
			try
			{
				if (product.PriceSchedule.UseCumulativeQuantity)
				{
					var totalQuantity = li?.Quantity ?? 0;
					foreach (var lineItem in existingLineItems)
					{
						if (li == null || !LineItemsMatch(li, lineItem))
						{
							totalQuantity += lineItem.Quantity;
						}
					}

					var selectedPriceBreak = product.PriceSchedule.PriceBreaks.Last(priceBreak => priceBreak.Quantity <= totalQuantity);
					decimal priceBasedOnQuantity = product.PriceSchedule.IsOnSale ? (decimal)selectedPriceBreak.SalePrice : selectedPriceBreak.Price;
					var tasks = new List<Task>();
					foreach (var lineItem in existingLineItems)
					{
						// Determine markup for all specs for this existing line item
						selectedPriceBreak = product.PriceSchedule.PriceBreaks.Last(priceBreak => priceBreak.Quantity <= li.Quantity);
						priceBasedOnQuantity = product.PriceSchedule.IsOnSale ? (decimal)selectedPriceBreak.SalePrice : selectedPriceBreak.Price;
						// Determine markup for the 1 line item
						var lineItemTotal = priceBasedOnQuantity + GetSpecMarkup(lineItem.Specs, product.Specs);
						if (lineItem.UnitPrice != lineItemTotal)
						{
							PartialLineItem lineItemToPatch = new PartialLineItem();
							lineItemToPatch.UnitPrice = lineItemTotal;
							tasks.Add(_oc.LineItems.PatchAsync<HsLineItem>(OrderDirection.Incoming, orderID, lineItem.ID, lineItemToPatch));
						}
					}
					await Task.WhenAll(tasks);
					// Return the item total for the li being added or modified
					return (li == null) ? 0 : priceBasedOnQuantity + GetSpecMarkup(li.Specs, product.Specs);
				}

				if (li != null)
				{
					// Determine price including quantity price break discount
					var priceBasedOnQuantity = product.PriceSchedule.PriceBreaks.Last(priceBreak => priceBreak.Quantity <= li.Quantity).Price;
					// Determine markup for the 1 line item
					respLineItemTotal = (priceBasedOnQuantity + GetSpecMarkup(li.Specs, product.Specs));
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return respLineItemTotal;
		}

		/// <summary>
		/// Public re-usable HandleRMALineItemStatusChanges method
		/// </summary>
		/// <param name="orderDirection"></param>
		/// <param name="rmaWithLineItemStatusByQuantity"></param>
		/// <param name="decodedToken"></param>
		/// <returns></returns>
		public async Task HandleRMALineItemStatusChanges(OrderDirection orderDirection, RMAWithLineItemStatusByQuantity rmaWithLineItemStatusByQuantity, DecodedToken decodedToken)
		{
			try
			{
				var orderId = string.Empty;
				if (!rmaWithLineItemStatusByQuantity.LineItemStatusChangesList.Any())
				{
					return;
				}                
				if (rmaWithLineItemStatusByQuantity.RMA.SupplierId == null)
				{
					// This is an MPO owned RMA
					orderId = rmaWithLineItemStatusByQuantity.RMA.SourceOrderId;
				}
				else
				{
					// This is a suplier owned RMA and by convention orders are in the format {orderID}-{supplierID}
					orderId = $"{rmaWithLineItemStatusByQuantity.RMA.SourceOrderId}-{rmaWithLineItemStatusByQuantity.RMA.SupplierId}";
				}

				foreach (var statusChange in rmaWithLineItemStatusByQuantity.LineItemStatusChangesList)
				{
					await UpdateLineItemStatusesAndNotifyIfApplicable(OrderDirection.Incoming, orderId, statusChange, decodedToken);
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Private re-usable GetSpecMarkup method
		/// </summary>
		/// <param name="lineItemSpecs"></param>
		/// <param name="productSpecs"></param>
		/// <returns>The lineItemTotal decimal value from the GetSpecMarkup process</returns>
		private decimal GetSpecMarkup(IList<LineItemSpec> lineItemSpecs, IList<Spec> productSpecs)
		{
			decimal lineItemTotal = 0;
			try
			{
				lineItemTotal = lineItemSpecs.Aggregate(0M, (accumulator, spec) =>
				{
					var relatedProductSpec = productSpecs.FirstOrDefault(productSpec => productSpec.ID == spec.SpecID);
					decimal? relatedSpecMarkup = 0;
					if (relatedProductSpec != null && relatedProductSpec.Options.HasItem())
					{
						relatedSpecMarkup = relatedProductSpec.Options?.FirstOrDefault(option => option.ID == spec.OptionID)?.PriceMarkup;
					}
					return accumulator + (relatedSpecMarkup ?? 0M);
				});
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return lineItemTotal;
		}

		/// <summary>
		/// Private re-usable LineItemsMatch method
		/// </summary>
		/// <param name="li1"></param>
		/// <param name="li2"></param>
		/// <returns>The boolean status for the LineItemsMatch process</returns>
		private bool LineItemsMatch(LineItem li1, LineItem li2)
		{
			var resp = false;
			try
			{
				if (li1.ProductID != li2.ProductID)
				{
					return false;
				}
				if (!string.IsNullOrEmpty(li2.xp.PrintArtworkURL))
				{
					if (li2.xp.PrintArtworkURL != li1.xp.PrintArtworkURL)
					{
						return false;
					}
				}

				foreach (var spec1 in li1.Specs)
				{
					var spec2 = (li2.Specs as List<LineItemSpec>)?.Find(s => s.SpecID == spec1.SpecID);
					if (spec1?.Value != spec2?.Value)
					{
						return false;
					}
				}
				resp = true;           
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}
	}
}