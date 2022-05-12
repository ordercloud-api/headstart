using Headstart.Models;
using Headstart.Models.Headstart;
using Microsoft.AspNetCore.Mvc;
using OrderCloud.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Headstart.Common.Constants
{
    class OrderSubmitEmailConstants
    {
        public enum OrderEmailTypes
        {
            OrderSubmitted,
            QuoteOrderSubmitted,
            RequiresApproval,
            RequestedApproval,
            Approved,
            Declined
        }


        public static EmailDisplayText GetOrderSubmitText(string orderID, string firstName, string lastName, VerifiedUserType decodedToken)
        {
            var dictionary = new Dictionary<VerifiedUserType, EmailDisplayText>()
            {
                {VerifiedUserType.buyer, new EmailDisplayText()
                {
                    EmailSubject = $"Your order has been submitted {orderID}",
                    DynamicText = "Thank you for your order.",
                    DynamicText2 = "We are getting your order ready to be shipped. You will be notified when it has been sent. Your order contains the folowing items."
                } },
                {VerifiedUserType.admin, new EmailDisplayText()
                {
                    EmailSubject = $"An order has been submitted {orderID}",
                    DynamicText = $"{firstName} {lastName} has placed an order.",
                    DynamicText2 = "The order contains the following items:"
                } },
                {VerifiedUserType.supplier, new EmailDisplayText()
                {
                    EmailSubject = $"An order has been submitted {orderID}",
                    DynamicText = $"{firstName} {lastName} has placed an order.",
                    DynamicText2 = "The order contains the following items:"
                } },
            };
            return dictionary[decodedToken];
        }
        public static EmailDisplayText GetQuoteOrderSubmitText(VerifiedUserType decodedToken)
        {
            var dictionary = new Dictionary<VerifiedUserType, EmailDisplayText>()
            {
                {VerifiedUserType.buyer, new EmailDisplayText()
                {
                    EmailSubject = "Your quote has been submitted",
                    DynamicText = "Your quote has been submitted.",
                    DynamicText2 = "The vendor for this product will contact your with more information on your quote request."
                } },
                {VerifiedUserType.supplier, new EmailDisplayText()
                {
                    EmailSubject = "A quote has been requested",
                    DynamicText = "A quote has been requested for one of your products",
                    DynamicText2 = "Please reach out to the customer directly to give them more information about their quote."
                } },
            };
            return dictionary[decodedToken];
        }
        public static EmailDisplayText GetOrderRequiresApprovalText()
        {
            return new EmailDisplayText()
            {
                EmailSubject = "An order requires your approval",
                DynamicText = "An order requires your approval. Please review the order and approve or decline the order.",
                DynamicText2 = "The order for approval contains the following items"
            };
        }
        public static EmailDisplayText GetRequestedApprovalText()
        {
            return new EmailDisplayText()
            {
                EmailSubject = "Your order was sent for approval",
                DynamicText = "Your order was sent for approval. You will receive an email when the order is approved.",
                DynamicText2 = "Your order awaiting approval contains the following items"
            };
        }
        public static EmailDisplayText GetOrderApprovedText()
        {
            return new EmailDisplayText()
            {
                EmailSubject = "Your order was approved",
                DynamicText = "Your order was approved and submitted.",
                DynamicText2 = "Your order contains the following items"
            };
        }
        public static EmailDisplayText GetOrderDeclinedText()
        {
            return new EmailDisplayText()
            {
                EmailSubject = "Your order was declined",
                DynamicText = "Your order was declined. Please review the order and re-submit the order again.",
                DynamicText2 = "Your declined order contains the following items"
            };
        }

        public static EmailDisplayText GetQuoteRequestConfirmationText()
        {
            return new EmailDisplayText()
            {
                EmailSubject = "Your quote request has been submitted",
                DynamicText = "A quote request has been submitted.",
                DynamicText2 = "You will recieve a notification once your quote request has been quoted."
            };
        }

        public static EmailDisplayText GetQuotePriceConfirmationText()
        {
            return new EmailDisplayText()
            {
                EmailSubject = "Your order has been quoted",
                DynamicText = "A quote has been offered on your order. Please review the order to see this quote.",
                DynamicText2 = "You will have an opportunity to purchase the product at the quoted price, or to reopen the quote request."
            };
        }
    }
}
