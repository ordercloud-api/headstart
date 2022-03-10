using System.Linq;
using Headstart.Models.Headstart;
using System.Collections.Generic;
using Headstart.Common.Models.Misc;
using Headstart.Common.Models.Headstart;
using Headstart.Common.Models.Headstart.Extended;

namespace Headstart.Common.Constants
{
	public static class LineItemStatusConstants
	{
		public static (SubmittedOrderStatus, ShippingStatus, ClaimStatus) GetOrderStatuses(List<HsLineItem> lineItems)
		{
			var orderStatusOccurrences = new HashSet<SubmittedOrderStatus>();
			var shippingStatusOccurrances = new HashSet<ShippingStatus>();
			var claimStatusOccurrences = new HashSet<ClaimStatus>();

			foreach (var lineItem in lineItems)
			{
				foreach (var status in lineItem.xp.StatusByQuantity)
				{
					if (status.Value <= 0)
					{
						continue;
					}
					orderStatusOccurrences.Add(RelatedOrderStatus[status.Key]);
					shippingStatusOccurrances.Add(RelatedShippingStatus[status.Key]);
					claimStatusOccurrences.Add(RelatedClaimStatus[status.Key]);
				}
			}
			var orderStatus = GetOrderStatus(orderStatusOccurrences);
			var shippingStatus = GetOrderShippingStatus(shippingStatusOccurrances);
			var claimStatus = GetOrderClaimStatus(claimStatusOccurrences);
			return (orderStatus, shippingStatus, claimStatus);
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
			// Otherwise all line item statuses are canceled
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
			// Otherwise all line item statuses are canceled
			return ShippingStatus.Canceled;
		}

		private static ClaimStatus GetOrderClaimStatus(HashSet<ClaimStatus> claimStatusOccurances)
		{
			if (claimStatusOccurances.Count == 1)
			{
				return claimStatusOccurances.First();
			}
			if (claimStatusOccurances.Contains(ClaimStatus.Pending))
			{
				return ClaimStatus.Pending;
			}
			if (claimStatusOccurances.Contains(ClaimStatus.Complete))
			{
				return ClaimStatus.Complete;
			}
			// Otherwise there are no claims
			return ClaimStatus.NoClaim;
		}

		public static Dictionary<LineItemStatus, int> EmptyStatuses = new Dictionary<LineItemStatus, int>()
		{
			{LineItemStatus.Submitted, 0},
			{LineItemStatus.BackOrdered, 0},
			{LineItemStatus.CancelRequested, 0},
			{LineItemStatus.CancelDenied, 0},
			{LineItemStatus.Complete, 0},
			{LineItemStatus.ReturnRequested, 0},
			{LineItemStatus.ReturnDenied, 0},
			{LineItemStatus.Returned, 0},
			{LineItemStatus.Canceled, 0},
		};

		private static readonly Dictionary<LineItemStatus, SubmittedOrderStatus> RelatedOrderStatus = new Dictionary<LineItemStatus, SubmittedOrderStatus>()
		{
			{LineItemStatus.Submitted, SubmittedOrderStatus.Open},
			{LineItemStatus.BackOrdered, SubmittedOrderStatus.Open},
			{LineItemStatus.CancelRequested, SubmittedOrderStatus.Open},
			{LineItemStatus.CancelDenied, SubmittedOrderStatus.Open},
			{LineItemStatus.Complete, SubmittedOrderStatus.Completed},
			{LineItemStatus.ReturnRequested, SubmittedOrderStatus.Completed},
			{LineItemStatus.Returned, SubmittedOrderStatus.Completed},
			{LineItemStatus.ReturnDenied, SubmittedOrderStatus.Completed},
			{LineItemStatus.Canceled, SubmittedOrderStatus.Canceled},
		};

