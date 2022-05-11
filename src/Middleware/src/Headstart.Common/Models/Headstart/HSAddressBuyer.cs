using OrderCloud.SDK;
using System;
using System.Collections.Generic;
using ordercloud.integrations.library;

namespace Headstart.Models
{
    public class HSAddressBuyer : Address<BuyerAddressXP>, IHSObject
    {
    }
    public class HSAddressMeBuyer : BuyerAddress<BuyerAddressXP>, IHSObject
    {
    }

	public class BuyerAddressXP
	{
		public List<DestinationAddressAccessorial> Accessorials { get; set; }
		public string Email { get; set; }
        public string LocationID { get; set; }
        public Coordinates Coordinates;
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

	public class Coordinates
	{
		public double Latitude { get; set; }
		public double Longitude { get; set; }
	}
}
