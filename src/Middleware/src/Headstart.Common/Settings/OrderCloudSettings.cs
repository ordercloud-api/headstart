namespace Headstart.Common.Settings
{
    public class OrderCloudSettings
    {
        public string ApiUrl { get; set; }

        public string ClientIDsWithAPIAccess { get; set; } = string.Empty; // Comma-separated list

        public string IncrementorPrefix { get; set; }

        public string MiddlewareClientID { get; set; }

        public string MiddlewareClientSecret { get; set; }

        public string MarketplaceID { get; set; }

        public string MarketplaceName { get; set; } // used for display purposes

        public string WebhookHashKey { get; set; }
    }
}
