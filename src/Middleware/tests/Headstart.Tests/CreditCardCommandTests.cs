using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Headstart.API.Commands;
using Headstart.Common;
using Headstart.Common.Services;
using Headstart.Common.Services.ShippingIntegration.Models;
using Headstart.Models;
using Headstart.Models.Headstart;
using Headstart.Tests.Mocks;
using NSubstitute;
using NUnit.Framework;
using OrderCloud.Catalyst;
using OrderCloud.Integrations.CardConnect;
using OrderCloud.Integrations.CardConnect.Models;
using OrderCloud.Integrations.ExchangeRates.Models;
using OrderCloud.Integrations.Library.Models;
using OrderCloud.SDK;

namespace Headstart.Tests
{
    public class CreditCardCommandTests
    {
        private IOrderCloudIntegrationsCardConnectService cardConnect;
        private IOrderCloudClient oc;
        private IHSExchangeRatesService hsExchangeRates;
        private ISupportAlertService supportAlerts;
        private AppSettings settings;
        private ICreditCardCommand sut;

        private string validretref = "myretref";
        private CurrencyCode currency = CurrencyCode.CAD;
        private string merchantID = "123";
        private string cvv = "112";
        private string orderID = "mockOrderID";
        private string userToken = "mockUserToken";
        private string creditCardID = "mockCreditCardID";
        private string ccToken = "mockCcToken";
        private decimal ccTotal = 38;
        private string paymentID = "mockPayment";
        private string transactionID = "trans1";

        [SetUp]
        public void Setup()
        {
            cardConnect = Substitute.For<IOrderCloudIntegrationsCardConnectService>();
            cardConnect.VoidAuthorization(Arg.Is<CardConnectVoidRequest>(r => r.merchid == merchantID))
                .Returns(Task.FromResult(new CardConnectVoidResponse { }));
            cardConnect.AuthWithoutCapture(Arg.Any<CardConnectAuthorizationRequest>())
                .Returns(Task.FromResult(new CardConnectAuthorizationResponse { authcode = "REVERS" }));

            oc = Substitute.For<IOrderCloudClient>();
            oc.Me.GetCreditCardAsync<CardConnectBuyerCreditCard>(creditCardID, userToken)
                .Returns(MockCreditCard());
            oc.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.Incoming, orderID)
                .Returns(Task.FromResult(new HSOrderWorksheet { Order = new HSOrder { ID = orderID, Total = 38 } }));
            oc.Payments.CreateTransactionAsync<HSPayment>(OrderDirection.Incoming, orderID, Arg.Any<string>(), Arg.Any<PaymentTransaction>())
                .Returns(Task.FromResult(new HSPayment { }));
            oc.Payments.PatchAsync<HSPayment>(OrderDirection.Incoming, orderID, Arg.Any<string>(), Arg.Any<PartialPayment>())
                .Returns(Task.FromResult(new HSPayment { }));

            hsExchangeRates = Substitute.For<IHSExchangeRatesService>();
            hsExchangeRates.GetCurrencyForUser(userToken)
                .Returns(Task.FromResult(currency));

            supportAlerts = Substitute.For<ISupportAlertService>();
            settings = Substitute.For<AppSettings>();
            settings.CardConnectSettings.CadMerchantID = merchantID;

            sut = new CreditCardCommand(cardConnect, oc, hsExchangeRates, supportAlerts, settings);
        }

        [Test]
        public void AuthorizePayment_WithNoPayments_ThrowsMissingCreditCardPaymentError()
        {
            // Arrange
            oc.Payments.ListAsync<HSPayment>(OrderDirection.Incoming, orderID, filters: Arg.Is<object>(f => (string)f == "Type=CreditCard"))
                .Returns(Task.FromResult(PaymentMocks.EmptyPaymentsList()));
            var payment = ValidIntegrationsPayment();

            // Act
            var ex = Assert.ThrowsAsync<CatalystBaseException>(async () => await sut.AuthorizePayment(payment, userToken, merchantID));

            // Assert
            Assert.AreEqual("Payment.MissingCreditCardPayment", ex.Errors[0].ErrorCode);
        }

