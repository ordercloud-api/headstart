using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using OrderCloud.SDK;

namespace OrderCloud.Integrations.Avalara.Tests
{
    public class AvalaraTests
    {
        private AvalaraCommand command;

        [SetUp]
        public void Setup()
        {
            var avalaraConfig = new AvalaraConfig()
            {
                LicenseKey = null,
                BaseApiUrl = "http://www.supersweeturi.com",
            };
            command = new AvalaraCommand(avalaraConfig,  AppEnvironment.Test.ToString());
        }

        [Test]
        public async Task CalculateEstimateAsync_WithoutCredentials_ReturnsMockResponse()
        {
            // Arrange

            // Act
            var response = await command.CalculateEstimateAsync(new OrderWorksheet(), new List<OrderPromotion>());

            // Assert
            Assert.AreEqual(123.45, response.TotalTax);
            Assert.AreEqual("Mock Avalara Response for Headstart", response.ExternalTransactionID);
        }

        [Test]
        public async Task CommitTransactionAsync_WithoutCredentials_ReturnsMockResponse()
        {
            // Arrange

            // Act
            var response = await command.CommitTransactionAsync(new OrderWorksheet(), new List<OrderPromotion>());

            // Assert
            Assert.AreEqual(123.45, response.TotalTax);
            Assert.AreEqual("Mock Avalara Response for Headstart", response.ExternalTransactionID);
        }
    }
}
