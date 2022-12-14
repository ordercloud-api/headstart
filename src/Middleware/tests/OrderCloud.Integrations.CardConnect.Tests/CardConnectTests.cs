using System.Collections;
using System.Threading.Tasks;
using Flurl.Http.Configuration;
using Flurl.Http.Testing;
using NUnit.Framework;
using OrderCloud.Integrations.CardConnect.Exceptions;
using OrderCloud.Integrations.CardConnect.Extensions;
using OrderCloud.Integrations.CardConnect.Mappers;
using OrderCloud.Integrations.CardConnect.Models;

namespace OrderCloud.Integrations.CardConnect.Tests
{
    public static class ResponseCodeFactory
    {
        public static IEnumerable AuthFailCases
        {
            get
            {
                yield return new TestCaseData(@"{'respstat': 'B', 'respcode': 'NU', 'cvvresp': 'M', 'avsresp': 'Y'}");
                yield return new TestCaseData(@"{'respstat': 'C', 'respcode': '05', 'cvvresp': 'M', 'avsresp': 'Y'}");
                yield return new TestCaseData(@"{'respstat': 'A', 'respcode': '101', 'cvvresp': 'N', 'avsresp': 'Y'}");
                yield return new TestCaseData(@"{'respstat': 'C', 'respcode': '00', 'cvvresp': 'P', 'avsresp': 'Y'}");
                yield return new TestCaseData(@"{'respstat': 'C', 'respcode': '101', 'cvvresp': 'U', 'avsresp': 'Y'}");
                yield return new TestCaseData(@"{'respstat': 'C', 'respcode': '00', 'cvvresp': 'M', 'avsresp': 'N'}");
                yield return new TestCaseData(@"{'respstat': 'A', 'respcode': '101', 'cvvresp': 'M', 'avsresp': 'A'}");
                yield return new TestCaseData(@"{'respstat': 'B', 'respcode': '00', 'cvvresp': 'M', 'avsresp': 'Z'}");
                yield return new TestCaseData(@"{'respstat': 'B', 'respcode': '100', 'cvvresp': 'M', 'avsresp': 'Z'}");
                yield return new TestCaseData(@"{'respstat': 'A', 'respcode': '101', 'cvvresp': 'P', 'avsresp': 'Y'}");
                yield return new TestCaseData(@"{'respstat': 'A', 'respcode': '500', 'cvvresp': 'P', 'avsresp': 'Y'}");
            }
        }

        public static IEnumerable VoidFailCases
        {
            get
            {
                yield return new TestCaseData(@"{'respproc': 'PPS', 'respstat': 'A'}");
                yield return new TestCaseData(@"{'respproc': 'PPS', 'respstat': 'C'}");
            }
        }
    }

    public class CardConnectTests
    {
        private HttpTest http;
        private CardConnectClient service;
        private CardConnectClient serviceNoConfig;

        [SetUp]
        public void Setup()
        {
            http = new HttpTest();
            service = new CardConnectClient(
                new CardConnectSettings()
                {
                    Authorization = "Authorization",
                    Site = "fts-uat",
                    BaseUrl = "cardconnect.com",
                },
                AppEnvironment.Test.ToString(),
                new PerBaseUrlFlurlClientFactory());

            serviceNoConfig = new CardConnectClient(new CardConnectSettings() { }, AppEnvironment.Test.ToString(), new PerBaseUrlFlurlClientFactory());
        }

        [TearDown]
        public void DisposeHttp()
        {
            http.Dispose();
        }

        [Test]
        public void ToCreditCardDisplay_ReturnsMaskedNumber()
        {
            // Arrange

            // Act
            var visa = "4444333322221111".ToCreditCardDisplay();
            var amex = "373485467448025".ToCreditCardDisplay();

            // Assert
            Assert.AreEqual("1111", visa);
            Assert.AreEqual("8025", amex);
        }