        [Test]
        public async Task AuthorizePayment_PaymentAcceptedMatchingOrderTotal_ShoudNotAuthorize()
        {
            // Arrange
            oc.Payments.ListAsync<HSPayment>(OrderDirection.Incoming, orderID, filters: Arg.Is<object>(f => (string)f == "Type=CreditCard"))
                .Returns(Task.FromResult(PaymentMocks.PaymentList(PaymentMocks.CCPayment("creditcardid1", 38))));
            var payment = ValidIntegrationsPayment();

            // Act
            await sut.AuthorizePayment(payment, userToken, merchantID);

            // Assert
            await cardConnect.DidNotReceive().AuthWithCapture(Arg.Any<CardConnectAuthorizationRequest>());
            await oc.Payments.DidNotReceive().CreateTransactionAsync(OrderDirection.Incoming, orderID, Arg.Any<string>(), Arg.Any<PaymentTransaction>());
        }

        [Test]
        public async Task AuthorizePayment_PaymentAcceptedDoesNotMatchOrderTotal_VoidsAuthorization()
        {
            // Arrange
            var paymentTotal = 30; // credit card total is 38
            var payment1transactions = new List<HSPaymentTransaction>()
            {
                new HSPaymentTransaction
                {
                    ID = transactionID,
                    Succeeded = true,
                    Type = "CreditCard",
                    xp = new TransactionXP
                    {
                        CardConnectResponse = new CardConnectAuthorizationResponse
                        {
                            retref = validretref,
                        },
                    },
                },
            };
            oc.Payments.ListAsync<HSPayment>(OrderDirection.Incoming, orderID, filters: Arg.Is<object>(f => (string)f == "Type=CreditCard"))
                .Returns(PaymentMocks.PaymentList(MockCCPayment(paymentTotal, true, payment1transactions)));
            var payment = ValidIntegrationsPayment();

            // Act
            await sut.AuthorizePayment(payment, userToken, merchantID);

            // Assert
            await cardConnect.Received().VoidAuthorization(Arg.Is<CardConnectVoidRequest>(x => x.retref == validretref && x.merchid == merchantID && x.currency == "CAD"));
            await oc.Payments.Received().CreateTransactionAsync(OrderDirection.Incoming, orderID, paymentID, Arg.Is<PaymentTransaction>(x => x.Amount == paymentTotal));
            await cardConnect.Received().AuthWithoutCapture(Arg.Any<CardConnectAuthorizationRequest>());
            await oc.Payments.Received().PatchAsync<HSPayment>(OrderDirection.Incoming, orderID, paymentID, Arg.Is<PartialPayment>(p => p.Accepted == true && p.Amount == ccTotal));
            await oc.Payments.Received().CreateTransactionAsync(OrderDirection.Incoming, orderID, paymentID, Arg.Any<PaymentTransaction>());
        }

        [Test]
        public async Task AuthorizePayment_PaymentAcceptedDoesNotMatchOrderTotalAndMultipleTransactions_VoidsAuthorization()
        {
            // Arrange
            var paymentTotal = 30; // credit card total is 38
            var payment1transactions = new List<HSPaymentTransaction>()
            {
                new HSPaymentTransaction
                {
                    ID = "authattempt1",
                    Succeeded = true,
                    Type = "CreditCard",
                    xp = new TransactionXP
                    {
                        CardConnectResponse = new CardConnectAuthorizationResponse
                        {
                            retref = "retref1",
                        },
                    },
                },
                new HSPaymentTransaction
                {
                    ID = "voidattempt1",
                    Succeeded = true,
                    Type = "CreditCardVoidAuthorization",
                    xp = new TransactionXP
                    {
                        CardConnectResponse = new CardConnectAuthorizationResponse
                        {
                            retref = "retref2",
                        },
                    },
                },
                new HSPaymentTransaction
                {
                    ID = "authattempt2",
                    Type = "CreditCard",
                    Succeeded = true,
                    xp = new TransactionXP
                    {
                        CardConnectResponse = new CardConnectAuthorizationResponse
                        {
                            retref = "retref3",
                        },
                    },
                },
            };
            oc.Payments.ListAsync<HSPayment>(OrderDirection.Incoming, orderID, filters: Arg.Is<object>(f => (string)f == "Type=CreditCard"))
                .Returns(PaymentMocks.PaymentList(MockCCPayment(paymentTotal, true, payment1transactions)));
            var payment = ValidIntegrationsPayment();

            // Act
            await sut.AuthorizePayment(payment, userToken, merchantID);

            // Assert
            await cardConnect.Received().VoidAuthorization(Arg.Is<CardConnectVoidRequest>(x => x.retref == "retref3" && x.merchid == merchantID && x.currency == "CAD"));
            await oc.Payments.Received().CreateTransactionAsync(OrderDirection.Incoming, orderID, paymentID, Arg.Is<PaymentTransaction>(x => x.Amount == paymentTotal));
            await cardConnect.Received().AuthWithoutCapture(Arg.Any<CardConnectAuthorizationRequest>());
            await oc.Payments.Received().PatchAsync<HSPayment>(OrderDirection.Incoming, orderID, paymentID, Arg.Is<PartialPayment>(p => p.Accepted == true && p.Amount == ccTotal));
            await oc.Payments.Received().CreateTransactionAsync(OrderDirection.Incoming, orderID, paymentID, Arg.Any<PaymentTransaction>());
        }

