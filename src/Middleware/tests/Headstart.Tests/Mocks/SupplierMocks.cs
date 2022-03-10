using OrderCloud.SDK;
using System.Collections.Generic;
using Headstart.Common.Models.Headstart;

namespace Headstart.Tests.Mocks
{
	public static class SupplierMocks
	{
		public static List<HsSupplier> Suppliers(params HsSupplier[] suppliers)
		{
			return new List<HsSupplier>(suppliers);
		}

		public static ListPage<HsSupplier> EmptySuppliersList()
		{
			var items = new List<HsSupplier>();
			return new ListPage<HsSupplier>
			{
				Items = items
			};
		}

		public static ListPage<HsSupplier> SupplierList(params HsSupplier[] suppliers)
		{
			return new ListPage<HsSupplier>
			{
				Items = new List<HsSupplier>(suppliers)
			};
		}
	}
}
