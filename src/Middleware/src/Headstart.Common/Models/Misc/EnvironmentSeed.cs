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
}