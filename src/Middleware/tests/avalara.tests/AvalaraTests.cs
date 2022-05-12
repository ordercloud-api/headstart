using NUnit.Framework;
using ordercloud.integrations.avalara;
using OrderCloud.SDK;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace avalara.tests
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
        public async Task avalara_mock_estimate_no_license_key()
        {
            var response = await command.CalculateEstimateAsync(new OrderWorksheet(), new List<OrderPromotion>());

            Assert.AreEqual(123.45, response.TotalTax);
            Assert.AreEqual("Mock Avalara Response for Headstart", response.ExternalTransactionID);
        }

        [Test]
        public async Task avalara_mock_transaction_no_license_key()
        {
            var response = await command.CommitTransactionAsync(new OrderWorksheet(), new List<OrderPromotion>());

            Assert.AreEqual(123.45, response.TotalTax);
            Assert.AreEqual("Mock Avalara Response for Headstart", response.ExternalTransactionID);
        }
    }
}
