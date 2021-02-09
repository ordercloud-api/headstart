using OrderCloud.SDK;
using System.Collections.Generic;
using ordercloud.integrations.library;

namespace Headstart.Models
{
    [SwaggerModel]
    public class HSAddressSupplier : Address<SupplierAddressXP>, IHSObject
    {
    }

    [SwaggerModel]
    public class SupplierAddressXP
	{
		public Coordinates Coordinates;
		public List<OriginAddressAccessorial> Accessorials { get; set; }
    }

    public enum OriginAddressAccessorial
    {
        LimitedAccessPickup = 10,
        OriginExhibition = 11,
        OriginInsidePickup = 12,
        OriginLiftGate = 13,
        ResidentialPickup = 16
    }
}
