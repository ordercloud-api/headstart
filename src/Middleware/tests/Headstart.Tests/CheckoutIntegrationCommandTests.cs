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

namespace Headstart.Tests
{
    public class CheckoutIntegrationCommandTests
    {
        #region FilterSlowerRatesWithHighCost
        [Test]
        public void dont_filter_valid_rates()
        {
            // preserve order, rates are already correct
            var method1 = new HSShipMethod
            {
                EstimatedTransitDays = 1,
                Cost = 25
            };
            var method2 = new HSShipMethod
            {
                EstimatedTransitDays = 2,
                Cost = 15
            };
            var method3 = new HSShipMethod
            {
                EstimatedTransitDays = 3,
                Cost = 5
            };
            var estimates = BuildEstimates(new[] { method1, method2, method3 });
            var result = CheckoutIntegrationCommand.FilterSlowerRatesWithHighCost(estimates);
            var methods = result[0].ShipMethods;

            Assert.AreEqual(3, methods.Count);
            Assert.AreEqual(25, methods[0].Cost);
            Assert.AreEqual(15, methods[1].Cost);
            Assert.AreEqual(5, methods[2].Cost);
        }

        [Test]
        public void remove_invalid_method()
        {
            // remove one offending ship method
            var method1 = new HSShipMethod
            {
                EstimatedTransitDays = 1,
                Cost = 10
            };
            var method2 = new HSShipMethod
            {
                EstimatedTransitDays = 2,
                Cost = 5
            };
            var method3 = new HSShipMethod
            {
                EstimatedTransitDays = 3,
                Cost = 20
            };
            var estimates = BuildEstimates(new[] { method1, method2, method3 });
            var result = CheckoutIntegrationCommand.FilterSlowerRatesWithHighCost(estimates);
            var methods = result[0].ShipMethods;

            Assert.AreEqual(2, methods.Count);
            Assert.AreEqual(10, methods[0].Cost);
            Assert.AreEqual(5, methods[1].Cost);
        }

        [Test]
        public void remove_two_invalid_methods()
        {
            // remove two offending ship methods
            var method1 = new HSShipMethod
            {
                EstimatedTransitDays = 1,
                Cost = 5
            };
            var method2 = new HSShipMethod
            {
                EstimatedTransitDays = 2,
                Cost = 10
            };
            var method3 = new HSShipMethod
            {
                EstimatedTransitDays = 3,
                Cost = 15
            };
            var estimates = BuildEstimates(new[] { method1, method2, method3 });
            var result = CheckoutIntegrationCommand.FilterSlowerRatesWithHighCost(estimates);
            var methods = result[0].ShipMethods;

            Assert.AreEqual(1, methods.Count);
            Assert.AreEqual(5, methods[0].Cost);
        }

        [Test]
        public void handle_mixed_order_by_transit_days()
        {
            // remove two offending ship methods, transit days ordered backwards
            var method1 = new HSShipMethod
            {
                EstimatedTransitDays = 3,
                Cost = 15
            };
            var method2 = new HSShipMethod
            {
                EstimatedTransitDays = 2,
                Cost = 10
            };
            var method3 = new HSShipMethod
            {
                EstimatedTransitDays = 1,
                Cost = 5
            };
            var estimates = BuildEstimates(new[] { method1, method2, method3 });
            var result = CheckoutIntegrationCommand.FilterSlowerRatesWithHighCost(estimates);
            var methods = result[0].ShipMethods;

            Assert.AreEqual(1, methods.Count);
            Assert.AreEqual(5, methods[0].Cost);
        }

        [Test]
        public void handle_free_shipping()
        {
            // handle free shipping
            var method1 = new HSShipMethod
            {
                EstimatedTransitDays = 1,
                Cost = 15
            };
            var method2 = new HSShipMethod
            {
                EstimatedTransitDays = 2,
                Cost = 0
            };
            var method3 = new HSShipMethod
            {
                EstimatedTransitDays = 3,
                Cost = 5
            };
            var estimates = BuildEstimates(new[] { method1, method2, method3 });
            var result = CheckoutIntegrationCommand.FilterSlowerRatesWithHighCost(estimates);
            var methods = result[0].ShipMethods;

            Assert.AreEqual(2, methods.Count);
            Assert.AreEqual(15, methods[0].Cost);
            Assert.AreEqual(0, methods[1].Cost);
        }