		private static readonly Dictionary<LineItemStatus, ShippingStatus> RelatedShippingStatus = new Dictionary<LineItemStatus, ShippingStatus>()
		{
			{LineItemStatus.Submitted, ShippingStatus.Processing},
			{LineItemStatus.BackOrdered, ShippingStatus.Processing},
			{LineItemStatus.CancelRequested, ShippingStatus.Processing},
			{LineItemStatus.CancelDenied, ShippingStatus.Processing},
			{LineItemStatus.Complete, ShippingStatus.Shipped},
			{LineItemStatus.ReturnRequested, ShippingStatus.Shipped},
			{LineItemStatus.ReturnDenied, ShippingStatus.Shipped},
			{LineItemStatus.Returned, ShippingStatus.Shipped},
			{LineItemStatus.Canceled, ShippingStatus.Canceled},
		};

		private static readonly Dictionary<LineItemStatus, ClaimStatus> RelatedClaimStatus = new Dictionary<LineItemStatus, ClaimStatus>()
		{
			{LineItemStatus.Submitted, ClaimStatus.NoClaim},
			{LineItemStatus.BackOrdered, ClaimStatus.Pending},
			{LineItemStatus.CancelRequested, ClaimStatus.Pending},
			{LineItemStatus.CancelDenied, ClaimStatus.NoClaim},
			{LineItemStatus.Complete, ClaimStatus.NoClaim},
			{LineItemStatus.ReturnRequested, ClaimStatus.Pending},
			{LineItemStatus.Returned, ClaimStatus.Complete},
			{LineItemStatus.ReturnDenied, ClaimStatus.NoClaim},
			{LineItemStatus.Canceled, ClaimStatus.Complete},
		};

		// these statuses can be set by either the supplier or the seller, but when this user modifies the 
		// line item status we do not want to notify themselves
		public static readonly List<LineItemStatus> LineItemStatusChangesDontNotifySetter = new List<LineItemStatus>()
		{
			LineItemStatus.Returned,
			LineItemStatus.BackOrdered,
			LineItemStatus.Canceled,
			LineItemStatus.CancelDenied,
			LineItemStatus.ReturnDenied
		};

		// defining seller and supplier together as the current logic is the 
		// seller should be able to do about anything a supplier can do
		private static readonly List<LineItemStatus> ValidSellerOrSupplierLineItemStatuses = new List<LineItemStatus>()
		{
			LineItemStatus.Returned,
			LineItemStatus.BackOrdered,
			LineItemStatus.Canceled,
			LineItemStatus.CancelDenied,
			LineItemStatus.ReturnDenied,
			LineItemStatus.CancelRequested,
			LineItemStatus.ReturnRequested
		};

		// definitions of which user contexts can can set which lineItemStatuses
		public static readonly Dictionary<VerifiedUserType, List<LineItemStatus>> ValidLineItemStatusSetByUserType = new Dictionary<VerifiedUserType, List<LineItemStatus>>()
		{
			{VerifiedUserType.admin, ValidSellerOrSupplierLineItemStatuses},
			{VerifiedUserType.supplier, ValidSellerOrSupplierLineItemStatuses},
			{VerifiedUserType.buyer, new List<LineItemStatus>{LineItemStatus.ReturnRequested, LineItemStatus.CancelRequested}},
			// requests that are not directly made to modify lineItem status, derivatives of order submit or shipping,
			// these should not be set without those trigger actions (order submit or shipping)
			{VerifiedUserType.noUser, new List<LineItemStatus>{LineItemStatus.Submitted, LineItemStatus.Complete}}
		};

