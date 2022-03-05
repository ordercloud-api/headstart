using OrderCloud.SDK;
using Headstart.Models;
using System.Collections.Generic;

namespace Headstart.Common.Models
{
    public static class SendGridModels
    {
        public class EmailTemplate<T>
        {
            public T Data { get; set; }

            public EmailDisplayText Message { get; set; } = new EmailDisplayText();
        }

        public class PasswordResetData
        {
            public string FirstName { get; set; } = string.Empty;

            public string LastName { get; set; } = string.Empty;

            public string Username { get; set; } = string.Empty;

            public string PasswordRenewalVerificationCode { get; set; } = string.Empty;

            public string PasswordRenewalAccessToken { get; set; } = string.Empty;

            public string PasswordRenewalUrl { get; set; } = string.Empty;
        }

        public class LineItemStatusChangeData
        {
            public string FirstName { get; set; } = string.Empty;

            public string LastName { get; set; } = string.Empty;

            public List<LineItemProductData> Products { get; set; } = new List<LineItemProductData>();

            public string DateSubmitted { get; set; } = string.Empty;

            public string OrderID { get; set; } = string.Empty;

            public string Comments { get; set; } = string.Empty;

            public string TrackingNumber { get; set; } = string.Empty;
        }

        public class LineItemProductData
        {
            public string ProductName { get; set; } = string.Empty;

            public string ImageURL { get; set; } = string.Empty;

            public string ProductID { get; set; } = string.Empty;

            public int? Quantity { get; set; }

            public decimal? LineTotal { get; set; }

            public int? QuantityChanged { get; set; }

            public string SpecCombo { get; set; } = string.Empty;

            public string MessageToBuyer { get; set; } = string.Empty;
        }

        public class NewUserData
        {
            public string FirstName { get; set; } = string.Empty;

            public string LastName { get; set; } = string.Empty;

            public string PasswordRenewalAccessToken { get; set; } = string.Empty;

            public string BaseAppURL { get; set; } = string.Empty;

            public string Username { get; set; } = string.Empty;
        }

        public class ProductInformationRequestData
        {
            public string ProductID { get; set; } = string.Empty;

            public string ProductName { get; set; } = string.Empty;

            public string FirstName { get; set; } = string.Empty;

            public string LastName { get; set; } = string.Empty;

            public string Phone { get; set; } = string.Empty;

            public string Location { get; set; } = string.Empty;

            public string Email { get; set; } = string.Empty;

            public string Note { get; set; } = string.Empty;
        }

        public class SupportTemplateData
        {
            public string OrderID { get; set; } = string.Empty;

            public string ErrorJsonString { get; set; } = string.Empty;

            public string DynamicPropertyName1 { get; set; } = string.Empty;

            public string DynamicPropertyValue1 { get; set; } = string.Empty;

            public string DynamicPropertyName2 { get; set; } = string.Empty;

            public string DynamicPropertyValue2 { get; set; } = string.Empty;

            public string DynamicPropertyName3 { get; set; } = string.Empty;

            public string DynamicPropertyValue3 { get; set; } = string.Empty;

            public string DynamicPropertyName4 { get; set; } = string.Empty;

            public string DynamicPropertyValue4 { get; set; } = string.Empty;
        }

        public class OrderTemplateData
        {
            public string FirstName { get; set; } = string.Empty;

            public string LastName { get; set; } = string.Empty;

            public string OrderID { get; set; } = string.Empty;

            public string DateSubmitted { get; set; } = string.Empty;

            public string ShippingAddressID { get; set; } = string.Empty;

            public Address ShippingAddress { get; set; } = new Address();

            public string BillingAddressID { get; set; } = string.Empty;

            public Address BillingAddress { get; set; } = new Address();

            public Address BillTo { get; set; } = new Address();

            public IEnumerable<LineItemProductData> Products { get; set; } = new List<LineItemProductData>();

            public decimal? Subtotal { get; set; }

            public decimal? TaxCost { get; set; }

            public decimal? ShippingCost { get; set; }

            public decimal? PromotionalDiscount { get; set; }

            public decimal? Total { get; set; }

            public string Currency { get; set; } = string.Empty;

            public string Comments { get; set; } = string.Empty;
        }

        public class QuoteOrderTemplateData
        {
            public string FirstName { get; set; } = string.Empty;

            public string LastName { get; set; } = string.Empty;

            public string Phone { get; set; } = string.Empty;

            public string Email { get; set; } = string.Empty;

            public string Location { get; set; } = string.Empty;

            public string ProductID { get; set; } = string.Empty;

            public string ProductName { get; set; } = string.Empty;

            public decimal? UnitPrice { get; set; }

            public string Currency { get; set; } = string.Empty;

            public HSOrder Order { get; set; } = new HSOrder();
        }
    }
}