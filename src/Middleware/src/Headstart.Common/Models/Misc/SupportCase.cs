using Microsoft.AspNetCore.Http;

namespace Headstart.Common.Models.Misc
{
	public class SupportCase
	{
		public string FirstName { get; set; } = string.Empty;

		public string LastName { get; set; } = string.Empty;

		public string Email { get; set; } = string.Empty;

		public string Vendor { get; set; } = string.Empty;

		public string Subject { get; set; } = string.Empty;

		public string Message { get; set; } = string.Empty;

		public IFormFile File { get; set; }
	}
}