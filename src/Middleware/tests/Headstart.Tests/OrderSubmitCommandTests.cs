using NUnit.Framework;
using NSubstitute;
using OrderCloud.SDK;
using System.Collections.Generic;
using Headstart.Common;
using ordercloud.integrations.cardconnect;
using System.Threading.Tasks;
using Headstart.Common.Services.ShippingIntegration.Models;
using Headstart.Models.Headstart;
using Headstart.Models;
using System;
using NSubstitute.ExceptionExtensions;
using Headstart.API.Commands;
using OrderCloud.Catalyst;

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
        public void should_throw_if_order_is_already_submitted()
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
        public void should_throw_if_order_is_missing_shipping_selections()
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
        public void should_throw_if_has_standard_lines_and_missing_payment()
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
        public async Task should_not_increment_orderid_if_is_resubmitting()
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
        public async Task should_not_increment_orderid_if_is_already_incremented()
        {
            // Arrange
            oc.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.Incoming, "mockOrderID").Returns(Task.FromResult(new HSOrderWorksheet
            {
                Order = new HSOrder { ID = "SEBmockOrderID", IsSubmitted = false, xp = new OrderXp { } },
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
        public async Task should_increment_orderid_if_has_not_been_incremented_and_is_not_resubmit()
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
        public async Task should_capture_credit_card_payment_if_has_standard_lineitems()
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
        public async Task should_void_payment_if_error_on_submit()
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
        public async Task should_use_usd_merchant_when_appropriate()
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
        public async Task should_use_cad_merchant_when_appropriate()
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
        public async Task should_use_eur_merchant_when_appropriate()
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
        public async Task should_handle_direction_outgoing()
        {
            // call order submit with direction outgoing

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
        public async Task should_handle_direction_incoming()
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
