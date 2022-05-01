using System.Linq;
using System.Collections.Generic;
using Headstart.Common.Models.Headstart;
using System.ComponentModel.DataAnnotations;
using System;

namespace Headstart.Common.Models.Misc
{
	public class EnvironmentSeed
	{
		#region Required settings
		/// <summary>
		/// The username for logging in to https://portal.ordercloud.io
		/// </summary>
		[Required]
		public string PortalUsername { get; set; } = string.Empty;

		/// <summary>
		/// The password for logging in to https://portal.ordercloud.io
		/// </summary>
		[Required]
		public string PortalPassword { get; set; } = string.Empty;

		/// <summary>
		/// The username for the admin user you will log in with after seeding
		/// </summary>
		[Required]
		public string InitialAdminUsername { get; set; } = string.Empty;

		/// <summary>
		/// The password for the admin user you will log in with after seeding
		/// </summary>
		[Required]
		[StringLength(100, ErrorMessage = @"Password must be at least 10 characters long and maximum 100 characters long.", MinimumLength = 10)]
		[RegularExpression(@"^(?=.{10,}$)(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*\W).*$", ErrorMessage = @"Password must contain one number, one uppercase letter, one lowercase letter, one special character and have a minimum of 10 characters total.")]
		public string InitialAdminPassword { get; set; } = string.Empty;

		/// <summary>
		/// The url to your hosted middleware endpoint
		/// needed for webhooks and message senders
		/// </summary>
		[Required]
		public string MiddlewareBaseUrl { get; set; } = string.Empty;

		/// <summary>
		/// Container for OrderCloud Settings
		/// </summary>
		[Required]
		public OrderCloudSeedSettings OrderCloudSeedSettings { get; set; } = new OrderCloudSeedSettings();
		#endregion

		#region Optional settings
		/// <summary>
		/// Optionally pass in a value if you have an existing marketplace you would like to seed. If no value is present a new marketplace will be created
		/// Creating a marketplace via seeding is only possible in the sandbox api environment
		/// </summary>
		public string MarketplaceId { get; set; } = string.Empty;

		/// <summary>
		/// Optionally pass in a marketplace name when first creating a marketplace
		/// </summary>
		public string MarketplaceName { get; set; } = string.Empty;

		/// <summary>
		/// An optional array of suppliers to create as part of the initial seeding
		/// </summary>
		public List<HsSupplier> Suppliers { get; set; } = new List<HsSupplier>();

		/// <summary>
		/// An optional array of buyers to create as part of the initial seeding
		/// </summary>
		public List<HsBuyer> Buyers { get; set; } = new List<HsBuyer>();

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
		public string AnonymousShoppingBuyerId { get; set; } = string.Empty;

		/// <summary>
		/// An optional object of storage settings for your translations container. 
		/// If none are provided the seeding funciton will not create a translation file or downloads file
		/// Provide a valid ConnectionString to have the seeding function generate your translation file
		/// </summary>
		public StorageAccountSeedSettings StorageAccountSettings { get; set; } = new StorageAccountSeedSettings();

		/// <summary>
		/// Optionally provide an region for your new marketplace to be hosted in.
		/// Options are US-West, US-East, Australia-East, Europe-West, Japan-East.
		/// If no value is provided US-West will be used by default.
		/// https://ordercloud.io/knowledge-base/ordercloud-regions
		/// </summary>
		[ValueRange(AllowableValues = new[] { "", null, "US-East", "Australia-East", "Europe-West", "Japan-East", "US-West" })]
		public string Region { get; set; } = string.Empty;
		#endregion
	}

	public class EnvironmentSeedResponse
	{
		public string Comments { get; set; } = string.Empty;

		public string MarketplaceName { get; set; } = string.Empty;

		public string MarketplaceId { get; set; } = string.Empty;

		public string OrderCloudEnvironment { get; set; } = string.Empty;

		public Dictionary<string, dynamic> ApiClients { get; set; } = new Dictionary<string, dynamic>();
	}

	public class OrderCloudSeedSettings
	{
		/// <summary>
		/// The ordercloud environment
		/// </summary>
		[Required]
		[ValueRange(AllowableValues = new[] {@"production", @"sandbox"})]
		public string Environment { get; set; } = string.Empty;

		/// <summary>
		/// Used to secure your webhook endpoints
		/// provide a secure, non-guessable string
		/// </summary>
		[Required, MaxLength(15)]
		public string WebhookHashKey { get; set; } = string.Empty;
	}

	public class StorageAccountSeedSettings
	{
		[Required]
		public string ConnectionString { get; set; } = string.Empty;
		public string ContainerNameTranslations { get; set; } = @"ngx-translate";
		public string ContainerNameDownloads { get; set; } = @"downloads";
	}

	public static class OrderCloudEnvironments
	{
		public static readonly OcEnv Production = new OcEnv()
		{
			EnvironmentName = @"Production",
			ApiUrl = @"https://api.ordercloud.io"
		};

		public static readonly OcEnv Sandbox = new OcEnv()
		{
			EnvironmentName = @"Sandbox",
			ApiUrl = @"https://sandboxapi.ordercloud.io"
		};
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
			var msg = $@"Please enter one of the allowable values: {string.Join(", ", AllowableValues ?? new string[] {@"No allowable values found"})}.";
			return new ValidationResult(msg);
		}
	}

	public class OcEnv
	{
		public string EnvironmentName { get; set; } = string.Empty;

		public string ApiUrl { get; set; } = string.Empty;
	}

	public class Region
	{
		public string AzureRegion { get; set; } = string.Empty;

		public string Id { get; set; } = string.Empty;

		public string Name { get; set; } = string.Empty;
	}
}
