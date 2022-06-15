using System.ComponentModel.DataAnnotations;
using OrderCloud.SDK;

namespace Headstart.Common.Models
{
    public class CCToken
    {
        public string AccountNumber { get; set; }

        [OrderCloud.SDK.Required]
        [MinLength(4, ErrorMessage = "Invalid expiration date format: MMYY or MMYYYY")]
        [MaxLength(6, ErrorMessage = "Invalid expiration date format: MMYY or MMYYYY")]
        public string ExpirationDate { get; set; }

        public string CardholderName { get; set; }

        public string CardType { get; set; }

        [OrderCloud.SDK.Required]
        public Address CCBillingAddress { get; set; }
    }
}
