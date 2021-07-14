using OrderCloud.SDK;
using System.Threading.Tasks;
using ordercloud.integrations.library;
using Headstart.Common.Models;
using System.Linq;
using Headstart.Common.Repositories;
using Microsoft.Azure.Cosmos;
using OrderCloud.Catalyst;
using Headstart.Models;
using System.Collections.Generic;
using Headstart.Models.Headstart;
using ordercloud.integrations.cardconnect;
using Headstart.Common.Services;
using System;
using Headstart.Models.Extended;
using Headstart.Common.Services.ShippingIntegration.Models;
using Avalara.AvaTax.RestClient;

namespace Headstart.API.Commands
{
    public interface IRMACommand
    {
        Task BuildRMA(HSOrder order, List<string> supplierIDs, LineItemStatusChanges lineItemStatusChanges, List<HSLineItem> lineItemsChanged, VerifiedUserContext verifiedUser);
        Task<RMA> PostRMA(RMA rma);
        Task<CosmosListPage<RMA>> ListBuyerRMAs(CosmosListOptions listOptions, string buyerID);
        Task<RMA> Get(ListArgs<RMA> args, VerifiedUserContext verifiedUser);
        Task<CosmosListPage<RMA>> ListRMAsByOrderID(string orderID, VerifiedUserContext verifiedUser, bool accessAllRMAsOnOrder = false);
        Task<CosmosListPage<RMA>> ListRMAs(CosmosListOptions listOptions, VerifiedUserContext verifiedUser);
        Task<RMAWithLineItemStatusByQuantity> ProcessRMA(RMA rma, VerifiedUserContext verifiedUser);
        Task<RMAWithLineItemStatusByQuantity> ProcessRefund(string rmaNumber, VerifiedUserContext verifiedUser);
    }

    public class RMACommand : IRMACommand
    {
        private const string QUEUED_FOR_CAPTURE = "Queued for Capture";
        private readonly IOrderCloudClient _oc;
        private readonly IRMARepo _rmaRepo;
        private readonly IOrderCloudIntegrationsCardConnectService _cardConnect;
        private readonly ISendgridService _sendgridService;

        public RMACommand(IOrderCloudClient oc, IRMARepo rmaRepo, IOrderCloudIntegrationsCardConnectService cardConnect, ISendgridService sendgridService)
        {
            _oc = oc;
            _rmaRepo = rmaRepo;
            _cardConnect = cardConnect;
            _sendgridService = sendgridService;
        }

        public async Task BuildRMA(HSOrder order, List<string> supplierIDs, LineItemStatusChanges lineItemStatusChanges, List<HSLineItem> lineItemsChanged, VerifiedUserContext verifiedUser)
        {
            foreach (string supplierID in supplierIDs)
            {
                HSSupplier supplier = await _oc.Suppliers.GetAsync<HSSupplier>(supplierID);

                RMA rma = new RMA()
                {
                    PartitionKey = "PartitionValue",
                    SourceOrderID = order.ID,
                    TotalCredited = 0M,
                    ShippingCredited = 0M,
                    RMANumber = await BuildRMANumber(order),
                    SupplierID = supplierID,
                    SupplierName = supplier.Name,
                    Type = lineItemStatusChanges.Status == LineItemStatus.CancelRequested ? RMAType.Cancellation : RMAType.Return,
                    DateCreated = DateTime.Now,
                    DateComplete = null,
                    Status = RMAStatus.Requested,
                    LineItems = BuildLineItemRMA(supplierID, lineItemStatusChanges, lineItemsChanged, order),
                    Logs = new List<RMALog>(),
                    FromBuyerID = order.FromCompanyID,
                    FromBuyerUserID = order.FromUser.ID
                };
                await PostRMA(rma);
            }
        }

        private async Task<string> BuildRMANumber(HSOrder order)
        {
            var args = new CosmosListOptions()
            {
                PageSize = 100,
                Filters = new List<ListFilter>() { new ListFilter("SourceOrderID", order.ID) }
            };

            CosmosListPage<RMA> existingRMAsOnOrder = await ListBuyerRMAs(args, order.FromCompanyID);
            int lastRMANumber = existingRMAsOnOrder.Items
                .OrderBy(rma => rma.RMANumber)
                .Select(rma => int.Parse(rma.RMANumber.Substring(rma.RMANumber.LastIndexOf("-") + 1)))
                .LastOrDefault();
            string rmaSuffix = $"{lastRMANumber + 1}".PadLeft(2, '0');
            string rmaNumber = $"{order.ID}-RMA-{rmaSuffix}";
            return rmaNumber;
        }

