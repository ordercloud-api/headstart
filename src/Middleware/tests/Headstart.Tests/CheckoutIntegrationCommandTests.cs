using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Headstart.Common.Extensions;
using Headstart.Common.Services.ShippingIntegration.Models;
using Headstart.Models;
using Headstart.Models.Headstart;
using Headstart.Tests.Mocks;
using NSubstitute;
using NUnit.Framework;
using OrderCloud.SDK;

namespace Headstart.Tests
{
    public class CheckoutIntegrationCommandTests
    {
        private const int FreeShippingDays = 3;

        private IOrderCloudClient oc;

        [SetUp]
        public void Setup()
        {
            oc = Substitute.For<IOrderCloudClient>();
            oc.Suppliers.ListAsync<HSSupplier>().ReturnsForAnyArgs(SupplierMocks.SupplierList(MockSupplier("010"), MockSupplier("012"), MockSupplier("027"), MockSupplier("100")));
        }

        [Test]
        public async Task ApplyShippingLogic_WithNoRates_ReturnsDefaultShipping()
        {
            // Arrange
            var shipItem1 = new ShipEstimateItem
            {
                LineItemID = "Line1",
            };
            var line1 = new HSLineItem
            {
                ID = "Line1",
                LineSubtotal = 370,
                SupplierID = "010",
            };
            var worksheet = BuildOrderWorksheet(new HSLineItem[] { line1 });
            var estimates = BuildEstimates(new HSShipMethod[] { }, new[] { shipItem1 });
            estimates = estimates.CheckForEmptyRates(20, 5);

            // Act
            var result = await estimates.ApplyShippingLogic(worksheet, oc, FreeShippingDays);

            // Assert
            var methods = result[0].ShipMethods;

            Assert.AreEqual(1, methods.Count());
            Assert.AreEqual(20, methods[0].Cost);
        }

        [Test]
        public async Task ApplyShippingLogic_WithFreeShippingLineItems_ReturnsFreeShipping()
        {
            // Arrange
            var shipItem1 = new ShipEstimateItem
            {
                LineItemID = "Line1",
            };
            var line1 = new HSLineItem
            {
                ID = "Line1",
                LineSubtotal = 370,
                SupplierID = "010",
            };
            var method1 = new HSShipMethod
            {
                ID = "NO_SHIPPING_RATES",
                xp = new ShipMethodXP(),
            };
            var worksheet = BuildOrderWorksheet(new HSLineItem[] { line1 });
            var estimates = BuildEstimates(new[] { method1 }, new[] { shipItem1 });
            estimates = estimates.CheckForEmptyRates(20, 5);

            // Act
            var result = await estimates.ApplyShippingLogic(worksheet, oc, FreeShippingDays);

            // Assert
            var methods = result[0].ShipMethods;

            Assert.AreEqual(1, methods.Count());
            Assert.AreEqual(0, methods[0].Cost);
            Assert.AreEqual(method1.Name, methods[0].Name);
        }

        [Test]
        public async Task ApplyShippingLogic_WithStandardLineItems_ReturnsStandardOvernightShipping()
        {
            // Arrange
            // don't transform methods if they aren't ground
            var shipItem1 = new ShipEstimateItem
            {
                LineItemID = "Line1",
            };
            var line1 = new HSLineItem
            {
                ID = "Line1",
                LineSubtotal = 20,
                SupplierID = "027",
            };
            var method1 = new HSShipMethod
            {
                Name = "STANDARD_OVERNIGHT",
                EstimatedTransitDays = 1,
                Cost = 150,
                xp = new ShipMethodXP { },
            };
            var worksheet = BuildOrderWorksheet(new HSLineItem[] { line1 });
            var estimates = BuildEstimates(new[] { method1 }, new[] { shipItem1 });

            // Act
            var result = await estimates.ApplyShippingLogic(worksheet, oc, FreeShippingDays);

            // Assert
            var methods = result[0].ShipMethods;

            Assert.AreEqual(1, methods.Count());
            Assert.AreEqual(method1.Cost, methods[0].Cost);
            Assert.AreEqual(method1.Name, methods[0].Name);
        }

        private IList<HSShipEstimate> BuildEstimates(HSShipMethod[] shipMethods, ShipEstimateItem[] shipItems = null, string id = "mockID")
        {
            return new List<HSShipEstimate>
            {
                new HSShipEstimate
                {
                    ID = id,
                    ShipMethods = shipMethods.ToList(),
                    ShipEstimateItems = shipItems?.ToList(),
                },
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
                            Country = buyerUserCountry,
                        },
                    },
                },
                LineItems = lineItems.ToList(),
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
                },
            };
        }
    }
}
