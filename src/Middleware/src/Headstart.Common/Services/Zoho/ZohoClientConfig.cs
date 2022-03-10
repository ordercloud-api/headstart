namespace Headstart.Common.Services.Zoho
{
	public class ZohoClientConfig
	{
		public string AccessToken { get; set; } = string.Empty;

		public string OrganizationId { get; set; } = string.Empty;

		public string ApiUrl { get; set; } = $@"https://books.zoho.com/api/v3";

		public string ClientId { get; set; } = string.Empty;

		public string ClientSecret { get; set; } = string.Empty;
	}
}