        private List<RMALineItem> BuildLineItemRMA(string supplierID, LineItemStatusChanges lineItemStatusChanges, List<HSLineItem> lineItemsChanged, HSOrder order)
        {
            List<RMALineItem> rmaLineItems = new List<RMALineItem>();
            foreach (HSLineItem lineItem in lineItemsChanged)
            {
                if (lineItem.SupplierID == supplierID)
                {
                    RMALineItem rmaLineItem = new RMALineItem()
                    {
                        ID = lineItem.ID,
                        QuantityRequested = (int)lineItemStatusChanges.Changes.FirstOrDefault(change => change.ID == lineItem.ID)?.Quantity,
                        QuantityProcessed = 0,
                        Status = RMALineItemStatus.Requested,
                        Reason = lineItemStatusChanges.Changes.FirstOrDefault(change => change.ID == lineItem.ID)?.Reason,
                        PercentToRefund = null,
                        RefundableViaCreditCard = order.xp.PaymentMethod == "Credit Card",
                        IsResolved = false,
                        IsRefunded = false,
                        LineTotalRefund = 0
                    };
                    rmaLineItems.Add(rmaLineItem);
                }
            }
            return rmaLineItems;
        }

        public virtual async Task<RMA> PostRMA(RMA rma)
        {
            return await _rmaRepo.AddItemAsync(rma);
        }

        public async Task<CosmosListPage<RMA>> ListBuyerRMAs(CosmosListOptions listOptions, string buyerID)
        {
            IQueryable<RMA> queryable = _rmaRepo.GetQueryable().Where(rma => rma.FromBuyerID == buyerID);

            CosmosListPage<RMA> rmas = await GenerateRMAList(queryable, listOptions);
            return rmas;
        }

        public async Task<RMA> Get(ListArgs<RMA> args, VerifiedUserContext verifiedUser)
        {
            CosmosListOptions listOptions = new CosmosListOptions()
            {
                PageSize = 100,
                Search = args.Search,
                SearchOn = "RMANumber"
            };

            IQueryable<RMA> queryable = _rmaRepo.GetQueryable()
                .Where(rma =>
                 rma.PartitionKey == "PartitionValue");

            string verifiedUserType = verifiedUser.UserType;

            if (verifiedUserType == "supplier")
            {
                queryable = queryable.Where(rma => rma.SupplierID == verifiedUser.Supplier.ID);
            }

            CosmosListPage<RMA> rmas = await GenerateRMAList(queryable, listOptions);
            return rmas.Items[0];
        }

        private async Task<CosmosListPage<RMA>> GenerateRMAList(IQueryable<RMA> queryable, CosmosListOptions listOptions)
        {
            QueryRequestOptions requestOptions = new QueryRequestOptions();
            requestOptions.MaxItemCount = listOptions.PageSize;

            CosmosListPage<RMA> rmas = await _rmaRepo.GetItemsAsync(queryable, requestOptions, listOptions);
            return rmas;
        }

        public virtual async Task<CosmosListPage<RMA>> ListRMAsByOrderID(string orderID, VerifiedUserContext verifiedUser, bool accessAllRMAsOnOrder = false)
        {
            string sourceOrderID = orderID.Split("-")[0];

            CosmosListOptions listOptions = new CosmosListOptions() { PageSize = 100 };

            IQueryable<RMA> queryable = _rmaRepo.GetQueryable()
                .Where(rma =>
                    rma.PartitionKey == "PartitionValue"
                    && rma.SourceOrderID == sourceOrderID);

            string verifiedUserType = verifiedUser.UserType;

            if (verifiedUserType == "supplier" && !accessAllRMAsOnOrder)
            {
                queryable = QueryOnlySupplierRMAs(queryable, verifiedUser);
            }

            CosmosListPage<RMA> rmas = await GenerateRMAList(queryable, listOptions);
            return rmas;
        }

        public virtual IQueryable<RMA> QueryOnlySupplierRMAs(IQueryable<RMA> queryable, VerifiedUserContext verifiedUser)
        {
            return queryable.Where(rma => rma.SupplierID == verifiedUser.Supplier.ID);
        }

        public async Task<CosmosListPage<RMA>> ListRMAs(CosmosListOptions listOptions, VerifiedUserContext verifiedUser)
        {
            IQueryable<RMA> queryable = _rmaRepo.GetQueryable().Where(rma => rma.PartitionKey == "PartitionValue");

            string verifiedUserType = verifiedUser.UserType;

            if (verifiedUserType == "supplier")
            {
                queryable = queryable.Where(rma => rma.SupplierID == verifiedUser.Supplier.ID);
            }

            CosmosListPage<RMA> rmas = await GenerateRMAList(queryable, listOptions);
            return rmas;
        }

