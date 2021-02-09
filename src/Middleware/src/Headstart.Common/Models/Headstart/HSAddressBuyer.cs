using OrderCloud.SDK;
using System;
using System.Collections.Generic;
using ordercloud.integrations.library;

namespace Headstart.Models
{
    [SwaggerModel]
    public class HSAddressBuyer : Address<BuyerAddressXP>, IHSObject
    {
    }
    [SwaggerModel]
    public class HSAddressMeBuyer : BuyerAddress<BuyerAddressXP>, IHSObject
    {
    }

	[SwaggerModel]
	public class BuyerAddressXP
	{
		public List<DestinationAddressAccessorial> Accessorials { get; set; }
		public string Email { get; set; }
        public string LocationID { get; set; }
        public Coordinates Coordinates;
		public int? AvalaraCertificateID { get; set; } // default value is null if no certificate
		public DateTimeOffset? AvalaraCertificateExpiration { get; set; } // default value is null if no certificate 
        public DateTimeOffset? OpeningDate { get; set; }
        public string BillingNumber { get; set; }
        public string Status { get; set; }
        public string LegalEntity { get; set; }
        public string PrimaryContactName { get; set; }
    }

    public enum DestinationAddressAccessorial
    {
        DestinationInsideDelivery = 3,
        DestinationLiftGate = 4,
        LimitedAccessDelivery = 9,
        ResidentialDelivery = 15,
    }

    [SwaggerModel]
	public class Coordinates
	{
		public double Latitude { get; set; }
		public double Longitude { get; set; }
	}
}
