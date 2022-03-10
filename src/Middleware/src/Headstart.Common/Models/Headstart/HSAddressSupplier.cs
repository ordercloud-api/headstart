using OrderCloud.SDK;
using System.Collections.Generic;

namespace Headstart.Common.Models.Headstart
{
	public class HsAddressSupplier : Address<SupplierAddressXP>
	{
		public string Id { get; set; } = string.Empty;
	}

	public class SupplierAddressXP
	{
		public Coordinates Coordinates { get; set; } = new Coordinates();

		public List<OriginAddressAccessorial> Accessorials { get; set; } = new List<OriginAddressAccessorial>();
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