        [Test]
        public async Task AuthorizePayment_USDPaymentAcceptedDoesNotMatchOrderTotalAndMultipleTransactions_VoidsAuthorization()
        {
            // Arrange
            var paymentTotal = 30; // credit card total is 38
            var payment1transactions = new List<HSPaymentTransaction>()
            {
                new HSPaymentTransaction
                {
                    ID = "authattempt1",
                    Succeeded = true,
                    Type = "CreditCard",
                    xp = new TransactionXP
                    {
                        CardConnectResponse = new CardConnectAuthorizationResponse
                        {
                            retref = "retref1",
                        },
                    },
                },
                new HSPaymentTransaction
                {
                    ID = "voidattempt1",
                    Succeeded = true,
                    Type = "CreditCardVoidAuthorization",
                    xp = new TransactionXP
                    {
                        CardConnectResponse = new CardConnectAuthorizationResponse
                        {
                            retref = "retref2",
                        },
                    },
                },
                new HSPaymentTransaction
                {
                    ID = "authattempt2",
                    Type = "CreditCard",
                    Succeeded = true,
                    xp = new TransactionXP
                    {
                        CardConnectResponse = new CardConnectAuthorizationResponse
                        {
                            retref = "retref3",
                        },
                    },
                },
            };
            settings.CardConnectSettings.UsdMerchantID = merchantID;
            settings.CardConnectSettings.CadMerchantID = "somethingelse";
            hsExchangeRates.GetCurrencyForUser(userToken)
                .Returns(Task.FromResult(CurrencyCode.USD));
            oc.Payments.ListAsync<HSPayment>(OrderDirection.Incoming, orderID, filters: Arg.Is<object>(f => (string)f == "Type=CreditCard"))
                .Returns(PaymentMocks.PaymentList(MockCCPayment(paymentTotal, true, payment1transactions)));
            var payment = ValidIntegrationsPayment();

            // Act
            await sut.AuthorizePayment(payment, userToken, merchantID);

            // Assert
            await cardConnect.Received().VoidAuthorization(Arg.Is<CardConnectVoidRequest>(x => x.retref == "retref3" && x.merchid == merchantID && x.currency == "USD"));
            await oc.Payments.Received().CreateTransactionAsync(OrderDirection.Incoming, orderID, paymentID, Arg.Is<PaymentTransaction>(x => x.Amount == paymentTotal));
            await cardConnect.Received().AuthWithoutCapture(Arg.Any<CardConnectAuthorizationRequest>());
            await oc.Payments.Received().PatchAsync<HSPayment>(OrderDirection.Incoming, orderID, paymentID, Arg.Is<PartialPayment>(p => p.Accepted == true && p.Amount == ccTotal));
            await oc.Payments.Received().CreateTransactionAsync(OrderDirection.Incoming, orderID, paymentID, Arg.Any<PaymentTransaction>());
        }

