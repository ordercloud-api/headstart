using System.Collections.Generic;

namespace OrderCloud.Integrations.EnvironmentSeed.Models
{
    public class EnvironmentSeedResponse
    {
        public string Comments { get; set; }

        public string MarketplaceName { get; set; }

        public string MarketplaceID { get; set; }

        public string OrderCloudEnvironment { get; set; }

        public Dictionary<string, dynamic> ApiClients { get; set; }

        public bool Success { get; set; } = true;
    }
}
