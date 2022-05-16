using System.Collections.Generic;
using OrderCloud.SDK;

namespace Headstart.Models
{
	public class HSSellerUser : User<SellerUserXp>, IHSObject
	{
	}

	public class SellerUserXp
	{
		public bool OrderEmails { get; set; } = false;

		public List<string> AddtlRcpts { get; set; } = new List<string>();
	}
}
