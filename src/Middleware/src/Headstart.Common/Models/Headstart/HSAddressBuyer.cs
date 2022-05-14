using OrderCloud.SDK;
using System;
using System.Collections.Generic;

namespace Headstart.Models
{
	public enum DestinationAddressAccessorial
	{
		DestinationInsideDelivery = 3,
		DestinationLiftGate = 4,
		LimitedAccessDelivery = 9,
		ResidentialDelivery = 15,
	}

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

		public Coordinates Coordinates { get; set; }

		public DateTimeOffset? OpeningDate { get; set; }

		public string BillingNumber { get; set; }

		public string Status { get; set; }

		public string LegalEntity { get; set; }

		public string PrimaryContactName { get; set; }
	}

	public class Coordinates
	{
		public double Latitude { get; set; }

		public double Longitude { get; set; }
	}
}
