using OrderCloud.SDK;
using System.Collections.Generic;

namespace Headstart.Common.Models.Headstart
{
	public class HsSellerUser : User<SellerUserXp>
	{
		public string Id { get; set; } = string.Empty;
	}

	public class SellerUserXp
	{
		public bool OrderEmails { get; set; }
		public List<string> AddtlRcpts { get; set; } = new List<string>();
	}
}