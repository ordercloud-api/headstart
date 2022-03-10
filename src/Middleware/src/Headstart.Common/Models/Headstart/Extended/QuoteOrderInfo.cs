using System.ComponentModel.DataAnnotations;

namespace Headstart.Common.Models.Headstart.Extended
{
	public class QuoteOrderInfo
	{
		[Required]
		public string FirstName { get; set; } = string.Empty;

		[Required]
		public string LastName { get; set; } = string.Empty;

		public string BuyerLocation { get; set; } = string.Empty;

		[Required]
		public string Phone { get; set; } = string.Empty;

		[Required]
		public string Email { get; set; } = string.Empty;

		[MaxLength(200, ErrorMessage = @"Quote request comments cannot exceed 200 characters.")]
		public string Comments { get; set; } = string.Empty;

		public string ShippingAddressId { get; set; } = string.Empty;
	}
}