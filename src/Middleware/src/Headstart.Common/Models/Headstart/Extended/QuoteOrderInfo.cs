using System.ComponentModel.DataAnnotations;
using ordercloud.integrations.library;

namespace Headstart.Models.Extended
{
    public class QuoteOrderInfo
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        public string BuyerLocation { get; set; }
        [Required]
        public string Phone { get; set; }
        [Required]
        public string Email { get; set; }
        [MaxLength(200, ErrorMessage = "Quote request comments cannot exceed 200 characters")]
        public string Comments { get; set; }
        public string ShippingAddressId { get; set; }
    }
}
