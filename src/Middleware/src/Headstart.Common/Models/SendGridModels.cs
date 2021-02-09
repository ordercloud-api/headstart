using Headstart.Models;
using OrderCloud.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Headstart.Common.Models
{
    public class SendGridModels
    {
        public class EmailTemplate<T>
        {
            public T Data { get; set; }
            public EmailDisplayText Message { get; set; }
        }

        public class PasswordResetData
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string PasswordRenewalAccessToken { get; set; }
            public string PasswordRenewalUrl { get; set; }
        }

        public class LineItemStatusChangeData
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public List<LineItemProductData> Products { get; set; }
            public string DateSubmitted { get; set; }
            public string OrderID { get; set; }
            public string Comments { get; set; }
            public string TrackingNumber { get; set; }
        }

        public class LineItemProductData
        {
            public string ProductName { get; set; }
            public string ImageURL { get; set; }
            public string ProductID { get; set; }
            public int? Quantity { get; set; }
            public decimal? LineTotal { get; set; }
            public int? QuantityChanged { get; set; }
            public string SpecCombo { get; set; }
        }

        public class ProductUpdateData
        {
            public string date { get; set; }
        }

        public class NewUserData
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string PasswordRenewalAccessToken { get; set; }
            public string BaseAppURL { get; set; }
            public string Username { get; set; }
        }

        public class InformationRequestData
        {
            public string ProductID { get; set; }
            public string ProductName { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Phone { get; set; }
            public string Location { get; set; }
            public string Email { get; set; }
            public string Note { get; set; }
        }

        public class SupportTemplateData
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string Vendor { get; set; }
            public string orderID { get; set; }
            public string BuyerID { get; set; }
            public string Username { get; set; }
            public string PaymentID { get; set; }
            public string TransactionID { get; set; }
            public string ErrorJsonString { get; set; }

        }


        public class OrderTemplateData
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string OrderID { get; set; }
            public string DateSubmitted { get; set; }
            public string ShippingAddressID { get; set; }
            public Address ShippingAddress { get; set; }
            public string BillingAddressID { get; set; }
            public Address BillingAddress { get; set; }
            public Address BillTo { get; set; }
            public IEnumerable<LineItemProductData> Products { get; set; }
            public decimal? Subtotal { get; set; }
            public decimal? TaxCost { get; set; }
            public decimal? ShippingCost { get; set; }
            public decimal? PromotionalDiscount { get; set; }
            public decimal? Total { get; set; }
            public  string Currency { get; set; }
        }

        public class QuoteOrderTemplateData
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Phone { get; set; }
            public string Email { get; set; }
            public string Location { get; set; }
            public string ProductID { get; set; }
            public string ProductName { get; set; }
            public HSOrder Order { get; set; }
        }

    }
}