        public async Task<RMAWithLineItemStatusByQuantity> ProcessRMA(RMA rma, VerifiedUserContext verifiedUser)
        {
            // Get the RMA from the last time it was saved.
            RMA currentRMA = await GetRMA(rma.RMANumber, verifiedUser);
            if (currentRMA.SupplierID != verifiedUser.Supplier.ID)
            {
                throw new Exception($"You do not have permission to process this RMA.");
            }

            // Should the Status and IsResolved proprerties of an RMALineItem change?
            rma.LineItems = UpdateRMALineItemStatusesAndCheckIfResolved(rma.LineItems);

            // Should status of RMA change?
            rma = UpdateRMAStatus(rma);

            // If the status on the new RMA differs from the old RMA, create an RMALog
            if (rma.Status != currentRMA.Status)
            {
                RMALog log = new RMALog() { Status = rma.Status, Date = DateTime.Now, FromUserID = verifiedUser.ID };
                rma.Logs.Insert(0, log);
            }

            HSOrderWorksheet worksheet = await _oc.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.Incoming, rma.SourceOrderID);

            IEnumerable<RMALineItem> deniedLineItems = rma.LineItems
                .Where(li => !li.IsRefunded && li.Status == RMALineItemStatus.Denied)
                .Where(li => currentRMA.LineItems.FirstOrDefault(currentLi => currentLi.ID == li.ID).Status != RMALineItemStatus.Denied).ToList();

            List<LineItemStatusChanges> lineItemStatusChangesList = BuildLineItemStatusChanges(deniedLineItems, worksheet, rma.Type, true);

            ItemResponse<RMA> updatedRMA = await _rmaRepo.ReplaceItemAsync(currentRMA.id, rma);

            await HandlePendingApprovalEmails(currentRMA, rma.LineItems, worksheet);

            RMAWithLineItemStatusByQuantity rmaWithStatusByQuantityChanges = new RMAWithLineItemStatusByQuantity()
            {
                SupplierOrderID = $"{rma.SourceOrderID}-{rma.SupplierID}",
                RMA = updatedRMA.Resource,
                LineItemStatusChangesList = lineItemStatusChangesList
            };

            return rmaWithStatusByQuantityChanges;
        }

        private async Task<RMA> GetRMA(string rmaNumber, VerifiedUserContext verifiedUser)
        {
            var currentRMAFilter = new ListFilter("RMANumber", rmaNumber);
            CosmosListOptions currentRMAListOptions = new CosmosListOptions() { PageSize = 1, ContinuationToken = null, Filters = { currentRMAFilter } };
            CosmosListPage<RMA> currentRMAListPage = await ListRMAs(currentRMAListOptions, verifiedUser);
            RMA currentRMA = currentRMAListPage.Items[0];
            return currentRMA;
        }

        private List<RMALineItem> UpdateRMALineItemStatusesAndCheckIfResolved(List<RMALineItem> rmaLineItems)
        {
            foreach (RMALineItem lineItem in rmaLineItems)
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
            return rmaLineItems;
        }

