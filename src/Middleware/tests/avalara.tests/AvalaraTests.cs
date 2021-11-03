using Avalara.AvaTax.RestClient;
using NUnit.Framework;
using ordercloud.integrations.avalara;
using OrderCloud.SDK;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
namespace avalara.tests
{
    public class AvalaraTests
    {
        private IOrderCloudClient _oc;
        private AvalaraCommand _command;

        [SetUp]
        public void Setup()
        {
            var avalaraConfig = new AvalaraConfig()
            {
                LicenseKey = null,
                BaseApiUrl = "http://www.supersweeturi.com"
            };
            _command = new AvalaraCommand(avalaraConfig,  AppEnvironment.Test.ToString());
        }

        [Test]
        public async Task avalara_mock_estimate_no_license_key()
        {
            var response = await _command.CalculateEstimateAsync(new OrderWorksheet(), new List<OrderPromotion>());

            Assert.AreEqual(123.45, response.TotalTax);
            Assert.AreEqual("Mock Avalara Response for Headstart", response.ExternalTransactionID);
        }

        [Test]
        public async Task avalara_mock_transaction_no_license_key()
        {
            var response = await _command.CommitTransactionAsync(new OrderWorksheet(), new List<OrderPromotion>());

            Assert.AreEqual(123.45, response.TotalTax);
            Assert.AreEqual("Mock Avalara Response for Headstart", response.ExternalTransactionID);
        }
    }
}
