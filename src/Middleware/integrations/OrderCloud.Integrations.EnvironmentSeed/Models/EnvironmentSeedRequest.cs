using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace OrderCloud.Integrations.EnvironmentSeed.Models
{
    public class EnvironmentSeedRequest
    {
        /// <summary>
        /// <see cref="AccountCredentials"/> for logging in to https://portal.ordercloud.io.
        /// </summary>
        [Required]
        public AccountCredentials Portal { get; set; }

        /// <summary>
        /// Container for OrderCloud <see cref="MarketplaceSettings"/>.
        /// </summary>
        [Required]
        public MarketplaceSettings Marketplace { get; set; }
    }
}
