namespace Headstart.Common.Services.Zoho.Models
{
	public class ZohoTokenResponse
	{
		public string access_token { get; set; } = string.Empty;

		public string api_domain { get; set; } = string.Empty;

		public string token_type { get; set; } = string.Empty;

		public double expires_in { get; set; }
	}
}