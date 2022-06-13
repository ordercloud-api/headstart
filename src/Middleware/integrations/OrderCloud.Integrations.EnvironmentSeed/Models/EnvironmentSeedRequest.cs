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

        /// <summary>
        /// An optional object of <see cref="StorageAccountSeedSettings"/> for your translations container.
        /// If none are provided the seeding funciton will not create a translation file or downloads file
        /// Provide a valid ConnectionString to have the seeding function generate your translation file.
        /// </summary>
        public StorageAccountSeedSettings StorageAccountSettings { get; set; }
    }
}