        [Test]
        public void handle_methods_with_same_rates()
        {
            // handle two estimates with same rates
            // we do not want to filter out a slower estimate with the same rate
            var method1 = new HSShipMethod
            {
                EstimatedTransitDays = 1,
                Cost = 15
            };
            var method2 = new HSShipMethod
            {
                EstimatedTransitDays = 2,
                Cost = 15
            };
            var method3 = new HSShipMethod
            {
                EstimatedTransitDays = 3,
                Cost = 5
            };
            var estimates = BuildEstimates(new[] { method1, method2, method3 });
            var result = CheckoutIntegrationCommand.FilterSlowerRatesWithHighCost(estimates);
            var methods = result[0].ShipMethods;

            Assert.AreEqual(3, methods.Count);
            Assert.AreEqual(15, methods[0].Cost);
            Assert.AreEqual(1, methods[0].EstimatedTransitDays);
            Assert.AreEqual(5, methods[2].Cost);
            Assert.AreEqual(3, methods[2].EstimatedTransitDays);
        }


        #endregion
        #region ApplyFlatRateShipping
        public const decimal FLAT_RATE_FIRST_TIER = 29.99M;
        public const decimal FLAT_RATE_SECOND_TIER = 0;

        [Test]
        public void flatrateshipping_ignore_nonground()
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
                Name = "PRIORITY_OVERNIGHT",
                EstimatedTransitDays = 1,
                Cost = 150,
                xp = new ShipMethodXP { }
            };
            var worksheet = BuildOrderWorksheet(line1);
            var estimates = BuildEstimates(new[] { method1 }, new[] { shipItem1 });
            var result = ApplyFlatRateShipping(worksheet, estimates, "027", null);
            var methods = result[0].ShipMethods;

