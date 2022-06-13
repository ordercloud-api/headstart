using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Headstart.Common.Models;
using OrderCloud.Integrations.EnvironmentSeed.Attributes;

namespace OrderCloud.Integrations.EnvironmentSeed.Models
{
    public class MarketplaceSettings
    {
        /// <summary>
        /// The ordercloud environment.
        /// </summary>
        [Required]
        public OrderCloudEnvironment Environment { get; set; }

        /// <summary>
        /// Optionally provide an region for your new marketplace to be hosted in.
        /// Options are US-West, US-East, Australia-East, Europe-West, Japan-East.
        /// If no value is provided US-West will be used by default.
        /// https://ordercloud.io/knowledge-base/ordercloud-regions.
        /// </summary>
        [ValueRange(AllowableValues = new[] { "", null, "US-East", "Australia-East", "Europe-West", "Japan-East", "US-West" })]
        public string Region { get; set; }

        /// <summary>
        /// Optionally pass in a marketplace ID if you have an existing marketplace you would like to seed. If no value is present a new marketplace will be created
        /// Creating a marketplace via seeding is only possible in the sandbox api environment.
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// Optionally pass in a marketplace name when first creating a marketplace.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// <see cref="AccountCredentials"/> for initial admin user.
        /// </summary>
        [Required]
        public AccountCredentials InitialAdmin { get; set; }

        /// <summary>
        /// Defaults to true
        /// Enables anonymous shopping whereby users do not have to be logged in to view products or submit an order
        /// pricing and visibility will be determined by what the default user can see.
        /// </summary>
        public bool EnableAnonymousShopping { get; set; } = true;

        /// <summary>
        /// An optional string to specify a buyer which the anonymous buyer user will be assigned to
        /// Otherwise we will assign the default buyer that is created in seeding.
        /// </summary>
        public string AnonymousShoppingBuyerID { get; set; }

        /// <summary>
        /// The url to your hosted middleware endpoint
        /// needed for webhooks and message senders.
        /// </summary>
        [Required]
        public string MiddlewareBaseUrl { get; set; }

        /// <summary>
        /// Used to secure your webhook endpoints
        /// provide a secure, non-guessable string.
        /// </summary>
        [Required, MaxLength(15)]
        public string WebhookHashKey { get; set; }

        /// <summary>
        /// An optional array of suppliers to create as part of the initial seeding.
        /// </summary>
        public List<HSSupplier> Suppliers { get; set; } = new List<HSSupplier> { };

        /// <summary>
        /// An optional array of buyers to create as part of the initial seeding.
        /// </summary>
        public List<HSBuyer> Buyers { get; set; } = new List<HSBuyer> { };
    }
}
