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
            _command = new AvalaraCommand(default, avalaraConfig, new AvaTaxClient("four51_headstart", "v1", "four51_headstart", new Uri(avalaraConfig.BaseApiUrl)), AppEnvironment.Test.ToString());
        }

        [Test]
        public async Task avalara_mock_estimate_no_license_key()
        {
            var response = await _command.GetEstimateAsync(new OrderCloud.SDK.OrderWorksheet());

            Assert.AreEqual(123.45, response.totalTax);
            Assert.AreEqual("Mock Avalara Response for Headstart", response.description);
        }

        [Test]
        public async Task avalara_mock_transaction_no_license_key()
        {
            var response = await _command.CreateTransactionAsync(new OrderCloud.SDK.OrderWorksheet());

            Assert.AreEqual(123.45, response.totalTax);
            Assert.AreEqual("Mock Avalara Response for Headstart", response.description);
        }

        [Test]
        public async Task avalara_mock_certificate_no_license_key()
        {
            var response = await _command.CreateCertificateAsync(new TaxCertificate(), new OrderCloud.SDK.Address());

            Assert.AreEqual("Mock Tax Certificate", response.FileName);

        }
    }
}
