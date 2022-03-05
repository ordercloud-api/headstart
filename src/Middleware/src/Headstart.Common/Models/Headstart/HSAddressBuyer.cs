using System;
using OrderCloud.SDK;
using System.Collections.Generic;

namespace Headstart.Models
{
    public class HSAddressBuyer : Address<BuyerAddressXP>, IHSObject { }
    public class HSAddressMeBuyer : BuyerAddress<BuyerAddressXP>, IHSObject { }

    public class BuyerAddressXP
    {
        public List<DestinationAddressAccessorial> Accessorials { get; set; }
        public string Email { get; set; } = string.Empty;
        public string LocationID { get; set; } = string.Empty;
        public Coordinates Coordinates;
        public DateTimeOffset? OpeningDate { get; set; }
        public string BillingNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string LegalEntity { get; set; } = string.Empty;
        public string PrimaryContactName { get; set; } = string.Empty;
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