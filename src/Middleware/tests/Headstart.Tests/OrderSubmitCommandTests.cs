using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Headstart.API.Commands;
using Headstart.Common;
using Headstart.Common.Services.ShippingIntegration.Models;
using Headstart.Models;
using Headstart.Models.Headstart;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using OrderCloud.Catalyst;
using OrderCloud.Integrations.CardConnect;
using OrderCloud.Integrations.CardConnect.Models;
using OrderCloud.SDK;

namespace Headstart.Tests
{
    public class OrderSubmitCommandTests
    {
        private IOrderCloudClient oc;
        private AppSettings settings;
        private ICreditCardCommand card;
        private IOrderSubmitCommand sut;

        [SetUp]
        public void Setup()
        {
            oc = Substitute.For<IOrderCloudClient>();
            settings = Substitute.For<AppSettings>();
            settings.CardConnectSettings = new OrderCloudIntegrationsCardConnectConfig
            {
                UsdMerchantID = "mockUsdMerchantID",
                CadMerchantID = "mockCadMerchantID",
                EurMerchantID = "mockEurMerchantID",
            };
            settings.OrderCloudSettings = new OrderCloudSettings
            {
                IncrementorPrefix = "SEB",
            };
            card = Substitute.For<ICreditCardCommand>();
            card.AuthorizePayment(Arg.Any<OrderCloudIntegrationsCreditCardPayment>(), "mockUserToken", Arg.Any<string>())
                    .Returns(Task.FromResult(new Payment { }));

            oc.Orders.PatchAsync(OrderDirection.Incoming, "mockOrderID", Arg.Any<PartialOrder>()).Returns(Task.FromResult(new Order { ID = "SEB12345" }));
            oc.AuthenticateAsync().Returns(Task.FromResult(new TokenResponse { AccessToken = "mockToken" }));
            oc.Orders.SubmitAsync<HSOrder>(Arg.Any<OrderDirection>(), Arg.Any<string>(), Arg.Any<string>()).Returns(Task.FromResult(new HSOrder { ID = "submittedorderid" }));
            sut = new OrderSubmitCommand(oc, settings, card); // sut is subject under test
        }

        [Test]
        public void SubmitOrderAsync_WithSubmittedOrder_ThrowsAlreadySubmittedError()
        {
            // Arrange
            oc.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.Incoming, "mockOrderID").Returns(Task.FromResult(new HSOrderWorksheet
            {
                Order = new Models.HSOrder { ID = "mockOrderID", IsSubmitted = true },
            }));

            // Act
            var ex = Assert.ThrowsAsync<CatalystBaseException>(async () => await sut.SubmitOrderAsync("mockOrderID",  OrderDirection.Outgoing, new OrderCloudIntegrationsCreditCardPayment { }, "mockUserToken"));

            // Assert
            Assert.AreEqual("OrderSubmit.AlreadySubmitted", ex.Errors[0].ErrorCode);
        }

        [Test]
        public void SubmitOrderAsync_WithoutShippingSelections_ThrowsMissingShippingSelectionsError()
        {
            // Arrange
            oc.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.Incoming, "mockOrderID").Returns(Task.FromResult(new HSOrderWorksheet
            {
                Order = new HSOrder { ID = "mockOrderID", IsSubmitted = false },
                ShipEstimateResponse = new HSShipEstimateResponse
                {
                    ShipEstimates = new List<HSShipEstimate>()
                    {
                        new HSShipEstimate
                        {
                            SelectedShipMethodID = "FEDEX_GROUND",
                        },
                        new HSShipEstimate
                        {
                            SelectedShipMethodID = null,
                        },
                    },
                },
            }));

            // Act
            var ex = Assert.ThrowsAsync<CatalystBaseException>(async () => await sut.SubmitOrderAsync("mockOrderID",  OrderDirection.Outgoing, new OrderCloudIntegrationsCreditCardPayment { }, "mockUserToken"));

