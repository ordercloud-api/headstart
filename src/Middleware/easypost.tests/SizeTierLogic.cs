using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using OrderCloud.SDK;
using ordercloud.integrations.easypost;

namespace easypost.tests
{
    public class SizeTierLogic
    {
        //[Test]
        //public void handles_all_lines_shipping_together_new()
        //{
        //    // line items with SizeTier not equal to "G" will all ship together

        //    // arrange
        //    var lines = new List<LineItem>
        //    {
        //        BuildLine(quantity: 1, shipWeight: 5, SizeTier.A),
        //        BuildLine(quantity: 1, shipWeight: 5, SizeTier.B),
        //        BuildLine(quantity: 1, shipWeight: 5, SizeTier.C),
        //        BuildLine(quantity: 1, shipWeight: 5, SizeTier.D),
        //        BuildLine(quantity: 1, shipWeight: 5, SizeTier.E),
        //        BuildLine(quantity: 1, shipWeight: 5, SizeTier.F),
        //        BuildLine(quantity: 1, shipWeight: 5, SizeTier.A),
        //        BuildLine(quantity: 1, shipWeight: 5, SizeTier.B),
        //        BuildLine(quantity: 1, shipWeight: 5, SizeTier.C),
        //        BuildLine(quantity: 1, shipWeight: 5, SizeTier.D),
        //    };

        //    // act
        //    var packages = SmartPackageMapper.MapLineItemsIntoPackages(lines);
        //    var package = packages[0];

        //    Assert.AreEqual(1, packages.Count);
        //    Assert.AreEqual(22, package.length);
        //    Assert.AreEqual(22, package.width);
        //    Assert.AreEqual(22, package.height);
        //    Assert.AreEqual(50, package.weight);
        //}

        [Test]
        public void handles_all_lines_shipping_together_legacy()
        {
            // line items with SizeTier not equal to "G" will all ship together

            // Arrange
            var lines = new List<LineItem>
            {
                BuildLine(quantity: 1, shipWeight: 5, SizeTier.A),
                BuildLine(quantity: 1, shipWeight: 5, SizeTier.A),
                BuildLine(quantity: 1, shipWeight: 5, SizeTier.A),
                BuildLine(quantity: 1, shipWeight: 5, SizeTier.A),
                BuildLine(quantity: 1, shipWeight: 5, SizeTier.A),
                BuildLine(quantity: 1, shipWeight: 5, SizeTier.A)
            };

            // Act
            var packages = SmartPackageMapper.MapLineItemsIntoPackages(lines);

            // Assert
            Assert.AreEqual(3, packages.Count);

            var package1 = packages[0];
            Assert.AreEqual(11, package1.length);
            Assert.AreEqual(11, package1.width);
            Assert.AreEqual(11, package1.height);
            Assert.AreEqual(10, package1.weight);

            var package2 = packages[1];
            Assert.AreEqual(11, package2.length);
            Assert.AreEqual(11, package2.width);
            Assert.AreEqual(11, package2.height);
            Assert.AreEqual(10, package2.weight);

            var package3 = packages[2];
            Assert.AreEqual(11, package3.length);
            Assert.AreEqual(11, package3.width);
            Assert.AreEqual(11, package3.height);
            Assert.AreEqual(10, package3.weight);
        }

        [Test]
        public void handles_ship_alone()
        {
            // line items with SizeTier "G" will all ship alone

            // Arrange
            var lines = new List<LineItem>
            {
                BuildLine(quantity: 3, shipWeight: 3, SizeTier.G),
                BuildLine(quantity: 1, shipWeight: 3, SizeTier.G)
            };

            // Act
            var packages = SmartPackageMapper.MapLineItemsIntoPackages(lines);

            // Assert
            Assert.AreEqual(4, packages.Count);
        }

        public LineItem BuildLine(int quantity, decimal shipWeight, SizeTier sizeTier)
        {
            return new LineItem
            {
                Quantity = quantity,
                Product = new LineItemProduct
                {
                    ShipWeight = shipWeight,
                    xp = new ProductXP {
                        SizeTier = sizeTier
                    }
                }
            };
        }
    }

    public class ProductXP
    {
        public SizeTier SizeTier { get; set; }
    }
}
