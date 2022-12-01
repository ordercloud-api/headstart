using System.Collections.Generic;
using System.Linq;
using Headstart.Common.Models;

namespace Headstart.Common.Constants
{
    public static class LineItemStatusConstants
    {
        public static readonly Dictionary<LineItemStatus, int> EmptyStatuses = new Dictionary<LineItemStatus, int>()
        {
            { LineItemStatus.Submitted, 0 },
            { LineItemStatus.Backordered, 0 },
            { LineItemStatus.Complete, 0 },
        };

        // these statuses can be set by either the supplier or the seller, but when this user modifies the
        // line item status we do not want to notify themselves
        public static readonly List<LineItemStatus> LineItemStatusChangesDontNotifySetter = new List<LineItemStatus>()
        {
            LineItemStatus.Backordered,
        };

        // defining seller and supplier together as the current logic is the
        // seller should be able to do about anything a supplier can do
        public static readonly List<LineItemStatus> ValidSellerOrSupplierLineItemStatuses = new List<LineItemStatus>()
        {
            LineItemStatus.Backordered,
        };

        // definitions of which user contexts can can set which lineItemStatuses
        public static readonly Dictionary<VerifiedUserType, List<LineItemStatus>> ValidLineItemStatusSetByUserType = new Dictionary<VerifiedUserType, List<LineItemStatus>>()
        {
            { VerifiedUserType.admin, ValidSellerOrSupplierLineItemStatuses },
            { VerifiedUserType.supplier, ValidSellerOrSupplierLineItemStatuses },

            // requests that are not directly made to modify lineItem status, derivatives of order submit or shipping,
            // these should not be set without those trigger actions (order submit or shipping)
            { VerifiedUserType.noUser, new List<LineItemStatus> { LineItemStatus.Submitted, LineItemStatus.Complete } },
        };

        // definitions to control which line item status changes are allowed
        // for example cannot change a completed line item to anything but returned or return requested (or return denied)
        public static readonly Dictionary<LineItemStatus, List<LineItemStatus>> ValidPreviousStateLineItemChangeMap = new Dictionary<LineItemStatus, List<LineItemStatus>>()
        {
            // no previous states for submitted
            { LineItemStatus.Submitted, new List<LineItemStatus>() { } },

            /* ordering of the items in the list is used for determining which line item statuses to change
            * for example when setting status to Complete, Submitted will be the first quantity to decrement,
            * once this is depleted, the backordered quantity will be decremented */
            { LineItemStatus.Complete, new List<LineItemStatus>() { LineItemStatus.Submitted, LineItemStatus.Backordered } },
            { LineItemStatus.Backordered, new List<LineItemStatus>() { LineItemStatus.Submitted } },
        };

        private static Dictionary<LineItemStatus, SubmittedOrderStatus> relatedOrderStatus = new Dictionary<LineItemStatus, SubmittedOrderStatus>()
        {
            { LineItemStatus.Submitted, SubmittedOrderStatus.Open },
            { LineItemStatus.Backordered, SubmittedOrderStatus.Open },
            { LineItemStatus.Complete, SubmittedOrderStatus.Completed },
        };

        private static Dictionary<LineItemStatus, ShippingStatus> relatedShippingStatus = new Dictionary<LineItemStatus, ShippingStatus>()
        {
            { LineItemStatus.Submitted, ShippingStatus.Processing },
            { LineItemStatus.Backordered, ShippingStatus.Processing },
            { LineItemStatus.Complete, ShippingStatus.Shipped },
        };

        public static (SubmittedOrderStatus, ShippingStatus) GetOrderStatuses(List<HSLineItem> lineItems)
        {
            var orderStatusOccurances = new HashSet<SubmittedOrderStatus>();
            var shippingStatusOccurances = new HashSet<ShippingStatus>();

            foreach (var lineItem in lineItems)
            {
                foreach (var status in lineItem.xp.StatusByQuantity)
                {
                    if (status.Value > 0)
                    {
                        orderStatusOccurances.Add(relatedOrderStatus[status.Key]);
                        shippingStatusOccurances.Add(relatedShippingStatus[status.Key]);
                    }
                }
            }

            var orderStatus = GetOrderStatus(orderStatusOccurances);
            var shippingStatus = GetOrderShippingStatus(shippingStatusOccurances);

            return (orderStatus, shippingStatus);
        }

        public static Dictionary<LineItemStatus, Dictionary<VerifiedUserType, EmailDisplayText>> GetStatusChangeEmailText(string supplierName)
        {
            return new Dictionary<LineItemStatus, Dictionary<VerifiedUserType, EmailDisplayText>>()
            {
                {
                    LineItemStatus.Complete, new Dictionary<VerifiedUserType, EmailDisplayText>()
                    {
                        {
                            VerifiedUserType.buyer, new EmailDisplayText()
                            {
                                EmailSubject = "Items on your order have shipped",
                                DynamicText = $"{supplierName} has shipped items from your order",
                                DynamicText2 = "The following items are on their way",
                            }
                        },
                    }
                },
                {
                    LineItemStatus.Backordered, new Dictionary<VerifiedUserType, EmailDisplayText>()
                    {
                        {
                            VerifiedUserType.buyer, new EmailDisplayText()
                            {
                                EmailSubject = "Item(s) on your order have been backordered by supplier",
                                DynamicText = "You will be updated on the status of the order when more information is known",
                                DynamicText2 = "The following items have been marked as backordered",
                            }
                        },
                        {
                            VerifiedUserType.admin, new EmailDisplayText()
                            {
                                EmailSubject = $"{supplierName} has marked items on an order as backordered",
                                DynamicText = "You will be updated on the status of the order when more information is known",
                                DynamicText2 = "The following items have been marked as backordered",
                            }
                        },
                        {
                            VerifiedUserType.supplier, new EmailDisplayText()
                            {
                                EmailSubject = "Item(s) on order have been marked as backordered",
                                DynamicText = "Keep the buyer updated on the status of these items when you know more information",
                                DynamicText2 = "The following items have been marked as backordered",
                            }
                        },
                    }
                },
            };
        }

        private static SubmittedOrderStatus GetOrderStatus(HashSet<SubmittedOrderStatus> orderStatusOccurances)
        {
            if (orderStatusOccurances.Count == 1)
            {
                return orderStatusOccurances.First();
            }

            if (orderStatusOccurances.Contains(SubmittedOrderStatus.Open))
            {
                return SubmittedOrderStatus.Open;
            }

            if (orderStatusOccurances.Contains(SubmittedOrderStatus.Completed))
            {
                return SubmittedOrderStatus.Completed;
            }

            // otherwise all lineitem statuses are canceled
            return SubmittedOrderStatus.Canceled;
        }

        private static ShippingStatus GetOrderShippingStatus(HashSet<ShippingStatus> shippingStatusOccurances)
        {
            if (shippingStatusOccurances.Count == 1)
            {
                return shippingStatusOccurances.First();
            }

            if (shippingStatusOccurances.Contains(ShippingStatus.Processing) && shippingStatusOccurances.Contains(ShippingStatus.Shipped))
            {
                return ShippingStatus.PartiallyShipped;
            }

            if (shippingStatusOccurances.Contains(ShippingStatus.Shipped))
            {
                return ShippingStatus.Shipped;
            }

            if (shippingStatusOccurances.Contains(ShippingStatus.Processing))
            {
                return ShippingStatus.Processing;
            }

            // otherwise all lineitem statuses are canceled
            return ShippingStatus.Canceled;
        }
    }
}