            // Assert
            Assert.AreEqual("OrderSubmit.MissingShippingSelections", ex.Errors[0].ErrorCode);
        }

        [Test]
        public void SubmitOrderAsync_WithoutPayment_ThrowsMissingPaymentError()
        {
            // Arrange
            oc.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.Incoming, "mockOrderID").Returns(Task.FromResult(new HSOrderWorksheet
            {
                Order = new HSOrder { ID = "mockOrderID", IsSubmitted = false },
                ShipEstimateResponse = new HSShipEstimateResponse
                {
                    ShipEstimates = new List<HSShipEstimate>()
                    {
                        new HSShipEstimate
                        {
                            SelectedShipMethodID = "FEDEX_GROUND",
                        },
                    },
                },
                LineItems = new List<HSLineItem>()
                {
                    new HSLineItem
                    {
                        Product = new HSLineItemProduct
                        {
                            xp = new ProductXp
                            {
                                ProductType = ProductType.Standard,
                            },
                        },
                    },
                },
            }));

            // Act
            var ex = Assert.ThrowsAsync<CatalystBaseException>(async () => await sut.SubmitOrderAsync("mockOrderID", OrderDirection.Outgoing, null, "mockUserToken"));

            // Assert
            Assert.AreEqual("OrderSubmit.MissingPayment", ex.Errors[0].ErrorCode);
        }

        [Test]
        public async Task SubmitOrderAsync_IsResubmitted_DoesNotIncrementOrderID()
        {
            // Arrange
            oc.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.Incoming, "mockOrderID").Returns(Task.FromResult(new HSOrderWorksheet
            {
                Order = new HSOrder { ID = "mockOrderID", IsSubmitted = false, xp = new OrderXp { IsResubmitting = true } },
                ShipEstimateResponse = new HSShipEstimateResponse
                {
                    ShipEstimates = new List<HSShipEstimate>()
                    {
                        new HSShipEstimate
                        {
                            SelectedShipMethodID = "FEDEX_GROUND",
                        },
                    },
                },
                LineItems = new List<HSLineItem>()
                {
                    new HSLineItem
                    {
                        Product = new HSLineItemProduct
                        {
                            xp = new ProductXp
                            {
                                ProductType = ProductType.Standard,
                            },
                        },
                    },
                },
            }));

            // Act
            await sut.SubmitOrderAsync("mockOrderID",  OrderDirection.Outgoing, new OrderCloudIntegrationsCreditCardPayment(), "mockUserToken");

            // Assert
            await oc.Orders.DidNotReceive().PatchAsync(OrderDirection.Incoming, "mockOrderID", Arg.Any<PartialOrder>());
        }

        [Test]
        public async Task SubmitOrderAsync_IsNotResubmitted_IncrementsOrderID()
        {
            // Arrange
            oc.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.Incoming, "mockOrderID").Returns(Task.FromResult(new HSOrderWorksheet
            {
                Order = new HSOrder { ID = "mockOrderID", IsSubmitted = false, xp = new OrderXp { IsResubmitting = false } },
                ShipEstimateResponse = new HSShipEstimateResponse
                {
                    ShipEstimates = new List<HSShipEstimate>()
                    {
                        new HSShipEstimate
                        {
                            SelectedShipMethodID = "FEDEX_GROUND",
                        },
                    },
                },
                LineItems = new List<HSLineItem>()
                {
                    new HSLineItem
                    {
                        Product = new HSLineItemProduct
                        {
                            xp = new ProductXp
                            {
                                ProductType = ProductType.Standard,
                            },
                        },
                    },
                },
            }));

            // Act
            await sut.SubmitOrderAsync("mockOrderID",  OrderDirection.Outgoing, new OrderCloudIntegrationsCreditCardPayment(), "mockUserToken");

            // Assert
            await oc.Orders.Received().PatchAsync(OrderDirection.Incoming, "mockOrderID", Arg.Any<PartialOrder>());
        }

        [Test]
        public async Task SubmitOrderAsync_HasCreditCardPayment_AuthorizesPayment()
        {
            // Arrange
            oc.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.Incoming, "mockOrderID").Returns(Task.FromResult(new HSOrderWorksheet
            {
                Order = new HSOrder { ID = "mockOrderID", IsSubmitted = false, xp = new OrderXp { IsResubmitting = false } },
                ShipEstimateResponse = new HSShipEstimateResponse
                {
                    ShipEstimates = new List<HSShipEstimate>()
                    {
                        new HSShipEstimate
                        {
                            SelectedShipMethodID = "FEDEX_GROUND",
                        },
                    },
                },
                LineItems = new List<HSLineItem>()
                {
                    new HSLineItem
                    {
                        Product = new HSLineItemProduct
                        {
                            xp = new ProductXp
                            {
                                ProductType = ProductType.Standard,
                            },
                        },
                    },
                },
            }));

            // Act
            await sut.SubmitOrderAsync("mockOrderID",  OrderDirection.Outgoing, new OrderCloudIntegrationsCreditCardPayment() { CreditCardID = "mockCreditCardID" }, "mockUserToken");

            // Assert
            await card.Received().AuthorizePayment(Arg.Any<OrderCloudIntegrationsCreditCardPayment>(), "mockUserToken", Arg.Any<string>());
        }

        [Test]
        public async Task SubmitOrderAsync_ExceptionCaughtDuringOrderCloudSubmit_VoidsPayment()
        {
            // Arrange
            oc.Orders.SubmitAsync<HSOrder>(Arg.Any<OrderDirection>(), Arg.Any<string>(), Arg.Any<string>()).Throws(new Exception("Some error"));

            oc.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.Incoming, "mockOrderID").Returns(Task.FromResult(new HSOrderWorksheet
            {
                Order = new HSOrder { ID = "mockOrderID", IsSubmitted = false, xp = new OrderXp { IsResubmitting = false } },
                ShipEstimateResponse = new HSShipEstimateResponse
                {
                    ShipEstimates = new List<HSShipEstimate>()
                    {
                        new HSShipEstimate
                        {
                            SelectedShipMethodID = "FEDEX_GROUND",
                        },
                    },
                },
                LineItems = new List<HSLineItem>()
                {
                    new HSLineItem
                    {
                        Product = new HSLineItemProduct
                        {
                            xp = new ProductXp
                            {
                                ProductType = ProductType.Standard,
                            },
                        },
                    },
                },
            }));

            // Act
            Assert.ThrowsAsync<Exception>(async () => await sut.SubmitOrderAsync("mockOrderID", OrderDirection.Outgoing, new OrderCloudIntegrationsCreditCardPayment() { CreditCardID = "mockCreditCardID" }, "mockUserToken"));

            // Assert
            await card.Received().AuthorizePayment(Arg.Any<OrderCloudIntegrationsCreditCardPayment>(), "mockUserToken", Arg.Any<string>());
            await card.Received().VoidPaymentAsync("SEB12345", "mockUserToken");
        }

        [Test]
        public async Task SubmitOrderAsync_WithUSDCurrency_CallsAuthorizePaymentWithUSDMechant()
        {
            // Arrange
            oc.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.Incoming, "mockOrderID").Returns(Task.FromResult(new HSOrderWorksheet
            {
                Order = new HSOrder { ID = "mockOrderID", IsSubmitted = false, xp = new OrderXp { IsResubmitting = false } },
                ShipEstimateResponse = new HSShipEstimateResponse
                {
                    ShipEstimates = new List<HSShipEstimate>()
                    {
                        new HSShipEstimate
                        {
                            SelectedShipMethodID = "FEDEX_GROUND",
                        },
                    },
                },
                LineItems = new List<HSLineItem>()
                {
                    new HSLineItem
                    {
                        Product = new HSLineItemProduct
                        {
                            xp = new ProductXp
                            {
                                ProductType = ProductType.Standard,
                            },
                        },
                    },
                },
            }));

            // Act
            await sut.SubmitOrderAsync("mockOrderID",  OrderDirection.Outgoing, new OrderCloudIntegrationsCreditCardPayment { Currency = "USD", CreditCardID = "mockCreditCardID" }, "mockUserToken");

            // Assert
            await card.Received().AuthorizePayment(Arg.Any<OrderCloudIntegrationsCreditCardPayment>(), "mockUserToken", "mockUsdMerchantID");
        }

        [Test]
        public async Task SubmitOrderAsync_WithCADCurrency_CallsAuthorizePaymentWithCADMechant()
        {
            // Arrange
            oc.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.Incoming, "mockOrderID").Returns(Task.FromResult(new HSOrderWorksheet
            {
                Order = new HSOrder { ID = "mockOrderID", IsSubmitted = false, xp = new OrderXp { IsResubmitting = false } },
                ShipEstimateResponse = new HSShipEstimateResponse
                {
                    ShipEstimates = new List<HSShipEstimate>()
                    {
                        new HSShipEstimate
                        {
                            SelectedShipMethodID = "FEDEX_GROUND",
                        },
                    },
                },
                LineItems = new List<HSLineItem>()
                {
                    new HSLineItem
                    {
                        Product = new HSLineItemProduct
                        {
                            xp = new ProductXp
                            {
                                ProductType = ProductType.Standard,
                            },
                        },
                    },
                },
            }));

            // Act
            await sut.SubmitOrderAsync("mockOrderID",  OrderDirection.Outgoing, new OrderCloudIntegrationsCreditCardPayment { Currency = "CAD", CreditCardID = "mockCreditCardID" }, "mockUserToken");

            // Assert
            await card.Received().AuthorizePayment(Arg.Any<OrderCloudIntegrationsCreditCardPayment>(), "mockUserToken", "mockCadMerchantID");
        }

        [Test]
        public async Task SubmitOrderAsync_WithEURCurrency_CallsAuthorizePaymentWithEURMechant()
        {
            // use eur merchant account when currency is not USD and not CAD

            // Arrange
            oc.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.Incoming, "mockOrderID").Returns(Task.FromResult(new HSOrderWorksheet
            {
                Order = new HSOrder { ID = "mockOrderID", IsSubmitted = false, xp = new OrderXp { IsResubmitting = false } },
                ShipEstimateResponse = new HSShipEstimateResponse
                {
                    ShipEstimates = new List<HSShipEstimate>()
                    {
                        new HSShipEstimate
                        {
                            SelectedShipMethodID = "FEDEX_GROUND",
                        },
                    },
                },
                LineItems = new List<HSLineItem>()
                {
                    new HSLineItem
                    {
                        Product = new HSLineItemProduct
                        {
                            xp = new ProductXp
                            {
                                ProductType = ProductType.Standard,
                            },
                        },
                    },
                },
            }));

            // Act
            await sut.SubmitOrderAsync("mockOrderID",  OrderDirection.Outgoing, new OrderCloudIntegrationsCreditCardPayment { Currency = "MXN", CreditCardID = "mockCreditCardID" }, "mockUserToken");

            // Assert
            await card.Received().AuthorizePayment(Arg.Any<OrderCloudIntegrationsCreditCardPayment>(), "mockUserToken", "mockEurMerchantID");
        }

        [Test]
        public async Task SubmitOrderAsync_WithOutgoingOrderDirection_CallsOrderCloudSubmitWithMatchingOrderDirection()
        {
            // Arrange
            oc.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.Incoming, "mockOrderID").Returns(Task.FromResult(new HSOrderWorksheet
            {
                Order = new HSOrder { ID = "mockOrderID", IsSubmitted = false, xp = new OrderXp { IsResubmitting = false } },
                ShipEstimateResponse = new HSShipEstimateResponse
                {
                    ShipEstimates = new List<HSShipEstimate>()
                    {
                        new HSShipEstimate
                        {
                            SelectedShipMethodID = "FEDEX_GROUND",
                        },
                    },
                },
                LineItems = new List<HSLineItem>()
                {
                    new HSLineItem
                    {
                        Product = new HSLineItemProduct
                        {
                            xp = new ProductXp
                            {
                                ProductType = ProductType.Standard,
                            },
                        },
                    },
                },
            }));

            // Act
            await sut.SubmitOrderAsync("mockOrderID", OrderDirection.Outgoing, new OrderCloudIntegrationsCreditCardPayment { }, "mockUserToken");

            // Assert
            await oc.Orders.Received().SubmitAsync<HSOrder>(OrderDirection.Outgoing, Arg.Any<string>(), Arg.Any<string>());
        }

        [Test]
        public async Task SubmitOrderAsync_WithIngoingOrderDirection_CallsOrderCloudSubmitWithMatchingOrderDirection()
        {
            // call order submit with direction incoming

            // Arrange
            oc.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.Incoming, "mockOrderID").Returns(Task.FromResult(new HSOrderWorksheet
            {
                Order = new HSOrder { ID = "mockOrderID", IsSubmitted = false, xp = new OrderXp { IsResubmitting = false } },
                ShipEstimateResponse = new HSShipEstimateResponse
                {
                    ShipEstimates = new List<HSShipEstimate>()
                    {
                        new HSShipEstimate
                        {
                            SelectedShipMethodID = "FEDEX_GROUND",
                        },
                    },
                },
                LineItems = new List<HSLineItem>()
                {
                    new HSLineItem
                    {
                        Product = new HSLineItemProduct
                        {
                            xp = new ProductXp
                            {
                                ProductType = ProductType.Standard,
                            },
                        },
                    },
                },
            }));

            // Act
            await sut.SubmitOrderAsync("mockOrderID", OrderDirection.Incoming, new OrderCloudIntegrationsCreditCardPayment { }, "mockUserToken");

            // Assert
            await oc.Orders.Received().SubmitAsync<HSOrder>(OrderDirection.Incoming, Arg.Any<string>(), Arg.Any<string>());
        }
    }
}
