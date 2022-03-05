namespace Headstart.Common.Services.Portal.Models
{
    public class MarketplaceTokenResponse
    {
        public string[] coreuser_roles { get; set; }

        public string[] portaluser_roles { get; set; }

        public string[] granted_roles { get; set; }

        public string access_token { get; set; } = string.Empty;

        public string refresh_token { get; set; } = string.Empty;

        public string token_type => $@"bearer";

        public int expires_in { get; set; }
    }
}