using Headstart.Common.Constants;
using Headstart.Common.Extensions;
using Headstart.Common.Services;
using Headstart.Common.Models;
using Headstart.Models;
using Headstart.Models.Extended;
using Headstart.Models.Headstart;
using Microsoft.ApplicationInsights;
using Newtonsoft.Json;
using ordercloud.integrations.library;
using OrderCloud.Catalyst;
using OrderCloud.SDK;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Headstart.API.Commands
{
    public interface ILineItemCommand
    {
        Task<HSLineItem> UpsertLineItem(string orderID, HSLineItem li, VerifiedUserContext verifiedUser);
        Task<List<HSLineItem>> UpdateLineItemStatusesAndNotifyIfApplicable(OrderDirection orderDirection, string orderID, LineItemStatusChanges lineItemStatusChanges, VerifiedUserContext verifiedUser = null);
        Task<List<HSLineItem>> SetInitialSubmittedLineItemStatuses(string buyerOrderID);
        Task DeleteLineItem(string orderID, string lineItemID, VerifiedUserContext verifiedUser);
        Task<decimal> ValidateLineItemUnitCost(string orderID, SuperHSMeProduct product, List<HSLineItem> existingLineItems, HSLineItem li);
        Task HandleRMALineItemStatusChanges(OrderDirection orderDirection, RMAWithLineItemStatusByQuantity rmaWithLineItemStatusByQuantity, VerifiedUserContext verifiedUserContext);
    }

    public class LineItemCommand : ILineItemCommand
    {
        private readonly IOrderCloudClient _oc;
        private readonly ISendgridService _sendgridService;
        private readonly IMeProductCommand _meProductCommand;
        private readonly IPromotionCommand _promotionCommand;
        private readonly IRMACommand _rmaCommand;

        private readonly TelemetryClient _telemetry;

        public LineItemCommand(ISendgridService sendgridService, IOrderCloudClient oc, IMeProductCommand meProductCommand, IPromotionCommand promotionCommand, IRMACommand rmaCommand, TelemetryClient telemetry)
        {
			_oc = oc;
            _sendgridService = sendgridService;
            _meProductCommand = meProductCommand;
            _promotionCommand = promotionCommand;
            _rmaCommand = rmaCommand;
            _telemetry = telemetry;
        }

        // used on post order submit
        public async Task<List<HSLineItem>> SetInitialSubmittedLineItemStatuses(string buyerOrderID)
        {
            var lineItems = await _oc.LineItems.ListAllAsync<HSLineItem>(OrderDirection.Incoming, buyerOrderID);
            var updatedLineItems = await Throttler.RunAsync(lineItems, 100, 5, li =>
            {
                var partial = new PartialLineItem()
                {
                    xp = new
                    {
                        StatusByQuantity = new Dictionary<LineItemStatus, int>() {
                            { LineItemStatus.Submitted, li.Quantity },
                            { LineItemStatus.Open, 0 },
                            { LineItemStatus.Backordered, 0 },
                            { LineItemStatus.Canceled, 0 },
                            { LineItemStatus.CancelRequested, 0 },
                            { LineItemStatus.CancelDenied, 0 },
                            { LineItemStatus.Returned, 0 },
                            { LineItemStatus.ReturnRequested, 0 },
                            { LineItemStatus.ReturnDenied, 0 },
                            { LineItemStatus.Complete, 0 }
                        },
                        Returns = new List<LineItemClaim>(),
                        Cancelations = new List<LineItemClaim>()
                    }
                };
                return _oc.LineItems.PatchAsync<HSLineItem>(OrderDirection.Incoming, buyerOrderID, li.ID, partial);
            });
            return updatedLineItems.ToList();
        }

        /// <summary>
        /// Validates LineItemStatus Change, Updates Line Item Statuses, Updates Order Statuses, Sends Necessary Emails
        /// </summary>

        // all line item status changes should go through here
        public async Task<List<HSLineItem>> UpdateLineItemStatusesAndNotifyIfApplicable(OrderDirection orderDirection, string orderID, LineItemStatusChanges lineItemStatusChanges, VerifiedUserContext verifiedUser = null)
        {
            var userType = verifiedUser?.UserType ?? "noUser";
            var verifiedUserType = userType.Reserialize<VerifiedUserType>();
            
            var buyerOrderID = orderID.Split('-')[0];
            var previousLineItemsStates = await _oc.LineItems.ListAllAsync<HSLineItem>(OrderDirection.Incoming, buyerOrderID);

            ValidateLineItemStatusChange(previousLineItemsStates.ToList(), lineItemStatusChanges, verifiedUserType);
            var updatedLineItems = await Throttler.RunAsync(lineItemStatusChanges.Changes, 100, 5, (lineItemStatusChange) =>
            {
                var newPartialLineItem = BuildNewPartialLineItem(lineItemStatusChange, previousLineItemsStates.ToList(), lineItemStatusChanges.Status);
               // if there is no verified user passed in it has been called from somewhere else in the code base and will be done with the client grant access
               return verifiedUser != null ? _oc.LineItems.PatchAsync<HSLineItem>(orderDirection, orderID, lineItemStatusChange.ID, newPartialLineItem, verifiedUser.AccessToken) : _oc.LineItems.PatchAsync<HSLineItem>(orderDirection, orderID, lineItemStatusChange.ID, newPartialLineItem);
            });

            var buyerOrder = await _oc.Orders.GetAsync<HSOrder>(OrderDirection.Incoming, buyerOrderID);
            var allLineItemsForOrder = await  _oc.LineItems.ListAllAsync<HSLineItem>(OrderDirection.Incoming, buyerOrderID);
            var lineItemsChanged = allLineItemsForOrder.Where(li => lineItemStatusChanges.Changes.Select(li => li.ID).Contains(li.ID)).ToList();
            var supplierIDsRelatingToChange = lineItemsChanged.Select(li => li.SupplierID).Distinct().Where(id => id != null).ToList();

            var relatedSupplierOrderIDs = (userType == "admin") ? null : supplierIDsRelatingToChange.Select(supplierID => $"{buyerOrderID}-{supplierID}").ToList();

            if (lineItemStatusChanges.Status == LineItemStatus.CancelRequested || lineItemStatusChanges.Status == LineItemStatus.ReturnRequested)
            {
                await _rmaCommand.BuildRMA(buyerOrder, supplierIDsRelatingToChange, lineItemStatusChanges, lineItemsChanged, verifiedUser);
            }

            var statusSync = SyncOrderStatuses(buyerOrder, relatedSupplierOrderIDs, allLineItemsForOrder.ToList());
            await statusSync;

            var notifictionSender = HandleLineItemStatusChangeNotification(verifiedUserType, buyerOrder, supplierIDsRelatingToChange, lineItemsChanged, lineItemStatusChanges);
            await notifictionSender;

            return updatedLineItems.ToList();
        }

        private async Task SyncOrderStatuses(HSOrder buyerOrder, List<string> relatedSupplierOrderIDs, List<HSLineItem> allOrderLineItems)
        {
            await SyncOrderStatus(OrderDirection.Incoming, buyerOrder.ID, allOrderLineItems);

            if(relatedSupplierOrderIDs != null)
            {
                foreach (var supplierOrderID in relatedSupplierOrderIDs)
                {
                    var supplierID = supplierOrderID.Split('-')[1];
                    var allOrderLineItemsForSupplierOrder = allOrderLineItems.Where(li => li.SupplierID == supplierID).ToList();
                    await SyncOrderStatus(OrderDirection.Outgoing, supplierOrderID, allOrderLineItemsForSupplierOrder);
                }
            }            
        }

        private async Task SyncOrderStatus(OrderDirection orderDirection, string orderID, List<HSLineItem> changedLineItems)
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

        private PartialLineItem BuildNewPartialLineItem(LineItemStatusChange lineItemStatusChange, List<HSLineItem> previousLineItemStates, LineItemStatus newLineItemStatus)
        {
            var existingLineItem = previousLineItemStates.First(li => li.ID == lineItemStatusChange.ID);
            var StatusByQuantity = BuildNewLineItemStatusByQuantity(lineItemStatusChange, existingLineItem, newLineItemStatus);
            if (newLineItemStatus == LineItemStatus.ReturnRequested || newLineItemStatus == LineItemStatus.Returned)
            {
                var returnRequests = existingLineItem.xp.Returns ?? new List<LineItemClaim>();
                return new PartialLineItem()
                {
                    xp = new
                    {
                        Returns = GetUpdatedChangeRequests(returnRequests, lineItemStatusChange, lineItemStatusChange.Quantity, newLineItemStatus, StatusByQuantity),
                        StatusByQuantity
                    }
                };
            } else if(newLineItemStatus == LineItemStatus.CancelRequested || newLineItemStatus == LineItemStatus.Canceled)
            {
                var cancelRequests = existingLineItem.xp.Cancelations ?? new List<LineItemClaim>();
                return new PartialLineItem()
                {
                    xp = new
                    {
                        Cancelations = GetUpdatedChangeRequests(cancelRequests, lineItemStatusChange, lineItemStatusChange.Quantity, newLineItemStatus, StatusByQuantity),
                        StatusByQuantity
                    }
                };
            } else
            {
                return new PartialLineItem()
                {
                    xp = new
                    {
                        StatusByQuantity
                    }
                };
            }
        }

        private List<LineItemClaim> GetUpdatedChangeRequests(List<LineItemClaim> existinglineItemStatusChangeRequests, LineItemStatusChange lineItemStatusChange, int QuantitySetting, LineItemStatus newLineItemStatus, Dictionary<LineItemStatus, int> lineItemStatuses)
        {
            if(newLineItemStatus == LineItemStatus.Returned || newLineItemStatus == LineItemStatus.Canceled) 
            {
                // go through the return requests and resolve each request until there aren't enough returned or canceled items 
                // to resolve an additional request
                var numberReturnedOrCanceled = lineItemStatuses[newLineItemStatus];
                var currentClaimIndex = 0;
                while (numberReturnedOrCanceled > 0 && currentClaimIndex < existinglineItemStatusChangeRequests.Count()) { 
                    if(existinglineItemStatusChangeRequests[currentClaimIndex].Quantity <= numberReturnedOrCanceled)
                    {
                        existinglineItemStatusChangeRequests[currentClaimIndex].IsResolved = true;
                        numberReturnedOrCanceled -= existinglineItemStatusChangeRequests[currentClaimIndex].Quantity;
                        currentClaimIndex++;
                    } else
                    {
                        currentClaimIndex++;
                    }
                }
            } else
            {
                existinglineItemStatusChangeRequests.Add(new LineItemClaim()
                {
                    Comment = lineItemStatusChange.Comment,
                    Reason = lineItemStatusChange.Reason,
                    IsResolved = false,
                    Quantity = QuantitySetting
                });

            }
            
            return existinglineItemStatusChangeRequests;
        }

        private Dictionary<LineItemStatus, int> BuildNewLineItemStatusByQuantity(LineItemStatusChange lineItemStatusChange, HSLineItem existingLineItem, LineItemStatus newLineItemStatus)
        {
            Dictionary<LineItemStatus, int> statusDictionary = existingLineItem.xp.StatusByQuantity;
            var quantitySetting = lineItemStatusChange.Quantity;

            // increment
            statusDictionary[newLineItemStatus] += quantitySetting;


            var validPreviousStates = LineItemStatusConstants.ValidPreviousStateLineItemChangeMap[newLineItemStatus];

            // decrement
            foreach(LineItemStatus status in validPreviousStates)
            {
                if(statusDictionary[status] != 0)
                {
                    if(statusDictionary[status] <= quantitySetting)
                    {
                        quantitySetting -= statusDictionary[status];
                        statusDictionary[status] = 0;
                    } else
                    {
                        statusDictionary[status] -= quantitySetting;
                        quantitySetting = 0;
                    }
                }
            }


            return statusDictionary;
        }

        private async Task HandleLineItemStatusChangeNotification(VerifiedUserType setterUserType, HSOrder buyerOrder, List<string> supplierIDsRelatedToChange, List<HSLineItem> lineItemsChanged, LineItemStatusChanges lineItemStatusChanges)
        {
            try
            {
                var suppliers = await Throttler.RunAsync(supplierIDsRelatedToChange, 100, 5, supplierID => _oc.Suppliers.GetAsync<HSSupplier>(supplierID));

                // currently the only place supplier name is used is when there should be lineitems from only one supplier included on the change, so we can just take the first supplier
                var statusChangeTextDictionary = LineItemStatusConstants.GetStatusChangeEmailText(suppliers.First().Name);

                foreach (KeyValuePair<VerifiedUserType, EmailDisplayText> entry in statusChangeTextDictionary[lineItemStatusChanges.Status])
                {
                    var userType = entry.Key;
                    var emailText = entry.Value;

                    var firstName = "";
                    var lastName = "";
                    var email = "";

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
                        var sellerUsers = await _oc.AdminUsers.ListAsync<HSSellerUser>();
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
                    { "Message", "Attempt to email line item changes failed" },
                    { "BuyerOrderID", buyerOrder.ID },
                    { "BuyerID", buyerOrder.FromCompanyID },
                    { "UserEmail", buyerOrder.FromUser.Email },
                    { "UserType", setterUserType.ToString() },
                    { "ErrorResponse", JsonConvert.SerializeObject(ex.Message, Newtonsoft.Json.Formatting.Indented)}
                };
                _telemetry.TrackEvent("Email.LineItemEmailFailed", customProperties);
                return;
            }
          
        }

        private void ValidateLineItemStatusChange(List<HSLineItem> previousLineItemStates, LineItemStatusChanges lineItemStatusChanges, VerifiedUserType userType)
        {
            /* need to validate 3 things on a lineitem status change
             * 
             * 1) user making the request has the ability to make that line item change based on usertype
             * 2) there are sufficient amount of the previous quantities for each lineitem
             */

            // 1) 
            var allowedLineItemStatuses = LineItemStatusConstants.ValidLineItemStatusSetByUserType[userType];
            Require.That(allowedLineItemStatuses.Contains(lineItemStatusChanges.Status), new ErrorCode("Not authorized to set this status on a lineItem", 400, $"Not authorized to set line items to {lineItemStatusChanges.Status}"));

            // 2)
            var areCurrentQuantitiesToSupportChange = lineItemStatusChanges.Changes.All(lineItemChange =>
            {
                return ValidateCurrentQuantities(previousLineItemStates, lineItemChange, lineItemStatusChanges.Status);
            });
            Require.That(areCurrentQuantitiesToSupportChange, new ErrorCode("Invalid lineItem status change", 400, $"Current lineitem quantity statuses on the order are not sufficient to support the requested change"));
        }

        public bool ValidateCurrentQuantities(List<HSLineItem> previousLineItemStates, LineItemStatusChange lineItemStatusChange, LineItemStatus lineItemStatusChangingTo)
        {
            var relatedLineItems = previousLineItemStates.Where(previousState => previousState.ID == lineItemStatusChange.ID);
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

            var countCanBeChanged = 0;
            var validPreviousStates = LineItemStatusConstants.ValidPreviousStateLineItemChangeMap[lineItemStatusChangingTo];

            foreach (KeyValuePair<LineItemStatus, int> entry in existingStatusByQuantity)
            {
                if (validPreviousStates.Contains(entry.Key)) {
                    countCanBeChanged += entry.Value;
                }
            }
            return countCanBeChanged >= lineItemStatusChange.Quantity;
        }

        public async Task<HSLineItem> UpsertLineItem(string orderID, HSLineItem liReq, VerifiedUserContext user)
        {
            // get me product with markedup prices correct currency and the existing line items in parellel
            var productRequest = _meProductCommand.Get(liReq.ProductID, user);
            var existingLineItemsRequest = _oc.LineItems.ListAllAsync<HSLineItem>(OrderDirection.Outgoing, orderID, filters: $"Product.ID={liReq.ProductID}", accessToken: user.AccessToken);
            var orderRequest = _oc.Orders.GetAsync(OrderDirection.Incoming, orderID);
            await Task.WhenAll(productRequest, existingLineItemsRequest, orderRequest);

            var existingLineItems = await existingLineItemsRequest;
            var product = await productRequest;
            var order = await orderRequest;

            var li = new HSLineItem();
            var markedUpPrice = ValidateLineItemUnitCost(orderID, product, existingLineItems, liReq);
            liReq.UnitPrice = await markedUpPrice;
            
            Require.That(!order.IsSubmitted, new ErrorCode("Invalid Order Status", 400, "Order has already been submitted"));

            liReq.xp.StatusByQuantity = LineItemStatusConstants.EmptyStatuses;
            liReq.xp.StatusByQuantity[LineItemStatus.Open] = liReq.Quantity;

            var preExistingLi = ((List<HSLineItem>)existingLineItems).Find(eli => LineItemsMatch(eli, liReq));
            if (preExistingLi != null)
            {
                liReq.ID = preExistingLi.ID; //ensure we do not change the line item id when updating
                li = await _oc.LineItems.SaveAsync<HSLineItem>(OrderDirection.Incoming, orderID, preExistingLi.ID, liReq);
            } else
            {
                li = await _oc.LineItems.CreateAsync<HSLineItem>(OrderDirection.Incoming, orderID, liReq);
            }
            await _promotionCommand.AutoApplyPromotions(orderID);
            return li;
        }

        public async Task DeleteLineItem(string orderID, string lineItemID, VerifiedUserContext verifiedUser)
        {
            LineItem lineItem = await _oc.LineItems.GetAsync(OrderDirection.Incoming, orderID, lineItemID);
            await _oc.LineItems.DeleteAsync(OrderDirection.Incoming, orderID, lineItemID);
            List<HSLineItem> existingLineItems = await _oc.LineItems.ListAllAsync<HSLineItem>(OrderDirection.Outgoing, orderID, filters: $"Product.ID={lineItem.ProductID}", accessToken: verifiedUser.AccessToken);
            if (existingLineItems != null && existingLineItems.Count > 0)
            {
                var product = await _meProductCommand.Get(lineItem.ProductID, verifiedUser);
                await ValidateLineItemUnitCost(orderID, product, existingLineItems, null);
            }
            await _promotionCommand.AutoApplyPromotions(orderID);
        }

        public async Task<decimal> ValidateLineItemUnitCost(string orderID, SuperHSMeProduct product, List<HSLineItem> existingLineItems, HSLineItem li)
        {
            
            if (product.PriceSchedule.UseCumulativeQuantity)
            {
                int totalQuantity = li?.Quantity ?? 0;
                foreach (HSLineItem lineItem in existingLineItems) {
                    if (li == null || !LineItemsMatch(li, lineItem))
                    {
                        totalQuantity += lineItem.Quantity;
                    }
                }
                decimal priceBasedOnQuantity = product.PriceSchedule.PriceBreaks.Last(priceBreak => priceBreak.Quantity <= totalQuantity).Price;
                var tasks = new List<Task>();
                foreach (HSLineItem lineItem in existingLineItems)
                {
                    // Determine markup for all specs for this existing line item
                    decimal lineItemTotal = priceBasedOnQuantity + GetSpecMarkup(lineItem.Specs, product.Specs);
                    if (lineItem.UnitPrice != lineItemTotal)
                   {
                       PartialLineItem lineItemToPatch = new PartialLineItem();
                       lineItemToPatch.UnitPrice = lineItemTotal;
                       tasks.Add(_oc.LineItems.PatchAsync<HSLineItem>(OrderDirection.Incoming, orderID, lineItem.ID, lineItemToPatch));
                   }
                }
                await Task.WhenAll(tasks);
                // Return the item total for the li being added or modified
                return li == null ? 0 : priceBasedOnQuantity + GetSpecMarkup(li.Specs, product.Specs);
            } else
            {
                decimal lineItemTotal = 0;
                if (li != null)
                {
                    // Determine price including quantity price break discount
                    decimal priceBasedOnQuantity = product.PriceSchedule.PriceBreaks.Last(priceBreak => priceBreak.Quantity <= li.Quantity).Price;
                    // Determine markup for the 1 line item
                    lineItemTotal = priceBasedOnQuantity + GetSpecMarkup(li.Specs, product.Specs);
                }
                return lineItemTotal;
            }
        }

        public async Task HandleRMALineItemStatusChanges(OrderDirection orderDirection, RMAWithLineItemStatusByQuantity rmaWithLineItemStatusByQuantity, VerifiedUserContext verifiedUserContext)
        {
            if (!rmaWithLineItemStatusByQuantity.LineItemStatusChangesList.Any())
            {
                return;
            }
            string supplierOrderID = $"{rmaWithLineItemStatusByQuantity.RMA.SourceOrderID}-{rmaWithLineItemStatusByQuantity.RMA.SupplierID}";
            foreach (var statusChange in rmaWithLineItemStatusByQuantity.LineItemStatusChangesList)
            {
                await UpdateLineItemStatusesAndNotifyIfApplicable(OrderDirection.Incoming, supplierOrderID, statusChange, verifiedUserContext);
            }
        }

        private decimal GetSpecMarkup(IList<LineItemSpec> lineItemSpecs, IList<Spec> productSpecs)
        {
            decimal lineItemTotal = lineItemSpecs.Aggregate(0M, (accumulator, spec) =>
            {
                Spec relatedProductSpec = productSpecs.FirstOrDefault(productSpec => productSpec.ID == spec.SpecID);
                decimal? relatedSpecMarkup = 0;
                if (relatedProductSpec != null && relatedProductSpec.Options.HasItem())
                {
                    relatedSpecMarkup = relatedProductSpec.Options?.FirstOrDefault(option => option.ID == spec.OptionID)?.PriceMarkup;
                }
                return accumulator + (relatedSpecMarkup ?? 0M);
            });
            return lineItemTotal;
        }

        private bool LineItemsMatch(LineItem li1, LineItem li2)
        {
            if (li1.ProductID != li2.ProductID) return false;
            if (!String.IsNullOrEmpty(li2.xp.PrintArtworkURL)) 
            {
                if (li2.xp.PrintArtworkURL != li1.xp.PrintArtworkURL) return false;
            }
            foreach (var spec1 in li1.Specs) {
                var spec2 = (li2.Specs as List<LineItemSpec>)?.Find(s => s.SpecID == spec1.SpecID);
                if (spec1?.Value != spec2?.Value) return false;
            }
            return true;
        }
    };
}