using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ordercloud.integrations.library;
using Newtonsoft.Json;
using Headstart.Models.Headstart;
using System.Linq;
using System;

namespace Headstart.Models.Misc
{
	public class EnvironmentSeed
	{
        /// <summary>
        /// The username for logging in to https://portal.ordercloud.io
        /// </summary>
        [Required]
		public string PortalUsername { get; set; }

		/// <summary>
		/// The password for logging in to https://portal.ordercloud.io
		/// </summary>
		[Required]
		public string PortalPassword { get; set; }

		/// <summary>
		/// The username for the admin user you will log in with after seeding
		/// </summary>
		[Required]
		public string InitialAdminUsername { get; set; }

		/// <summary>
		/// The password for the admin user you will log in with after seeding
		/// </summary>
		[Required]
		[RegularExpression("^(?=.{10,100}$)(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*\\W).*$", ErrorMessage = "Password must contain one number, one uppercase letter, one lowercase letter, one special character and have a minimum of 10 characters total")]
		public string InitialAdminPassword { get; set; }

		/// <summary>
		/// The url to your hosted middleware endpoint
		/// needed for webhooks and message senders
		/// </summary>
		[Required]
		public string MiddlewareBaseUrl { get; set; }

		/// <summary>
		/// Container for OrderCloud Settings
		/// </summary>
		[Required]
		public OrderCloudSeedSettings OrderCloudSeedSettings { get; set; }

        /// <summary>
        /// Optionally pass in a value if you have an existing marketplace you would like to seed. If no value is present a new marketplace will be created
        /// Creating a marketplace via seeding is only possible in the sandbox api environment
        /// </summary>
        public string MarketplaceID { get; set; }

		/// <summary>
		/// Optionally pass in a marketplace name when first creating a marketplace
		/// </summary>
		public string MarketplaceName { get; set; }

		/// <summary>
		/// An optional array of suppliers to create as part of the initial seeding
		/// </summary>
		public List<HSSupplier> Suppliers { get; set; } = new List<HSSupplier> { };

		/// <summary>
		/// An optional array of buyers to create as part of the initial seeding
		/// </summary>
		public List<HSBuyer> Buyers { get; set; } = new List<HSBuyer> { };

		/// <summary>
		/// Defaults to true
		/// Enables anonymous shopping whereby users do not have to be logged in to view products or submit an order
		/// pricing and visibility will be determined by what the default user can see
		/// </summary>
		public bool EnableAnonymousShopping { get; set; } = true;

		/// <summary>
		/// An optional string to specify a buyer which the anonymous buyer user will be assigned to
		/// Otherwise we will assign the default buyer that is created in seeding.
		/// </summary>
		public string AnonymousShoppingBuyerID { get; set; }

		/// <summary>
		/// An optional object of storage settings for your translations container.
		/// If none are provided the seeding funciton will not create a translation file or downloads file
		/// Provide a valid ConnectionString to have the seeding function generate your translation file
		/// </summary>
		public StorageAccountSeedSettings StorageAccountSettings { get; set; }
    }

    public class EnvironmentSeedResponse
    {
		public string Comments { get; set; }
		public string MarketplaceName { get; set; }
		public string MarketplaceID { get; set; }
		public string OrderCloudEnvironment { get; set; }
		public Dictionary<string, dynamic> ApiClients { get; set; }
    }

	public class OrderCloudSeedSettings
    {
		/// <summary>
		/// The ordercloud environment
		/// </summary>
		[Required]
		[ValueRange(AllowableValues = new[] { "production", "sandbox" })]
		public string Environment { get; set; }

		/// <summary>
		/// Optionally provide an region for your new marketplace to be hosted in.
		/// Options are US-West, US-East, Australia-East, Europe-West, Japan-East.
		/// If no value is provided US-West will be used by default.
		/// https://ordercloud.io/knowledge-base/ordercloud-regions
		/// </summary>
		[ValueRange(AllowableValues = new[] { "", null, "US-East", "Australia-East", "Europe-West", "Japan-East", "US-West" })]
		public string Region { get; set; }

		/// <summary>
		/// Used to secure your webhook endpoints
		/// provide a secure, non-guessable string
		/// </summary>
		[Required, MaxLength(15)]
		public string WebhookHashKey { get; set; }
	}

	public class StorageAccountSeedSettings
    {
		[Required]
		public string ConnectionString { get; set; }
		public string ContainerNameTranslations { get; set; } = "ngx-translate";
		public string ContainerNameDownloads { get; set; } = "downloads";
	}

	public class ValueRange : ValidationAttribute
    {
		public string[] AllowableValues { get; set; }

		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			if (AllowableValues?.Contains(value?.ToString(), StringComparer.OrdinalIgnoreCase) == true)
			{
				return ValidationResult.Success;
			}
			var msg = $"Please enter one of the allowable values: {string.Join(", ", (AllowableValues ?? new string[] { "No allowable values found" }))}.";
			return new ValidationResult(msg);
		}
	}

	public class OcEnv
    {
        public string EnvironmentName { get; set; }
		public string ApiUrl { get; set; }
        public Region Region { get; set; }
    }

	public class Region
	{
		public string AzureRegion { get; set; }
		public string Id { get; set; }
		public string Name { get; set; }

	}
}
