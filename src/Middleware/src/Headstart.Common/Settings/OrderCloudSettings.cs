namespace Headstart.Common.Settings
{
    public class OrderCloudSettings
    {
        /// <summary>
        /// URL to connect to OrderCloud such as https://useast-sandbox.ordercloud.io
        /// https://ordercloud.io/knowledge-base/ordercloud-supported-regions
        /// </summary>
        public string ApiUrl { get; set; }

        /// <summary>
        /// Comma-separated string of API Client IDs that should have API access to the middleware
        /// This is used as a security feature to prevent malicious actors from calling the middleware API.
        /// </summary>
        public string ClientIDsWithAPIAccess { get; set; } = string.Empty; // Comma-separated list

        /// <summary>
        /// A prefix used in all orderIDs to create a distinct incrementor
        /// </summary>
        public string IncrementorPrefix { get; set; }

        /// <summary>
        /// ClientID used to interface with OrderCloud.
        /// </summary>
        public string MiddlewareClientID { get; set; }

        /// <summary>
        /// Client Secret used to interface with OrderCloud.
        /// </summary>
        public string MiddlewareClientSecret { get; set; }

        /// <summary>
        /// The ID of the Marketplace
        /// </summary>
        public string MarketplaceID { get; set; }

        /// <summary>
        /// The Name of your Marketplace
        /// If you're a marketplace owner that is participating in commerce then any orders placed for your products
        /// will display MarketplaceName as the company that the user is purchasing from.
        /// </summary>
        public string MarketplaceName { get; set; }

        /// <summary>
        /// A non-guessable string used to validate requests from Webhooks and Integration Events are coming from OrderCloud.
        /// </summary>
        public string WebhookHashKey { get; set; }
    }
}