            Assert.AreEqual(1, methods.Count());
            Assert.AreEqual(method1.Cost, methods[0].Cost);
            Assert.AreEqual(method1.Name, methods[0].Name);
        }

        [Test]
        public void flatrateshipping_ignore_zerocost()
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
            var worksheet = BuildOrderWorksheet(line1);
            var estimates = BuildEstimates(new[] { method1 }, new[] { shipItem1 });
            var result = ApplyFlatRateShipping(worksheet, estimates, "027", null);
            var methods = result[0].ShipMethods;

            Assert.AreEqual(1, methods.Count());
            Assert.AreEqual(method1.Cost, methods[0].Cost);
            Assert.AreEqual(method1.Name, methods[0].Name);
        }

        [Test]
        public void flatrateshipping_handle_first_tier()
        {
            // set shipping cost to $29.99 if line item cost is between 0.01$ and $499.99
            var shipItem1 = new ShipEstimateItem
            {
                LineItemID = "Line1"
            };
            var line1 = new HSLineItem
            {
                ID = "Line1",
                LineSubtotal = 370,
                SupplierID = "027"
            };
            var method1 = new HSShipMethod
            {
                Name = "FEDEX_GROUND",
                EstimatedTransitDays = 3,
                Cost = 60,
                xp = new ShipMethodXP { }
            };
            var worksheet = BuildOrderWorksheet(line1);
            var estimates = BuildEstimates(new[] { method1 }, new[] { shipItem1 });
            var result = ApplyFlatRateShipping(worksheet, estimates, "027", null);
            var methods = result[0].ShipMethods;

            Assert.AreEqual(1, methods.Count());
            Assert.AreEqual(FLAT_RATE_FIRST_TIER, methods[0].Cost);
            Assert.AreEqual(method1.Name, methods[0].Name);
        }

        [Test]
        public void flatrateshipping_handle_first_tier_multiple_lines()
        {
            // set shipping cost to $29.99 if line item cost is between 0.01$ and $499.99
            var shipItem1 = new ShipEstimateItem
            {
                LineItemID = "Line1"
            };
            var shipItem2 = new ShipEstimateItem
            {
                LineItemID = "Line2"
            };
            var line1 = new HSLineItem
            {
                ID = "Line1",
                LineSubtotal = 200,
                SupplierID = "027"
            };
            var line2 = new HSLineItem
            {
                ID = "Line2",
                LineSubtotal = 250,
                SupplierID = "027"
            };
            var method1 = new HSShipMethod
            {
                Name = "FEDEX_GROUND",
                EstimatedTransitDays = 3,
                Cost = 60,
                xp = new ShipMethodXP { }
            };
            var worksheet = BuildOrderWorksheet(line1, line2);
            var estimates = BuildEstimates(new[] { method1 }, new[] { shipItem1, shipItem2 });
            var result = ApplyFlatRateShipping(worksheet, estimates, "027", null);
            var methods = result[0].ShipMethods;

            Assert.AreEqual(1, methods.Count());
            Assert.AreEqual(FLAT_RATE_FIRST_TIER, methods[0].Cost);
            Assert.AreEqual(method1.Name, methods[0].Name);
        }

        [Test]
        public void flatrateshipping_handle_second_tier()
        {
            // set shipping cost to $0 if line item cost is greater than $499.99
            var shipItem1 = new ShipEstimateItem
            {
                LineItemID = "Line1"
            };
            var line1 = new HSLineItem
            {
                ID = "Line1",
                LineSubtotal = 602,
                SupplierID = "027"
            };
            var method1 = new HSShipMethod
            {
                Name = "FEDEX_GROUND",
                EstimatedTransitDays = 3,
                Cost = 60,
                xp = new ShipMethodXP { }
            };
            var worksheet = BuildOrderWorksheet(line1);
            var estimates = BuildEstimates(new[] { method1 }, new[] { shipItem1 });
            var result = ApplyFlatRateShipping(worksheet, estimates, "027", null);
            var methods = result[0].ShipMethods;

            Assert.AreEqual(1, methods.Count());
            Assert.AreEqual(FLAT_RATE_SECOND_TIER, methods[0].Cost);
            Assert.AreEqual(method1.Name, methods[0].Name);
        }

        private IList<HSShipEstimate> ApplyFlatRateShipping(HSOrderWorksheet worksheet, List<HSShipEstimate> estimates, string medlineSupplierID, string laliciousSupplierID)
        {
            return CheckoutIntegrationCommand.ApplyFlatRateShipping(worksheet, estimates);
        }

        [Test]
        public void flatrateshipping_handle_second_tier_multiple_lines()
        {
            // set shipping cost to $0 if line item cost is greater than $499.99
            var shipItem1 = new ShipEstimateItem
            {
                LineItemID = "Line1"
            };
            var shipItem2 = new ShipEstimateItem
            {
                LineItemID = "Line2"
            };
            var line1 = new HSLineItem
            {
                ID = "Line1",
                LineSubtotal = 250,
                SupplierID = "027"
            };
            var line2 = new HSLineItem
            {
                ID = "Line2",
                LineSubtotal = 250,
                SupplierID = "027"
            };
            var method1 = new HSShipMethod
            {
                Name = "FEDEX_GROUND",
                EstimatedTransitDays = 3,
                Cost = 60,
                xp = new ShipMethodXP { }
            };
            var worksheet = BuildOrderWorksheet(line1, line2);
            var estimates = BuildEstimates(new[] { method1 }, new[] { shipItem1, shipItem2 });
            var result = ApplyFlatRateShipping(worksheet, estimates, "027", null);
            var methods = result[0].ShipMethods;

            Assert.AreEqual(1, methods.Count());
            Assert.AreEqual(FLAT_RATE_SECOND_TIER, methods[0].Cost);
            Assert.AreEqual(method1.Name, methods[0].Name);
        }

        [Test]
        public void flatrateshipping_multiple_methods_with_qualifyingflatrate()
        {
            // If qualifies for flat rate shipping then the only rate that should be returned is the modified ground option
            // this test ensures the non-ground option is stripped out
            var shipItem1 = new ShipEstimateItem
            {
                LineItemID = "Line1"
            };
            var shipItem2 = new ShipEstimateItem
            {
                LineItemID = "Line2"
            };
            var line1 = new HSLineItem
            {
                ID = "Line1",
                LineSubtotal = 250,
                SupplierID = "027"
            };
            var line2 = new HSLineItem
            {
                ID = "Line2",
                LineSubtotal = 130,
                SupplierID = "027"
            };
            var method1 = new HSShipMethod
            {
                Name = "FEDEX_GROUND",
                EstimatedTransitDays = 3,
                Cost = 60,
                xp = new ShipMethodXP { }
            };
            var method2 = new HSShipMethod
            {
                Name = "PRIORITY_OVERNIGHT",
                EstimatedTransitDays = 1,
                Cost = 120,
                xp = new ShipMethodXP { }
            };
            var worksheet = BuildOrderWorksheet(line1, line2);
            var estimates = BuildEstimates(new[] { method1, method2 }, new[] { shipItem1, shipItem2 });
            var result = ApplyFlatRateShipping(worksheet, estimates, "027", null);
            var methods = result[0].ShipMethods;

            Assert.AreEqual(1, methods.Count());
            Assert.AreEqual(FLAT_RATE_FIRST_TIER, methods[0].Cost);
            Assert.AreEqual(method1.Name, methods[0].Name);
        }

        [Test]
        public void flatrateshipping_multiple_methods_with_nonqualifyingflatrate()
        {
            // If DOESNT qualify for flat rate shipping then dont modify the rates
            var shipItem1 = new ShipEstimateItem
            {
                LineItemID = "Line1"
            };
            var shipItem2 = new ShipEstimateItem
            {
                LineItemID = "Line2"
            };
            var line1 = new HSLineItem
            {
                ID = "Line1",
                LineSubtotal = 250,
                SupplierID = "027"
            };
            var line2 = new HSLineItem
            {
                ID = "Line2",
                LineSubtotal = 130,
                SupplierID = "027"
            };
            var method1 = new HSShipMethod
            {
                Name = "SOMETHING_ELSE",
                EstimatedTransitDays = 3,
                Cost = 60,
                xp = new ShipMethodXP { }
            };
            var method2 = new HSShipMethod
            {
                Name = "PRIORITY_OVERNIGHT",
                EstimatedTransitDays = 1,
                Cost = 120,
                xp = new ShipMethodXP { }
            };
            var worksheet = BuildOrderWorksheet(line1, line2);
            var estimates = BuildEstimates(new[] { method1, method2 }, new[] { shipItem1, shipItem2 });
            var result = ApplyFlatRateShipping(worksheet, estimates, "027", null);
            var methods = result[0].ShipMethods;

            Assert.AreEqual(2, methods.Count());
            Assert.AreEqual(method1.Cost, methods[0].Cost);
            Assert.AreEqual(method1.Name, methods[0].Name);
            Assert.AreEqual(method2.Cost, methods[1].Cost);
            Assert.AreEqual(method2.Name, methods[1].Name);
        }

        [Test]
        public void flatrateshipping_handle_multiple_estimates()
        {
            // set shipping cost to $0 if line item cost is greater than $499.99
            var shipItem1 = new ShipEstimateItem
            {
                LineItemID = "Supplier1Line1"
            };
            var shipitem2 = new ShipEstimateItem
            {
                LineItemID = "Supplier2Line1"
            };
            var line1 = new HSLineItem
            {
                ID = "Supplier1Line1",
                LineSubtotal = 110,
                SupplierID = "010"
            };
            var line2 = new HSLineItem
            {
                ID = "Supplier1Line2",
                LineSubtotal = 125,
                SupplierID = "010"
            };
            var line3 = new HSLineItem
            {
                ID = "Supplier2Line1",
                LineSubtotal = 130,
                SupplierID = "027"
            };
            var line4 = new HSLineItem
            {
                ID = "Supplier2Line2",
                LineSubtotal = 180,
                SupplierID = "027"
            };
            var method1 = new HSShipMethod
            {
                Name = "FEDEX_GROUND",
                EstimatedTransitDays = 1,
                Cost = 80,
                xp = new ShipMethodXP { }
            };
            var method2 = new HSShipMethod
            {
                Name = "FEDEX_GROUND",
                EstimatedTransitDays = 3,
                Cost = 60,
                xp = new ShipMethodXP { }
            };
            var worksheet = BuildOrderWorksheet(line1, line2, line3, line4);
            var estimates = BuildEstimates(new[] { method1 }, new[] { shipItem1 });
            estimates.AddRange(BuildEstimates(new[] { method2 }, new[] { shipitem2 }));
            var result = ApplyFlatRateShipping(worksheet, estimates, "027", null);

            Assert.AreEqual(2, result.Count());
            // compare first shipment item from first estimate (no changes because its not medline supplier)
            Assert.AreEqual(1, result[0].ShipMethods.Count());
            Assert.AreEqual(method1.Name, result[0].ShipMethods[0].Name);
            Assert.AreEqual(method1.Cost, result[0].ShipMethods[0].Cost);

            // compare first shipment item from second estimate (cost falls in first tier between $0.01 and $499.9)
            Assert.AreEqual(1, result[0].ShipMethods.Count());
            Assert.AreEqual(method2.Name, result[1].ShipMethods[0].Name);
            Assert.AreEqual(29.99M, result[1].ShipMethods[0].Cost);
        }
        #endregion

        private List<HSShipEstimate> BuildEstimates( HSShipMethod[] shipMethods, ShipEstimateItem[] shipItems = null)
        {
            return new List<HSShipEstimate>
            {
                new HSShipEstimate
                {
                    ShipMethods = shipMethods.ToList(),
                    ShipEstimateItems = shipItems?.ToList()
                }
            };
        }

        private HSOrderWorksheet BuildOrderWorksheet(params HSLineItem[] lineItems)
        {
            return new HSOrderWorksheet
            {
                LineItems = lineItems.ToList()
            };
        }
    }
}
