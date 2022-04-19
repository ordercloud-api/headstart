using Headstart.Common.Extensions;
using Headstart.Tests.Mocks;
using NSubstitute;
using NUnit.Framework;
using OrderCloud.SDK;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Headstart.Common.Models.Headstart;

namespace Headstart.Tests
{
	public class CheckoutIntegrationCommandTests
	{
		private IOrderCloudClient _oc;

		[SetUp]
		public void Setup()
		{
			_oc = Substitute.For<IOrderCloudClient>();
			_oc.Suppliers.ListAsync<HsSupplier>().ReturnsForAnyArgs(SupplierMocks.SupplierList(MockSupplier("010"), MockSupplier("012"), MockSupplier("027"), MockSupplier("100")));
		}

		public const int FREE_SHIPPING_DAYS = 3;

		#region CheckoutIntegrationTests
		public async Task default_shipping_for_no_rates()
		{
			var shipItem1 = new ShipEstimateItem
			{
				LineItemID = "Line1"
			};
			var line1 = new HsLineItem
			{
				ID = "Line1",
				LineSubtotal = 370,
				SupplierID = "010"
			};
			var worksheet = BuildOrderWorksheet(new HsLineItem[] { line1 });
			var estimates = BuildEstimates(new HsShipMethod[] { }, new[] { shipItem1 });
			var result = await estimates.CheckForEmptyRates(20, 5).ApplyShippingLogic(worksheet, _oc, FREE_SHIPPING_DAYS);
			var methods = result[0].ShipMethods;

			Assert.AreEqual(1, methods.Count());
			Assert.AreEqual(20, methods[0].Cost);
		}

		[Test]
		public async Task free_shipping_for_free_shiping_line_items()
		{
			var shipItem1 = new ShipEstimateItem
			{
				LineItemID = "Line1"
			};
			var line1 = new HsLineItem
			{
				ID = "Line1",
				LineSubtotal = 370,
				SupplierID = "010"
			};
			var method1 = new HsShipMethod
			{
				ID = "NO_SHIPPING_RATES",
				xp = new ShipMethodXp()
			};
			var worksheet = BuildOrderWorksheet(new HsLineItem[] { line1 });
			var estimates = BuildEstimates(new[] { method1 }, new[] { shipItem1 });
			var result = await estimates.CheckForEmptyRates(20, 5).ApplyShippingLogic(worksheet, _oc, FREE_SHIPPING_DAYS);
			var methods = result[0].ShipMethods;

			Assert.AreEqual(1, methods.Count());
			Assert.AreEqual(0, methods[0].Cost);
			Assert.AreEqual(method1.Name, methods[0].Name);
		}

		[Test]
		public async Task shipping_ignore_nongroundAsync()
		{
			// don't transform methods if they aren't ground
			var shipItem1 = new ShipEstimateItem
			{
				LineItemID = "Line1"
			};
			var line1 = new HsLineItem
			{
				ID = "Line1",
				LineSubtotal = 20,
				SupplierID = "027"
			};
			var method1 = new HsShipMethod
			{
				Name = "STANDARD_OVERNIGHT",
				EstimatedTransitDays = 1,
				Cost = 150,
				xp = new ShipMethodXp()
			};
			var worksheet = BuildOrderWorksheet(new HsLineItem[] { line1 });
			var estimates = BuildEstimates(new[] { method1 }, new[] { shipItem1 });
			var result = await estimates.ApplyShippingLogic(worksheet, _oc, FREE_SHIPPING_DAYS);
			var methods = result[0].ShipMethods;

			Assert.AreEqual(1, methods.Count());
			Assert.AreEqual(method1.Cost, methods[0].Cost);
			Assert.AreEqual(method1.Name, methods[0].Name);
		}

		[Test]
		public async Task shipping_ignore_zerocostAsync()
		{
			// don't transform methods if they are zero cost
			var shipItem1 = new ShipEstimateItem
			{
				LineItemID = "Line1"
			};
			var line1 = new HsLineItem
			{
				ID = "Line1",
				LineSubtotal = 0,
				SupplierID = "027"
			};
			var method1 = new HsShipMethod
			{
				Name = "FEDEX_GROUND",
				EstimatedTransitDays = 3,
				Cost = 60,
				xp = new ShipMethodXp()
			};
			var worksheet = BuildOrderWorksheet(new HsLineItem[] { line1 });
			var estimates = BuildEstimates(new[] { method1 }, new[] { shipItem1 });
			var result = await estimates.ApplyShippingLogic(worksheet, _oc, FREE_SHIPPING_DAYS);
			var methods = result[0].ShipMethods;

			Assert.AreEqual(1, methods.Count());
			Assert.AreEqual(method1.Cost, methods[0].Cost);
			Assert.AreEqual(method1.Name, methods[0].Name);
		}

		#endregion

		#region SetupMethods
		private List<HsShipEstimate> BuildEstimates(HsShipMethod[] shipMethods, ShipEstimateItem[] shipItems = null, string id = "mockID")
		{
			return new List<HsShipEstimate>
			{
				new HsShipEstimate
				{
					ID = id,
					ShipMethods = shipMethods.ToList(),
					ShipEstimateItems = shipItems?.ToList()
				}
			};
		}

		private HsOrderWorksheet BuildOrderWorksheet(HsLineItem[] lineItems, string buyerUserCountry = "US")
		{
			return new HsOrderWorksheet
			{
				Order = new HsOrder()
				{
					FromUser = new HsUser()
					{
						xp = new UserXp
						{
							Country = buyerUserCountry
						}
					}
				},
				LineItems = lineItems.ToList()
			};
		}
		private HsSupplier MockSupplier(string id = "mockID", int freeShippingThreshold = 500)
		{
			return new HsSupplier
			{
				ID = id,
				xp = new SupplierXp
				{
					FreeShippingThreshold = freeShippingThreshold,
				}
			};
		}
		#endregion
	}
}
