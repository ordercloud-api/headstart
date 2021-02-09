using ordercloud.integrations.library;
using System.ComponentModel.DataAnnotations;

namespace ordercloud.integrations.cardconnect
{
	[SwaggerModel]
	public class OrderCloudIntegrationsCreditCardPayment
    {
        [OrderCloud.SDK.Required]
        public string OrderID { get; set; }
        [OrderCloud.SDK.Required]
        public string PaymentID { get; set; }
        public string CreditCardID { get; set; } // Use for saved Credit Cards
        public OrderCloudIntegrationsCreditCardToken CreditCardDetails { get; set; }  // Use for one-time Credit Cards
        [OrderCloud.SDK.Required]
        [MinLength(3, ErrorMessage = "Invalid currency specified: Must be 3 digit code. Ex: USD or CAD")]
        [MaxLength(3, ErrorMessage = "Invalid currency specified: Must be 3 digit code. Ex: USD or CAD")]
        public string Currency { get; set; }
        [MinLength(3, ErrorMessage = "Invalid CVV: Must be 3 or 4 digit code.")]
        [MaxLength(4, ErrorMessage = "Invalid CVV: Must be 3 or 4 digit code.")]
        public string CVV { get; set; }
        [OrderCloud.SDK.Required]
        public string MerchantID { get; set; }
    }
}
