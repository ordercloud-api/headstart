using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ordercloud.integrations.library;
using SendGrid.Helpers.Mail;
using Newtonsoft.Json;
using System.Text.Json.Serialization;
using Headstart.Models.Headstart;

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
		/// The ID of the seller organization to be seeded
		/// it is not currently possible to create an organization outside of the portal
		/// </summary>
		[Required]
		public string SellerOrgID { get; set; }

		/// <summary>
		/// An optional array of suppliers to create as part of the initial seeding
		/// </summary>
		public List<HSSupplier> Suppliers { get; set; } = new List<HSSupplier> { };

		/// <summary>
		/// An optional array of buyers to create as part of the initial seeding
		/// </summary>
		public List<HSBuyer> Buyers { get; set; } = new List<HSBuyer> { };
	}

	[DocIgnore]
	public class EnvironmentSeedResponse
    {
		public string Comments { get; set; }
		public Dictionary<string, dynamic> ApiClients { get; set; }
    }
}