		// definitions to control which line item status changes are allowed
		// for example cannot change a completed line item to anything but returned or return requested (or return denied)
		public static readonly Dictionary<LineItemStatus, List<LineItemStatus>> ValidPreviousStateLineItemChangeMap = new Dictionary<LineItemStatus, List<LineItemStatus>>()
		{
			// no previous states for submitted
			{LineItemStatus.Submitted, new List<LineItemStatus>()},

			/* ordering of the items in the list is used for determining which line item statuses to change
			* for example when setting status to canceled, cancel requested will be the first quantity to decrement,
			* once this is depleted, the back-ordered quantity will be decremented */
			{LineItemStatus.Complete, new List<LineItemStatus>() {LineItemStatus.Submitted, LineItemStatus.BackOrdered, LineItemStatus.CancelDenied}},
			{LineItemStatus.ReturnRequested, new List<LineItemStatus>() {LineItemStatus.CancelDenied, LineItemStatus.Complete, LineItemStatus.ReturnDenied}},
			{LineItemStatus.Returned, new List<LineItemStatus>() {LineItemStatus.ReturnRequested, LineItemStatus.Complete}},
			{LineItemStatus.ReturnDenied, new List<LineItemStatus>() {LineItemStatus.ReturnRequested}},
			{LineItemStatus.BackOrdered, new List<LineItemStatus>() {LineItemStatus.Submitted}},
			{LineItemStatus.CancelRequested, new List<LineItemStatus>() {LineItemStatus.BackOrdered, LineItemStatus.Submitted, LineItemStatus.CancelDenied}},
			{LineItemStatus.Canceled, new List<LineItemStatus>() {LineItemStatus.CancelRequested, LineItemStatus.BackOrdered, LineItemStatus.Submitted}},
			{LineItemStatus.CancelDenied, new List<LineItemStatus>() {LineItemStatus.CancelRequested}},
		};

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
								EmailSubject = @"Items on your order have shipped.",
								DynamicText = @"{supplierName} has shipped items from your order.",
								DynamicText2 = @"The following items are on their way."
							}
						}
					}
				},
				{
					LineItemStatus.ReturnRequested, new Dictionary<VerifiedUserType, EmailDisplayText>()
					{
						{
							VerifiedUserType.buyer, new EmailDisplayText()
							{
								EmailSubject = @"A return request has been submitted on your order.",
								DynamicText = @"You will be updated when this return is processed.",
								DynamicText2 = @"The following items have been requested for return."
							}
						},
						{
							VerifiedUserType.admin, new EmailDisplayText()
							{
								EmailSubject = @"A buyer has submitted a return on their order.",
								DynamicText = @"Contact the Supplier to process the return request.",
								DynamicText2 = @"The following items have been requested for return."
							}
						},
						{ 
							VerifiedUserType.supplier, new EmailDisplayText()
							{
								EmailSubject = @"A buyer has submitted a return on their order.",
								DynamicText = @"The seller will contact you to process the return request.",
								DynamicText2 = @"The following items have been requested for return."
							}
						}
					}
				},
				{
					LineItemStatus.Returned, new Dictionary<VerifiedUserType, EmailDisplayText>() 
					{
						{ 
							VerifiedUserType.buyer, new EmailDisplayText()
							{
								EmailSubject = @"A return has been processed for your order.",
								DynamicText = @"You will be refunded for the proper amount.",
								DynamicText2 = @"The following items have had returns processed."
							}
						},
						{ 
							VerifiedUserType.admin, new EmailDisplayText()
							{
								EmailSubject = @"The supplier has processed a return.",
								DynamicText = @"Ensure that the full return process is complete, and the customer was refunded.",
								DynamicText2 = @"The following items have been marked as returned."
							}
						},
						{ 
							VerifiedUserType.supplier, new EmailDisplayText()
							{
								EmailSubject = @"The seller has processed a return.",
								DynamicText = @"Ensure that the full return process is complete.",
								DynamicText2 = @"The following items have been marked as returned."
							}
						}
					}
				},
				{
					LineItemStatus.ReturnDenied, new Dictionary<VerifiedUserType, EmailDisplayText>() 
					{
						{ 
							VerifiedUserType.buyer, new EmailDisplayText()
							{
								EmailSubject = @"A return has been denied for your order.",
								DynamicText = @"A return could not be processed for this order.",
								DynamicText2 = @"The following items will not be returned."
							}
						},
						{ 
							VerifiedUserType.admin, new EmailDisplayText()
							{
								EmailSubject = @"The supplier has denied a return.",
								DynamicText = @"The customer will not be refunded for the following items.",
								DynamicText2 = @"The following items have been marked as return denied."
							}
						},
						{ 
							VerifiedUserType.supplier, new EmailDisplayText()
							{
								EmailSubject = @"The supplier has denied a return.",
								DynamicText = @"The customer will not be refunded for the following items.",
								DynamicText2 = @"The following items have been marked as return denied."
							}
						}
					}
				},
				{
					LineItemStatus.BackOrdered, new Dictionary<VerifiedUserType, EmailDisplayText>() 
					{
						{ 
							VerifiedUserType.buyer, new EmailDisplayText()
							{
								EmailSubject = @"Item(s) on your order have been back-ordered by supplier.",
								DynamicText = @"You will be updated on the status of the order when more information is known.",
								DynamicText2 = @"The following items have been marked as back-ordered."
							}
						},
						{ 
							VerifiedUserType.admin, new EmailDisplayText()
							{
								EmailSubject = $"{supplierName} has marked items on an order as back-ordered.",
								DynamicText = @"You will be updated on the status of the order when more information is known.",
								DynamicText2 = @"The following items have been marked as back-ordered."
							}
						},
						{ 
							VerifiedUserType.supplier, new EmailDisplayText()
							{
								EmailSubject = @"Item(s) on order have been marked as back-ordered.",
								DynamicText = @"Keep the buyer updated on the status of these items when you know more information.",
								DynamicText2 = @"The following items have been marked as back-ordered."
							}
						}
					}
				},
				{
					LineItemStatus.CancelRequested, new Dictionary<VerifiedUserType, EmailDisplayText>() 
					{
						{ 
							VerifiedUserType.buyer, new EmailDisplayText()
							{
								EmailSubject = @"Your request for cancellation has been submitted.",
								DynamicText = @"You will be updated on the status of the cancellation when more information is known.",
								DynamicText2 = @"The following items have had cancellation requested."
							}
						},
						{ 
							VerifiedUserType.admin, new EmailDisplayText()
							{
								EmailSubject = @"A buyer has requested cancellation of line items on an order.",
								DynamicText = @"The supplier will look into the feasibility of this cancellation.",
								DynamicText2 = @"The following items have been requested for cancellation."
							}
						},
						{ 
							VerifiedUserType.supplier, new EmailDisplayText()
							{
								EmailSubject = @"A buyer has requested cancellation of line items on an order.",
								DynamicText = @"Review the items below to see if any can be cancelled before they ship.",
								DynamicText2 = @"The following items have have been requested for cancellation."
							}
						}
					}
				},
				{
					LineItemStatus.Canceled, new Dictionary<VerifiedUserType, EmailDisplayText>() 
					{
						{ 
							VerifiedUserType.buyer, new EmailDisplayText()
							{
								EmailSubject = @"Items on your order have been cancelled.",
								DynamicText = @"You will be refunded for the cost of these items.",
								DynamicText2 = @"The following items have been cancelled."
							}
						},
						{ 
							VerifiedUserType.admin, new EmailDisplayText()
							{
								EmailSubject = @"Item(s) on an order have been cancelled.",
								DynamicText = @"Ensure the buyer is refunded for the proper amount.",
								DynamicText2 = @"The following items have been cancelled."
							}
						},
						{ 
							VerifiedUserType.supplier, new EmailDisplayText()
							{
								EmailSubject = @"Item(s) on an order have been cancelled.",
								DynamicText = @"The seller will refund the buyer for the proper amount.",
								DynamicText2 = @"The following items have been cancelled."
							}
						}
					}
				},
				{
					LineItemStatus.CancelDenied, new Dictionary<VerifiedUserType, EmailDisplayText>()
					{
						{ 
							VerifiedUserType.buyer, new EmailDisplayText()
							{
								EmailSubject = @"A cancellation has been denied for your order.",
								DynamicText = @"A cancellation could not be processed for this order.",
								DynamicText2 = @"The following items will not be canceled."
							}
						},
						{ 
							VerifiedUserType.admin, new EmailDisplayText()
							{
								EmailSubject = @"The supplier has denied a cancellation.",
								DynamicText = @"The customer will not be refunded for the following items.",
								DynamicText2 = @"The following items have been marked as cancel denied."
							}
						},
						{ 
							VerifiedUserType.supplier, new EmailDisplayText()
							{
								EmailSubject = @"The supplier has denied a cancellation.",
								DynamicText = @"The customer will not be refunded for the following items.",
								DynamicText2 = @"The following items have been marked as cancel denied."
							}
						}
					}
				}
			};
		}
	}
}