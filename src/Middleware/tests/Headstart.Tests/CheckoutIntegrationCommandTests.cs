using OrderCloud.SDK;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Headstart.Common.Services.ShippingIntegration.Models;
using Headstart.Models.Headstart;
using System.Linq;
using Headstart.API.Commands;
using Headstart.Common.Extensions;
using Headstart.Tests.Mocks;
using Headstart.Models;

namespace Headstart.Tests
{
    public class CheckoutIntegrationCommandTests
    {
        private IOrderCloudClient _oc;

        [SetUp]
        public void Setup()
        {
            _oc = Substitute.For<IOrderCloudClient>();
            _oc.Suppliers.ListAsync<HSSupplier>().ReturnsForAnyArgs(SupplierMocks.SupplierList(MockSupplier("010"), MockSupplier("012"), MockSupplier("027"), MockSupplier("100")));
        }

        public const int FREE_SHIPPING_DAYS = 3;

        #region CheckoutIntegrationTests
        public async Task default_shipping_for_no_rates()
        {
            var shipItem1 = new ShipEstimateItem
            {
                LineItemID = "Line1"
            };
            var line1 = new HSLineItem
            {
                ID = "Line1",
                LineSubtotal = 370,
                SupplierID = "010"
            };
            var worksheet = BuildOrderWorksheet(new HSLineItem[] { line1 });
            var estimates = BuildEstimates(new HSShipMethod[] { }, new[] { shipItem1 });
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
            var line1 = new HSLineItem
            {
                ID = "Line1",
                LineSubtotal = 370,
                SupplierID = "010"
            };
            var method1 = new HSShipMethod
            {
                ID = "NO_SHIPPING_RATES",
                xp = new ShipMethodXP()
            };
            var worksheet = BuildOrderWorksheet(new HSLineItem[] { line1 });
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
            var line1 = new HSLineItem
            {
                ID = "Line1",
                LineSubtotal = 20,
                SupplierID = "027"
            };
            var method1 = new HSShipMethod
            {
                Name = "STANDARD_OVERNIGHT",
                EstimatedTransitDays = 1,
                Cost = 150,
                xp = new ShipMethodXP { }
            };
            var worksheet = BuildOrderWorksheet(new HSLineItem[] { line1 });
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
            var line1 = new HSLineItem
            {
                ID = "Line1",
                LineSubtotal = 0,
                SupplierID = "027"
            };
            var method1 = new HSShipMethod
            {
                Name = "FEDEX_GROUND",
                EstimatedTransitDays = 3,
                Cost = 60,
                xp = new ShipMethodXP { }
            };
            var worksheet = BuildOrderWorksheet(new HSLineItem[] { line1 });
            var estimates = BuildEstimates(new[] { method1 }, new[] { shipItem1 });
            var result = await estimates.ApplyShippingLogic(worksheet, _oc, FREE_SHIPPING_DAYS);
            var methods = result[0].ShipMethods;

            Assert.AreEqual(1, methods.Count());
            Assert.AreEqual(method1.Cost, methods[0].Cost);
            Assert.AreEqual(method1.Name, methods[0].Name);
        }

        #endregion

        #region SetupMethods
        private List<HSShipEstimate> BuildEstimates(HSShipMethod[] shipMethods, ShipEstimateItem[] shipItems = null, string id = "mockID")
        {
            return new List<HSShipEstimate>
            {
                new HSShipEstimate
                {
                    ID = id,
                    ShipMethods = shipMethods.ToList(),
                    ShipEstimateItems = shipItems?.ToList()
                }
            };
        }

        private HSOrderWorksheet BuildOrderWorksheet(HSLineItem[] lineItems, string buyerUserCountry = "US")
        {
            return new HSOrderWorksheet
            {
                Order = new HSOrder()
                {
                    FromUser = new HSUser()
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
        private HSSupplier MockSupplier(string id = "mockID", int freeShippingThreshold = 500)
        {
            return new HSSupplier
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
