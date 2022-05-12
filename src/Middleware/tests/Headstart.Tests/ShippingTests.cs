using System.Collections;
using NUnit.Framework;
using ordercloud.integrations.easypost;
using OrderCloud.SDK;

namespace Headstart.Tests
{
    public class ShippingTests
    {
        [SetUp]
        public void Setup()
        {
        }

        public class LineItemFactory
        {
            public static IEnumerable LineItemCases
            {
                get
                {
                    yield return new TestCaseData(new LineItem()
                    {
                        Product = new LineItemProduct() { ShipLength = 5, ShipWidth = 5, ShipHeight = 5, ShipWeight = 5 },
                        Variant = null
                    }).Returns(5);
                    yield return new TestCaseData(new LineItem()
                    {
                        Product = new LineItemProduct() { ShipLength = 5, ShipWidth = 5, ShipHeight = 5, ShipWeight = 5 },
                        Variant = new LineItemVariant() { ShipLength = 10, ShipHeight = 10, ShipWeight = 10, ShipWidth = 10 }
                    }).Returns(10);
                    yield return new TestCaseData(new LineItem()
                    {
                        Product = new LineItemProduct() { ShipLength = null, ShipWidth = null, ShipHeight = null, ShipWeight = null },
                        Variant = new LineItemVariant() { ShipLength = null, ShipHeight = null, ShipWeight = null, ShipWidth = null }
                    }).Returns(100);
                }
            }
        }

        [Test, TestCaseSource(typeof(LineItemFactory), nameof(LineItemFactory.LineItemCases))]
        public double TestShipDimensions(LineItem item)
        {
            return item.ShipWeightOrDefault(100);
        }
    }
}
