using System.Collections.Generic;
using OrderCloud.SDK;

namespace Headstart.Models
{
    public enum OriginAddressAccessorial
    {
        LimitedAccessPickup = 10,
        OriginExhibition = 11,
        OriginInsidePickup = 12,
        OriginLiftGate = 13,
        ResidentialPickup = 16,
    }

    public class HSAddressSupplier : Address<SupplierAddressXP>, IHSObject
    {
    }

    public class SupplierAddressXP
    {
        public Coordinates Coordinates { get; set; }

        public List<OriginAddressAccessorial> Accessorials { get; set; }
    }
}