        private RMA UpdateRMAStatus(RMA rma)
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
            return rma;
        }

        private List<LineItemStatusChanges> BuildLineItemStatusChanges(IEnumerable<RMALineItem> rmaLineItemsToUpdate, HSOrderWorksheet worksheet, RMAType rmaType, bool isDenyingAll)
        {
            if (!rmaLineItemsToUpdate.Any())
            {
                return new List<LineItemStatusChanges>();
            }
            IEnumerable<HSLineItem> orderWorksheetLineItems = worksheet.LineItems
                .Where(li => rmaLineItemsToUpdate
                    .Any(itemToUpdate => itemToUpdate.ID == li.ID));

            List<LineItemStatusChanges> lineItemStatusChangesList = new List<LineItemStatusChanges>();

            LineItemStatus actionCompleteType = rmaType == RMAType.Cancellation ? LineItemStatus.Canceled : LineItemStatus.Returned;
            LineItemStatus actionDeniedType = rmaType == RMAType.Cancellation ? LineItemStatus.CancelDenied : LineItemStatus.ReturnDenied;

            foreach (HSLineItem lineItem in orderWorksheetLineItems)
            {
                RMALineItem correspondingRMALineItem = rmaLineItemsToUpdate.FirstOrDefault(li => li.ID == lineItem.ID);
                if (correspondingRMALineItem.QuantityProcessed == correspondingRMALineItem.QuantityRequested)
                {
                    if (isDenyingAll)
                    {
                        LineItemStatusChanges statusChangeToAdjust = lineItemStatusChangesList.FirstOrDefault(change => change.Status == actionDeniedType);
                        BuildStatusChangeToAdjust(statusChangeToAdjust, actionDeniedType, lineItemStatusChangesList, correspondingRMALineItem, isDenyingAll);
                    }
                    else
                    {
                        LineItemStatusChanges statusChangeToAdjust = lineItemStatusChangesList.FirstOrDefault(change => change.Status == actionCompleteType);
                        BuildStatusChangeToAdjust(statusChangeToAdjust, actionCompleteType, lineItemStatusChangesList, correspondingRMALineItem, isDenyingAll);
                    }
                }
                else
                {
                    int quantityToComplete = correspondingRMALineItem.QuantityProcessed;
                    int quantityToDeny = correspondingRMALineItem.QuantityRequested - correspondingRMALineItem.QuantityProcessed;
                    LineItemStatusChanges statusChangeForComplete = lineItemStatusChangesList.FirstOrDefault(change => change.Status == actionCompleteType);
                    LineItemStatusChanges statusChangeForDeny = lineItemStatusChangesList.FirstOrDefault(change => change.Status == actionDeniedType);

                    BuildStatusChangeToAdjust(statusChangeForComplete, actionCompleteType, lineItemStatusChangesList, correspondingRMALineItem, isDenyingAll, quantityToComplete);

                    BuildStatusChangeToAdjust(statusChangeForDeny, actionDeniedType, lineItemStatusChangesList, correspondingRMALineItem, isDenyingAll, quantityToDeny);
                }
            }

            return lineItemStatusChangesList;
        }

        private void BuildStatusChangeToAdjust(LineItemStatusChanges statusChangeToAdjust, LineItemStatus lineItemStatus, List<LineItemStatusChanges> lineItemStatusChangesList, RMALineItem correspondingRMALineItem, bool isDenyingAll, int? overriddenQuantity = null)
        {
            int? quantityToChange = overriddenQuantity != null ? overriddenQuantity : correspondingRMALineItem.QuantityProcessed;
            if ((int)quantityToChange == 0)
            {
                return;
            }

            if (statusChangeToAdjust == null)
            {
                LineItemStatusChanges newStatusChange = new LineItemStatusChanges() { Status = lineItemStatus, Changes = new List<LineItemStatusChange>() };
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
                ID = correspondingRMALineItem.ID,
                Quantity = (int)quantityToChange,
                Comment = correspondingRMALineItem.Comment,
                Refund = refundAmount,
                QuantityRequestedForRefund = correspondingRMALineItem.QuantityRequested,
            });
        }

        private async Task HandlePendingApprovalEmails(RMA rma, List<RMALineItem> rmaLineItems, HSOrderWorksheet worksheet)
        {
            IEnumerable<RMALineItem> pendingApprovalLineItemsWithNewComments = rmaLineItems
                .Where(li => li.Status == RMALineItemStatus.Processing)
                .Where(li => li.Comment != rma.LineItems.FirstOrDefault(item => item.ID == li.ID).Comment);

            if (!pendingApprovalLineItemsWithNewComments.Any())
            {
                return;
            }

            LineItemStatusChanges lineItemStatusChanges = new LineItemStatusChanges()
            {
                Status = rma.Type == RMAType.Cancellation ? LineItemStatus.CancelRequested : LineItemStatus.ReturnRequested,
                Changes = new List<LineItemStatusChange>()
            };

            foreach (var rmaLineItem in pendingApprovalLineItemsWithNewComments)
            {
                lineItemStatusChanges.Changes.Add(new LineItemStatusChange()
                {
                    ID = rmaLineItem.ID,
                    Quantity = rmaLineItem.QuantityProcessed,
                    Comment = rmaLineItem.Comment,
                    QuantityRequestedForRefund = rmaLineItem.QuantityRequested
                });

            }

            List<HSLineItem> lineItemsChanged = worksheet.LineItems.Where(li => lineItemStatusChanges.Changes.Select(li => li.ID).Contains(li.ID)).ToList();

            Supplier supplier = await _oc.Suppliers.GetAsync(rma.SupplierID);

            EmailDisplayText emailText = new EmailDisplayText()
            {
                EmailSubject = $"New message available from {supplier.Name}",
                DynamicText = $"{supplier.Name} has contacted you regarding your request for {rma.Type.ToString().ToLower()}",
                DynamicText2 = "The following items have new messages"
            };

            await _sendgridService.SendLineItemStatusChangeEmail(worksheet.Order, lineItemStatusChanges, lineItemsChanged, worksheet.Order.FromUser.FirstName, worksheet.Order.FromUser.LastName, worksheet.Order.FromUser.Email, emailText);
        }

        public async Task<RMAWithLineItemStatusByQuantity> ProcessRefund(string rmaNumber, VerifiedUserContext verifiedUser)
        {
            RMA rma = await GetRMA(rmaNumber, verifiedUser);

            ValidateRMA(rma, verifiedUser);

            decimal initialAmountRefunded = rma.TotalCredited;

            IEnumerable<RMALineItem> rmaLineItemsToUpdate = rma.LineItems
                .Where(li => !li.IsRefunded
                    && (li.Status == RMALineItemStatus.Approved || li.Status == RMALineItemStatus.PartialQtyApproved)).ToList();

            HSOrderWorksheet worksheet = await _oc.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.Incoming, rma.SourceOrderID);

            CosmosListPage<RMA> allRMAsOnThisOrder = await ListRMAsByOrderID(worksheet.Order.ID, verifiedUser, true);

            CalculateAndUpdateLineTotalRefund(rmaLineItemsToUpdate, worksheet, allRMAsOnThisOrder, rma.SupplierID);

            // UPDATE RMA LINE ITEM STATUSES
            SetRMALineItemStatusesToComplete(rmaLineItemsToUpdate);

            // UPDATE RMA STATUS
            UpdateRMAStatus(rma);

            await HandleRefund(rma, allRMAsOnThisOrder, worksheet, verifiedUser);

            MarkRMALineItemsAsRefunded(rmaLineItemsToUpdate);

            decimal totalRefundedForThisTransaction = rma.TotalCredited - initialAmountRefunded;
            RMALog log = new RMALog() { Status = rma.Status, Date = DateTime.Now, AmountRefunded = totalRefundedForThisTransaction, FromUserID = verifiedUser.ID };
            rma.Logs.Insert(0, log);

            List<LineItemStatusChanges> lineItemStatusChangesList = BuildLineItemStatusChanges(rmaLineItemsToUpdate, worksheet, rma.Type, false);

            // SAVE RMA
            ItemResponse<RMA> updatedRMA = await _rmaRepo.ReplaceItemAsync(rma.id, rma);

            RMAWithLineItemStatusByQuantity rmaWithStatusByQuantityChanges = new RMAWithLineItemStatusByQuantity() { SupplierOrderID = $"{rma.SourceOrderID}-{rma.SupplierID}", RMA = updatedRMA.Resource, LineItemStatusChangesList = lineItemStatusChangesList };

            return rmaWithStatusByQuantityChanges;
        }

        private void ValidateRMA(RMA rma, VerifiedUserContext verifiedUser)
        {
            if (rma.SupplierID != verifiedUser.Supplier.ID)
            {
                throw new Exception("You do not have permission to process a refund for this RMA.");
            }

            if (rma.Status == RMAStatus.Complete || rma.Status == RMAStatus.Denied || rma.Status == RMAStatus.Requested)
            {
                throw new Exception("This RMA is not in a valid status to be refunded.");
            }
        }

        public void CalculateAndUpdateLineTotalRefund(IEnumerable<RMALineItem> lineItemsToUpdate, HSOrderWorksheet orderWorksheet, CosmosListPage<RMA> allRMAsOnThisOrder, string supplierID)
        {
            IEnumerable<dynamic> orderWorksheetLines = orderWorksheet.OrderCalculateResponse.xp.TaxResponse.lines;

            foreach (RMALineItem rmaLineItem in lineItemsToUpdate)
            {
                if (!rmaLineItem.RefundableViaCreditCard)
                {
                    HSLineItem lineItemFromOrder = orderWorksheet.LineItems.FirstOrDefault(li => li.ID == rmaLineItem.ID);
                    rmaLineItem.LineTotalRefund = lineItemFromOrder.LineTotal / lineItemFromOrder.Quantity * rmaLineItem.QuantityProcessed;
                }
                else
                {
                    int quantityToRefund = rmaLineItem.QuantityProcessed;
                    dynamic orderWorksheetLineItem = orderWorksheetLines.FirstOrDefault(li => li.lineNumber == rmaLineItem.ID);

                    // Exempt products will have an exempt amount instead of a taxable amount.
                    decimal lineItemBaseCost = orderWorksheetLineItem.exemptAmount > 0 ? orderWorksheetLineItem.exemptAmount : orderWorksheetLineItem.taxableAmount;
                    decimal totalRefundIfReturningAllLineItems = (decimal)(lineItemBaseCost + orderWorksheetLineItem.tax);
                    double taxableAmountPerSingleLineItem = (double)(lineItemBaseCost / orderWorksheetLineItem.quantity);
                    double taxPerSingleLineItem = (double)(orderWorksheetLineItem.tax / orderWorksheetLineItem.quantity);
                    double singleQuantityLineItemRefund = Math.Round(taxableAmountPerSingleLineItem + taxPerSingleLineItem, 2);
                    decimal expectedLineTotalRefund = (decimal)singleQuantityLineItemRefund * quantityToRefund;

                    rmaLineItem.LineTotalRefund = ValidateExpectedLineTotalRefund(expectedLineTotalRefund, totalRefundIfReturningAllLineItems, allRMAsOnThisOrder, rmaLineItem, orderWorksheet, supplierID);
                }

                ApplyPercentToDiscount(rmaLineItem);

                rmaLineItem.Status = rmaLineItem.QuantityProcessed == rmaLineItem.QuantityRequested ? RMALineItemStatus.Complete : RMALineItemStatus.PartialQtyComplete;
            }
        }

        private decimal ValidateExpectedLineTotalRefund(decimal expectedLineTotalRefund, decimal totalRefundIfReturningAllLineItems, CosmosListPage<RMA> allRMAsOnThisOrder, RMALineItem rmaLineItem, HSOrderWorksheet orderWorksheet, string supplierID)
        {
            // If minor rounding error occurs during singleQuantityLineItemRefund calculation, ensure we don't refund more than the full line item cost on the order
            // Would only occur for full quantity cancellations/returns
            if (expectedLineTotalRefund > totalRefundIfReturningAllLineItems || ShouldIssueFullLineItemRefund(rmaLineItem, allRMAsOnThisOrder, orderWorksheet, supplierID))
            {
                return totalRefundIfReturningAllLineItems;
            }

            // If previous RMAs on this order for the same line item
            if (allRMAsOnThisOrder.Items.Count > 1)
            {
                decimal previouslyRefundedAmountForThisLineItem = 0M;
                // Find previously refunded total for line items on this order...
                foreach (RMA previouslyRefundedRMA in allRMAsOnThisOrder.Items)
                {
                    RMALineItem previouslyRefundedLineItem = previouslyRefundedRMA.LineItems.FirstOrDefault(li => li.ID == rmaLineItem.ID);
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
                    decimal totalAfterPossibleRoundingErrors = totalRefundIfReturningAllLineItems - previouslyRefundedAmountForThisLineItem;
                    return totalAfterPossibleRoundingErrors;
                }
            }

            return expectedLineTotalRefund;
        }

        private bool ShouldIssueFullLineItemRefund(RMALineItem rmaLineItem, CosmosListPage<RMA> allRMAsOnThisOrder, HSOrderWorksheet orderWorksheet, string supplierID)
        {
            TransactionLineModel orderWorksheetLineItem = orderWorksheet.OrderCalculateResponse.xp.TaxResponse.lines.FirstOrDefault(li => li.lineNumber == rmaLineItem.ID);
            var rmasFromThisSupplier = allRMAsOnThisOrder.Items.Where(r => r.SupplierID == supplierID);
            // If this is the only RMA for this line item and all requested RMA quantity are approved, and the quantity equals the original order quantity, issue a full refund (line item cost + tax).
            if (rmaLineItem.Status == RMALineItemStatus.Approved && rmaLineItem.QuantityProcessed == orderWorksheetLineItem.quantity && rmasFromThisSupplier.Count() == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public virtual void ApplyPercentToDiscount(RMALineItem rmaLineItem)
        {
            if (rmaLineItem.PercentToRefund <= 0 || rmaLineItem.PercentToRefund > 100)
            {
                throw new Exception($"The refund percentage for {rmaLineItem.ID} must be greater than 0 and no higher than 100 percent.");
            }
            if (rmaLineItem.PercentToRefund != null)
            {
                // Math.Round() by default would round 13.745 to 13.74 based on banker's rounding (going to the nearest even number when the final digit is 5).
                // This errs on the side of rounding up (away from zero) when only a percentage of a refund is applied.
                rmaLineItem.LineTotalRefund = Math.Round((decimal)(rmaLineItem.LineTotalRefund / 100 * rmaLineItem.PercentToRefund), 2, MidpointRounding.AwayFromZero);
            }
        }

        private void SetRMALineItemStatusesToComplete(IEnumerable<RMALineItem> rmaLineItemsToUpdate)
        {
            foreach (RMALineItem lineItem in rmaLineItemsToUpdate)
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

        public virtual async Task HandleRefund(RMA rma, CosmosListPage<RMA> allRMAsOnThisOrder, HSOrderWorksheet worksheet, VerifiedUserContext verifiedUser)
        {

            // Get payment info from the order
            ListPage<HSPayment> paymentResponse = await _oc.Payments.ListAsync<HSPayment>(OrderDirection.Incoming, rma.SourceOrderID);
            HSPayment creditCardPayment = paymentResponse.Items.FirstOrDefault(payment => payment.Type == OrderCloud.SDK.PaymentType.CreditCard);
            if (creditCardPayment == null)
            {
                // Items were not paid for with a credit card.  No refund to process via CardConnect.
                return;
            }
            HSPaymentTransaction creditCardPaymentTransaction = creditCardPayment.Transactions
                .OrderBy(x => x.DateExecuted)
                .LastOrDefault(x => x.Type == "CreditCard" && x.Succeeded);
            decimal purchaseOrderTotal = (decimal)paymentResponse.Items
                .Where(payment => payment.Type == OrderCloud.SDK.PaymentType.PurchaseOrder)
                .Select(payment => payment.Amount)
                .Sum();

            // Refund via CardConnect
            CardConnectInquireResponse inquiry = await _cardConnect.Inquire(new CardConnectInquireRequest
            {
                merchid = creditCardPaymentTransaction.xp.CardConnectResponse.merchid,
                orderid = rma.SourceOrderID,
                set = "1",
                currency = worksheet.Order.xp.Currency.ToString(),
                retref = creditCardPaymentTransaction.xp.CardConnectResponse.retref
            });

            decimal shippingRefund = rma.Type == RMAType.Cancellation ? GetShippingRefundIfCancellingAll(worksheet, rma, allRMAsOnThisOrder) : 0M;

            decimal lineTotalToRefund = rma.LineItems
                .Where(li => li.IsResolved
                    && !li.IsRefunded
                    && li.RefundableViaCreditCard
                    && (li.Status == RMALineItemStatus.PartialQtyComplete || li.Status == RMALineItemStatus.Complete)
                    ).Select(li => li.LineTotalRefund)
                    .Sum();

            decimal totalToRefund = lineTotalToRefund + shippingRefund;

            // Update Total Credited on RMA
            rma.TotalCredited += totalToRefund;

            // Transactions that are queued for capture can only be fully voided, and we are only allowing partial voids moving forward.
            if (inquiry.voidable == "Y" && inquiry.setlstat == QUEUED_FOR_CAPTURE)
            {
                throw new CatalystBaseException(new ApiError
                {
                    ErrorCode = "Payment.FailedToVoidAuthorization",
                    Message = "This customer's credit card transaction is currently queued for capture and cannot be refunded at this time.  Please try again later."
                });
            }

            // If voidable, but not refundable, void the refund amount off the original order total
            if (inquiry.voidable == "Y")
            {
                await HandleVoidTransaction(worksheet, creditCardPayment, creditCardPaymentTransaction, totalToRefund, rma);
            }

            // If refundable, but not voidable, do a refund
            if (inquiry.voidable == "N")
            {
                await HandleRefundTransaction(worksheet, creditCardPayment, creditCardPaymentTransaction, totalToRefund, rma);
            }
        }

        public virtual decimal GetShippingRefundIfCancellingAll(HSOrderWorksheet worksheet, RMA rma, CosmosListPage<RMA> allRMAsOnThisOrder)
        {
            // What are all the line items on this order for this supplier and their quantities?
            IEnumerable<HSLineItem> allLineItemsShippedFromThisSupplier = worksheet.LineItems
                .Where(li => li.SupplierID == rma.SupplierID && worksheet.Order.xp.PaymentMethod == "Credit Card");
            Dictionary<string, int> allLineItemsDictionary = new Dictionary<string, int>();
            foreach (HSLineItem li in allLineItemsShippedFromThisSupplier)
            {
                allLineItemsDictionary.Add(li.ID, li.Quantity);
            }

            // Including this RMA and previous RMAs for this supplier, get everything that has been refunded or is about to be refunded.
            var rmasFromThisSupplier = allRMAsOnThisOrder.Items.Where(r => r.SupplierID == rma.SupplierID);
            Dictionary<string, int> allCompleteRMALineItemsDictionary = new Dictionary<string, int>();
            foreach (RMA existingRMA in rmasFromThisSupplier)
            {
                RMA rmaToAnalyze = existingRMA.RMANumber == rma.RMANumber ? rma : existingRMA;
                foreach (RMALineItem rmaLineItem in rmaToAnalyze.LineItems)
                {
                    if (rmaLineItem.Status == RMALineItemStatus.Complete && rmaLineItem.RefundableViaCreditCard)
                    {
                        if (!allCompleteRMALineItemsDictionary.ContainsKey(rmaLineItem.ID))
                        {
                            allCompleteRMALineItemsDictionary.Add(rmaLineItem.ID, rmaLineItem.QuantityProcessed);
                        }
                        else
                        {
                            allCompleteRMALineItemsDictionary[rmaLineItem.ID] += rmaLineItem.QuantityProcessed;
                        }
                    }
                }
            }

            // If these are the same, the supplier hasn't shipped anything, and shipping should be credited back to the buyer.
            bool shouldShippingBeCanceled = allLineItemsDictionary.OrderBy(kvp => kvp.Key)
                .SequenceEqual(allCompleteRMALineItemsDictionary.OrderBy(kvp => kvp.Key));

            // Figure out what the buyer paid for shipping for this supplier on this order.
            if (shouldShippingBeCanceled)
            {
                string selectedShipMethodID = worksheet.ShipEstimateResponse.ShipEstimates
                    .FirstOrDefault(estimate => estimate.xp.SupplierID == rma.SupplierID)?.SelectedShipMethodID;
                TransactionLineModel shippingLine = worksheet.OrderCalculateResponse.xp.TaxResponse.lines.FirstOrDefault(line => line.lineNumber == selectedShipMethodID);
                decimal shippingCostToRefund = (decimal)(shippingLine.taxableAmount + shippingLine.tax + shippingLine.exemptAmount);
                rma.ShippingCredited += shippingCostToRefund;
                return shippingCostToRefund;
            }

            return 0M;
        }

        public virtual async Task HandleVoidTransaction(HSOrderWorksheet worksheet, HSPayment creditCardPayment, HSPaymentTransaction creditCardPaymentTransaction, decimal totalToRefund, RMA rma)
        {
            HSPayment newCreditCardVoid = new HSPayment() { Amount = totalToRefund };
            try
            {
                CardConnectVoidResponse response = await _cardConnect.VoidAuthorization(new CardConnectVoidRequest
                {
                    currency = worksheet.Order.xp.Currency.ToString(),
                    merchid = creditCardPaymentTransaction.xp.CardConnectResponse.merchid,
                    retref = creditCardPaymentTransaction.xp.CardConnectResponse.retref,
                    amount = totalToRefund.ToString("F2"),
                });
                await _oc.Payments.CreateTransactionAsync(OrderDirection.Incoming, rma.SourceOrderID, creditCardPayment.ID, CardConnectMapper.Map(newCreditCardVoid, response));
            }
            catch (CreditCardVoidException ex)
            {
                await _oc.Payments.CreateTransactionAsync(OrderDirection.Incoming, rma.SourceOrderID, creditCardPayment.ID, CardConnectMapper.Map(newCreditCardVoid, ex.Response));
                throw new CatalystBaseException(new ApiError
                {
                    ErrorCode = "Payment.FailedToVoidAuthorization",
                    Message = ex.ApiError.Message
                });
            }
        }

        public virtual async Task HandleRefundTransaction(HSOrderWorksheet worksheet, HSPayment creditCardPayment, HSPaymentTransaction creditCardPaymentTransaction, decimal totalToRefund, RMA rma)
        {
            try
            {
                CardConnectRefundRequest requestedRefund = new CardConnectRefundRequest()
                {
                    currency = worksheet.Order.xp.Currency.ToString(),
                    merchid = creditCardPaymentTransaction.xp.CardConnectResponse.merchid,
                    retref = creditCardPaymentTransaction.xp.CardConnectResponse.retref,
                    amount = totalToRefund.ToString("F2"),
                };
                CardConnectRefundResponse response = await _cardConnect.Refund(requestedRefund);
                HSPayment newCreditCardPayment = new HSPayment() { Amount = response.amount };
                await _oc.Payments.CreateTransactionAsync(OrderDirection.Incoming, rma.SourceOrderID, creditCardPayment.ID, CardConnectMapper.Map(newCreditCardPayment, response));
            }
            catch (CreditCardRefundException ex)
            {
                throw new CatalystBaseException(new ApiError
                {
                    ErrorCode = "Payment.FailedToRefund",
                    Message = ex.ApiError.Message
                });
            }
        }

        private void MarkRMALineItemsAsRefunded(IEnumerable<RMALineItem> rmaLineItemsToUpdate)
        {
            foreach (RMALineItem rmaLineItem in rmaLineItemsToUpdate)
            {
                rmaLineItem.IsRefunded = true;
            }
        }
    }
}