        [Test]
        public async Task AuthWithoutCapture_WithCredentials_CallsConstructedUrl()
        {
            // Arrange
            http.RespondWith(@"{'respstat': 'A', 'respcode': '00', 'cvvresp': 'M', 'avsresp': 'X'}");

            // Act
            await service.AuthWithoutCapture(new CardConnectAuthorizationRequest());

            // Assert
            http.ShouldHaveCalled("https://fts-uat.cardconnect.com/cardconnect/rest/auth");
        }

        [Test]
        public async Task Tokenize_WithCredentials_CallsConstructedUrl()
        {
            // Arrange
            http.RespondWith(@"{'account': 'test'}");

            // Act
            await service.Tokenize(new CardConnectAccountRequest());

            // Assert
            http.ShouldHaveCalled("https://fts-uat.cardconnect.com/cardsecure/api/v1/ccn/tokenize");
        }

        [Test]
        [TestCase(@"{'respstat': 'A', 'respcode': '0', 'cvvresp': 'M', 'avsresp': 'X'}", ResponseStatus.Approved)]
        [TestCase(@"{'respstat': 'A', 'respcode': '00', 'cvvresp': 'M', 'avsresp': 'X'}", ResponseStatus.Approved)]
        [TestCase(@"{'respstat': 'A', 'respcode': '000', 'cvvresp': 'M', 'avsresp': 'X'}", ResponseStatus.Approved)]
        [TestCase(@"{'respstat': 'A', 'respcode': '0', 'cvvresp': 'N', 'avsresp': 'Z'}", ResponseStatus.Approved)]
        public async Task AuthWithoutCapture_WithCredentials_ReturnsApprovedResponseStatus(string body, ResponseStatus result)
        {
            // Arrange
            http.RespondWith(body);

            // Act
            var call = await service.AuthWithoutCapture(new CardConnectAuthorizationRequest());

            // Assert
            Assert.That(call.respstat.ToResponseStatus() == result);
        }

        [Test, TestCaseSource(typeof(ResponseCodeFactory), nameof(ResponseCodeFactory.AuthFailCases))]
        public void AuthWithoutCapture_WithUnsuccessfulResponse_ThrowsCreditCardAuthorizationException(string body)
        {
            // Arrange
            http.RespondWith(body);

            // Act/Assert
            var ex = Assert.ThrowsAsync<CreditCardAuthorizationException>(() => service.AuthWithoutCapture(new CardConnectAuthorizationRequest() { cvv2 = "112" }));
        }

        [Test]
        [TestCase(@"{'respstat': 'A', 'respcode': '0', 'authcode': 'REVERS'}")]
        public async Task VoidAuthorization_WithSuccessfulResponse_ReturnsSuccessfulResponse(string body)
        {
            // Arrange
            http.RespondWith(body);

            // Act
            var call = await service.VoidAuthorization(new CardConnectVoidRequest());

            // Assert
            Assert.IsTrue(call.WasSuccessful());
        }

        [Test, TestCaseSource(typeof(ResponseCodeFactory), nameof(ResponseCodeFactory.VoidFailCases))]
        public void VoidAuthorization_WithUnsuccessfulResponse_ThrowsCreditCardVoidException(string body)
        {
            // Arrange
            http.RespondWith(body);

            // Act/Assert
            var ex = Assert.ThrowsAsync<CreditCardVoidException>(() => service.VoidAuthorization(new CardConnectVoidRequest() { }));
        }

        [Test]
        [TestCase(@"{'respstat': 'A', 'respcode': '00', 'cvvresp': 'P', 'avsresp': 'Y'}", ResponseStatus.Approved)]
        [TestCase(@"{'respstat': 'A', 'respcode': '00', 'cvvresp': null, 'avsresp': 'Y'}", ResponseStatus.Approved)]
        public async Task AuthWithoutCapture_WithoutCvv_ReturnsSuccessfulResponse(string body, ResponseStatus result)
        {
            // Arrange
            http.RespondWith(body);

            // Act
            var call = await service.AuthWithoutCapture(new CardConnectAuthorizationRequest()
            {
                cvv2 = null,
            });

            // Assert
            Assert.That(call.respstat.ToResponseStatus() == result);
        }
    }
}
