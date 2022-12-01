using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Headstart.Common.Models;
using Headstart.Common.Services;
using OrderCloud.Catalyst;
using OrderCloud.SDK;

namespace Headstart.API.Commands
{
    public interface IOrderReturnIntegrationEventCommand
    {
        public Task<HSCalculateOrderReturnResponse> CalculateOrderReturn(HSOrderWorksheet worksheet, HSOrderReturn orderReturn);
    }

    public class OrderReturnIntegrationEventCommand : IOrderReturnIntegrationEventCommand
    {
        private readonly IOrderCloudClient oc;
        private readonly IDiscountDistributionService discountDistributionService;

        public OrderReturnIntegrationEventCommand(IOrderCloudClient oc, IDiscountDistributionService discountDistributionService)
        {
            this.oc = oc;
            this.discountDistributionService = discountDistributionService;
        }

        public async Task<HSCalculateOrderReturnResponse> CalculateOrderReturn(HSOrderWorksheet worksheet, HSOrderReturn orderReturn)
        {
            var orderPromos = worksheet.OrderPromotions;
            var allLineItems = discountDistributionService.GetLineItemsWithProportionalPromoDiscountApplied(worksheet);
            var taxLines = worksheet?.OrderCalculateResponse?.xp?.TaxCalculation?.LineItems;
            var previousOrderReturns = await oc.OrderReturns.ListAllAsync<HSOrderReturn>(filters: $"OrderID={worksheet.Order.ID}&Status=Open|Completed&ID!={orderReturn.ID}");
            return new HSCalculateOrderReturnResponse
            {
                ItemsToReturnCalcs = orderReturn.ItemsToReturn.Select(orderReturnItem =>
                {
                    var lineItem = allLineItems.FirstOrDefault(li => li.ID == orderReturnItem.LineItemID);
                    var quantityToRefund = orderReturnItem.Quantity;
                    if (taxLines == null)
                    {
                        var pricePerQuantity = lineItem.LineTotal / lineItem.Quantity;
                        return new LineItemReturnCalculation
                        {
                            LineItemID = lineItem.ID,
                            RefundAmount = pricePerQuantity * quantityToRefund,
                        };
                    }
                    else
                    {
                        // Tax doesn't live on the line item but if a tax calculation occurred and we have access to the tax lines
                        // on the order worksheet then we should also refund tax
                        var taxLine = taxLines == null ? null : taxLines.FirstOrDefault(taxLine => taxLine.LineItemID == orderReturnItem.LineItemID);
                        var lineItemTotalTax = taxLine?.LineItemTotalTax ?? 0; // Exempt products will have an exempt amount instead of a taxable amount.
                        var totalRefundIfReturningAllLineItems = lineItem.LineTotal + lineItemTotalTax;
                        var taxableAmountPerSingleLineItem = (double)(lineItem.LineTotal / lineItem.Quantity);
                        var taxPerSingleLineItem = (double)(lineItemTotalTax / lineItem.Quantity);
                        var singleQuantityLineItemRefund = Math.Round(taxableAmountPerSingleLineItem + taxPerSingleLineItem, 2);
                        var expectedLineTotalRefund = (decimal)singleQuantityLineItemRefund * quantityToRefund;
                        var refundAmount = ValidateRefundAmount(expectedLineTotalRefund, totalRefundIfReturningAllLineItems, orderReturnItem, lineItem, previousOrderReturns);

                        return new LineItemReturnCalculation
                        {
                            LineItemID = lineItem.ID,
                            RefundAmount = refundAmount,
                        };
                    }
                }),
            };
        }

        private decimal ValidateRefundAmount(decimal expectedLineTotalRefund, decimal totalRefundIfReturningAllLineItems, OrderReturnItem orderReturnItem, HSLineItem lineItem, List<HSOrderReturn> previousOrderReturns)
        {
            // If minor rounding error occurs during singleQuantityLineItemRefund calculation, ensure we don't refund more than the full line item cost on the order
            var shouldIssueFullRefund = orderReturnItem.Quantity == lineItem.Quantity;
            if (expectedLineTotalRefund > totalRefundIfReturningAllLineItems || shouldIssueFullRefund)
            {
                return totalRefundIfReturningAllLineItems;
            }

            if (previousOrderReturns.Count > 1)
            {
                decimal previouslyRefundedAmountForThisLineItem = 0M;

                // Find previously refunded total for line items on this order...
                foreach (var orderReturn in previousOrderReturns)
                {
                    var previouslyRefundedLineItem = orderReturn.ItemsToReturn.FirstOrDefault(li => li.LineItemID == orderReturnItem.LineItemID);
                    if (previouslyRefundedLineItem != null)
                    {
                        previouslyRefundedAmountForThisLineItem += previouslyRefundedLineItem.RefundAmount ?? 0;
                    }
                }

                // If previous total + new line total > totalRefundIfReturningAllLineItems, then totalRefundIfReturningAllLineItems - previousTotal = newLineTotal
                if (previouslyRefundedAmountForThisLineItem + expectedLineTotalRefund > totalRefundIfReturningAllLineItems)
                {
                    var totalAfterPossibleRoundingErrors = totalRefundIfReturningAllLineItems - previouslyRefundedAmountForThisLineItem;
                    return totalAfterPossibleRoundingErrors;
                }
            }

            return expectedLineTotalRefund;
        }
    }
}
