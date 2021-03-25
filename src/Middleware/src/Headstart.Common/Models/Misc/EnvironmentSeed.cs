using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ordercloud.integrations.library;
using SendGrid.Helpers.Mail;
using Newtonsoft.Json;
using System.Text.Json.Serialization;
using Headstart.Models.Headstart;
using Headstart.Common;
using System.Linq;

namespace Headstart.Models.Misc
{
    [DocIgnore]
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
		[StringLength(100, ErrorMessage = "Password must be at least 8 characters long and maximum 100 characters long", MinimumLength = 8)]
		[RegularExpression("^(?=.*[a-zA-Z])(?=.*[0-9]).+$", ErrorMessage = "Password must contain at least one letter and one number")]
		public string InitialAdminPassword { get; set; }

		/// <summary>
		/// Optionally pass in a value if you have an existing organization you would like to seed. If no value is present a new org will be created
		/// Creating an org via seeding is only possible in the sandbox api environment
		/// </summary>
		public string SellerOrgID { get; set; }

		public string SellerOrgName { get; set; }

		/// <summary>
		/// An optional array of suppliers to create as part of the initial seeding
		/// </summary>
		public List<HSSupplier> Suppliers { get; set; } = new List<HSSupplier> { };

		/// <summary>
		/// An optional array of buyers to create as part of the initial seeding
		/// </summary>
		public List<HSBuyer> Buyers { get; set; } = new List<HSBuyer> { };

		/// <summary>
		/// An optional string to specify a buyer which the anonymous buyer user will be assigned to
		/// Otherwise we will assign the default buyer that is created in seeding.
		/// </summary>
		public string AnonymousShoppingBuyerID { get; set; }

		public string MiddlewareBaseUrl { get; set; }

		/// <summary>
		/// OrderCloud values that tell us what OC environment to use.
		/// Environment and WebhookHashKey are the only required fields for seeding.
		/// Your environment will be either sandbox or production. Your WebhookHashKey can be any string of your choosing.
		/// </summary>
		public OrderCloudSeedRequest OrderCloudSettings { get; set; }

		/// <summary>
		/// An optional object of storage settings for your translations container. 
		/// If none are provided the seeding funciton will not create a translation file.
		/// Provide a valid ConnectionString and ContainerNameTranslations to have the seeding function generate your translation file
		/// </summary>
		public BlobSettings BlobSettings { get; set; }
    }

	[DocIgnore]
	public class EnvironmentSeedResponse
    {
		public string Comments { get; set; }
		public string OrganizationName { get; set; }
		public string OrganizationID { get; set; }
		public string OrderCloudEnvironment { get; set; }
		public Dictionary<string, dynamic> ApiClients { get; set; }
    }

	public class OrderCloudSeedRequest : OrderCloudSettings
	{
		[Required]
		[ValueRange(AllowableValues = new[] { "production", "prod", "sandbox" })]
		public string Environment { get; set; }
	}

	public class OrderCloudEnvironments
    {
		public static OcEnv Production = new OcEnv()
		{
			environmentName = "Production",
			apiUrl = "https://api.ordercloud.io"
		};
		public static OcEnv Sandbox = new OcEnv()
		{
			environmentName = "Sandbox",
			apiUrl = "https://sandboxapi.ordercloud.io"
		};
    }

	public class ValueRange : ValidationAttribute
    {
		public string[] AllowableValues { get; set; }

		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			if (AllowableValues?.Contains(value?.ToString().ToLower()) == true)
			{
				return ValidationResult.Success;
			}
			var msg = $"Please enter one of the allowable values: {string.Join(", ", (AllowableValues ?? new string[] { "No allowable values found" }))}.";
			return new ValidationResult(msg);
		}
	}

	public class OcEnv
    {
        public string environmentName { get; set; }
		public string apiUrl { get; set; }
    }
}