        [Test]
        public async Task AuthorizePayment_WithFailedAuthorization_CallsPaymentTransaction()
        {
            // this gives us full insight into transaction history
            // as well as sets Accepted to false

            // Arrange
            var paymentTotal = 38; // credit card total is 38
            var payment1transactions = new List<HSPaymentTransaction>()
            {
                new HSPaymentTransaction
                {
                    ID = transactionID,
                    Succeeded = true,
                    xp = new TransactionXP
                    {
                        CardConnectResponse = new CardConnectAuthorizationResponse
                        {
                            retref = validretref,
                        },
                    },
                },
            };
            var mockedCCPayment = MockCCPayment(paymentTotal, false, payment1transactions);
            oc.Payments.ListAsync<HSPayment>(OrderDirection.Incoming, orderID, filters: Arg.Is<object>(f => (string)f == "Type=CreditCard"))
                .Returns(PaymentMocks.PaymentList(mockedCCPayment));
            oc.Payments.PatchAsync<HSPayment>(OrderDirection.Incoming, orderID, Arg.Any<string>(), Arg.Any<PartialPayment>())
                .Returns(Task.FromResult(mockedCCPayment));
            var payment = ValidIntegrationsPayment();
            cardConnect
                .When(x => x.AuthWithoutCapture(Arg.Any<CardConnectAuthorizationRequest>()))
                .Do(x => throw new CreditCardAuthorizationException(new ApiError { }, new CardConnectAuthorizationResponse { }));

            // Act
            var ex = Assert.ThrowsAsync<CatalystBaseException>(async () => await sut.AuthorizePayment(payment, userToken, merchantID));

            // Assert
            Assert.AreEqual("CreditCardAuth.", ex.Errors[0].ErrorCode);
            await cardConnect.Received().AuthWithoutCapture(Arg.Any<CardConnectAuthorizationRequest>());

            // stuff that happens in catch block
            await oc.Payments.Received().PatchAsync<HSPayment>(OrderDirection.Incoming, orderID, paymentID, Arg.Is<PartialPayment>(p => p.Accepted == false && p.Amount == ccTotal));
            await oc.Payments.Received().CreateTransactionAsync(OrderDirection.Incoming, orderID, paymentID, Arg.Any<PaymentTransaction>());
        }

        [Test]
        public async Task AuthorizePayment_ThrowsExceptionInVoidAuthorization_CallsPaymentTransaction()
        {
            // creates a new transaction when auth fails which
            // gives us full insight into transaction history as well as sets Accepted to false
            var paymentTotal = 30; // credit card total is 38

            // Arrange
            var payment1transactions = new List<HSPaymentTransaction>()
            {
                new HSPaymentTransaction
                {
                    ID = transactionID,
                    Succeeded = true,
                    Type = "CreditCard",
                    xp = new TransactionXP
                    {
                        CardConnectResponse = new CardConnectAuthorizationResponse
                        {
                            retref = validretref,
                        },
                    },
                },
            };
            var mockedCCPayment = MockCCPayment(paymentTotal, true, payment1transactions);
            oc.Payments.ListAsync<HSPayment>(OrderDirection.Incoming, orderID, filters: Arg.Is<object>(f => (string)f == "Type=CreditCard"))
                .Returns(PaymentMocks.PaymentList(mockedCCPayment));
            var payment = ValidIntegrationsPayment();
            cardConnect
                .When(x => x.VoidAuthorization(Arg.Any<CardConnectVoidRequest>()))
                .Do(x => throw new CreditCardVoidException(new ApiError { }, new CardConnectVoidResponse { }));

            // Act
            var ex = Assert.ThrowsAsync<CatalystBaseException>(async () => await sut.AuthorizePayment(payment, userToken, merchantID));

            // Assert
            Assert.AreEqual("Payment.FailedToVoidAuthorization", ex.Errors[0].ErrorCode);

            // stuff that happens in catch block
            await supportAlerts
                .Received()
                .VoidAuthorizationFailed(Arg.Any<HSPayment>(), transactionID, Arg.Any<HSOrder>(), Arg.Any<CreditCardVoidException>());

            await oc.Payments.Received().CreateTransactionAsync(OrderDirection.Incoming, orderID, paymentID, Arg.Any<PaymentTransaction>());
        }

        private HSPayment MockCCPayment(decimal amount, bool accepted = false, List<HSPaymentTransaction> transactions = null)
        {
            return new HSPayment
            {
                ID = paymentID,
                Type = PaymentType.CreditCard,
                Amount = amount,
                Accepted = accepted,
                xp = new PaymentXP { },
                Transactions = new ReadOnlyCollection<HSPaymentTransaction>(transactions ?? new List<HSPaymentTransaction>()),
            };
        }

        private Task<CardConnectBuyerCreditCard> MockCreditCard()
        {
            return Task.FromResult(new CardConnectBuyerCreditCard
            {
                Token = ccToken,
                ExpirationDate = new DateTimeOffset(),
                xp = new CreditCardXP
                {
                    CCBillingAddress = new Address { },
                },
            });
        }

        private OrderCloudIntegrationsCreditCardPayment ValidIntegrationsPayment()
        {
            return new OrderCloudIntegrationsCreditCardPayment
            {
                CVV = cvv,
                CreditCardID = creditCardID,
                OrderID = orderID,
            };
        }
    }
}
