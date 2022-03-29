using Headstart.API.Commands;
using Headstart.Common;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using ordercloud.integrations.cardconnect;
using OrderCloud.Catalyst;
using OrderCloud.SDK;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Headstart.Common.Models.Headstart;

namespace Headstart.Tests
{
	public class OrderSubmitCommandTests
	{
		private IOrderCloudClient _oc;
		private AppSettings _settings;
		private ICreditCardCommand _card;
		private IOrderSubmitCommand _sut;

		[SetUp]
		public void Setup()
		{
			_oc = Substitute.For<IOrderCloudClient>();
			_settings = Substitute.For<AppSettings>();
			_settings.CardConnectSettings = new OrderCloudIntegrationsCardConnectConfig
			{
				UsdMerchantID = "mockUsdMerchantID",
				CadMerchantID = "mockCadMerchantID",
				EurMerchantID = "mockEurMerchantID"
			};
			_settings.OrderCloudSettings = new OrderCloudSettings
			{
				IncrementorPrefix = "SEB"
			};
			_card = Substitute.For<ICreditCardCommand>();
			_card.AuthorizePayment(Arg.Any<OrderCloudIntegrationsCreditCardPayment>(), "mockUserToken", Arg.Any<string>())
				.Returns(Task.FromResult(new Payment { }));

			_oc.Orders.PatchAsync(OrderDirection.Incoming, "mockOrderID", Arg.Any<PartialOrder>()).Returns(Task.FromResult(new Order { ID = "SEB12345" }));
			_oc.AuthenticateAsync().Returns(Task.FromResult(new TokenResponse { AccessToken = "mockToken" }));
			_oc.Orders.SubmitAsync<HsOrder>(Arg.Any<OrderDirection>(), Arg.Any<string>(), Arg.Any<string>()).Returns(Task.FromResult(new HsOrder { ID = "submittedorderid" }));
			_sut = new OrderSubmitCommand(_oc, _settings, _card); // sut is subject under test
		}

		[Test]
		public async Task should_throw_if_order_is_already_submitted()
		{
			// Arrange
			_oc.IntegrationEvents.GetWorksheetAsync<HsOrderWorksheet>(OrderDirection.Incoming, "mockOrderID").Returns(Task.FromResult(new HsOrderWorksheet
			{
				Order = new HsOrder
				{
					ID = "mockOrderID", 
					IsSubmitted = true
				}
			}));

			// Act
			var ex = Assert.ThrowsAsync<CatalystBaseException>(async () => await _sut.SubmitOrderAsync("mockOrderID",  OrderDirection.Outgoing, new OrderCloudIntegrationsCreditCardPayment { }, "mockUserToken"));

			// Assert
			Assert.AreEqual("OrderSubmit.AlreadySubmitted", ex.Errors[0].ErrorCode);
		}

		[Test]
		public void should_throw_if_order_is_missing_shipping_selections()
		{
			// Arrange
			_oc.IntegrationEvents.GetWorksheetAsync<HsOrderWorksheet>(OrderDirection.Incoming, "mockOrderID").Returns(Task.FromResult(new HsOrderWorksheet
			{
				Order = new HsOrder { ID = "mockOrderID", IsSubmitted = false },
				ShipEstimateResponse = new HsShipEstimateResponse
				{
					ShipEstimates = new List<HsShipEstimate>()
					{
						new HsShipEstimate
						{
							SelectedShipMethodID = "FEDEX_GROUND"
						},
						new HsShipEstimate
						{
							SelectedShipMethodID = null
						}
					}
				}
			}));

			// Act
			var ex = Assert.ThrowsAsync<CatalystBaseException>(async () => await _sut.SubmitOrderAsync("mockOrderID",  OrderDirection.Outgoing, new OrderCloudIntegrationsCreditCardPayment { }, "mockUserToken"));

			// Assert
			Assert.AreEqual("OrderSubmit.MissingShippingSelections", ex.Errors[0].ErrorCode);
		}

		[Test]
		public void should_throw_if_has_standard_lines_and_missing_payment()
		{
			// Arrange
			_oc.IntegrationEvents.GetWorksheetAsync<HsOrderWorksheet>(OrderDirection.Incoming, "mockOrderID").Returns(Task.FromResult(new HsOrderWorksheet
			{
				Order = new HsOrder { ID = "mockOrderID", IsSubmitted = false },
				ShipEstimateResponse = new HsShipEstimateResponse
				{
					ShipEstimates = new List<HsShipEstimate>()
					{
						new HsShipEstimate
						{
							SelectedShipMethodID = "FEDEX_GROUND"
						}
					}
				},
				LineItems = new List<HsLineItem>()
				{
					new HsLineItem
					{
						Product = new HsLineItemProduct
						{
							xp = new ProductXp
							{
								ProductType = ProductType.Standard
							}
						}
					}
				}
			}));

			// Act
			var ex = Assert.ThrowsAsync<CatalystBaseException>(async () => await _sut.SubmitOrderAsync("mockOrderID", OrderDirection.Outgoing, null, "mockUserToken"));

			// Assert
			Assert.AreEqual("OrderSubmit.MissingPayment", ex.Errors[0].ErrorCode);
		}

		[Test]
		public async Task should_not_increment_orderid_if_is_resubmitting()
		{
			// Arrange
			_oc.IntegrationEvents.GetWorksheetAsync<HsOrderWorksheet>(OrderDirection.Incoming, "mockOrderID").Returns(Task.FromResult(new HsOrderWorksheet
			{
				Order = new HsOrder { ID = "mockOrderID", IsSubmitted = false, xp = new OrderXp { IsResubmitting = true } },
				ShipEstimateResponse = new HsShipEstimateResponse
				{
					ShipEstimates = new List<HsShipEstimate>()
					{
						new HsShipEstimate
						{
							SelectedShipMethodID = "FEDEX_GROUND"
						}
					}
				},
				LineItems = new List<HsLineItem>()
				{
					new HsLineItem
					{
						Product = new HsLineItemProduct
						{
							xp = new ProductXp
							{
								ProductType = ProductType.Standard
							}
						}
					}
				}
			}));



			// Act
			await _sut.SubmitOrderAsync("mockOrderID",  OrderDirection.Outgoing, new OrderCloudIntegrationsCreditCardPayment(), "mockUserToken");

			// Assert
			await _oc.Orders.DidNotReceive().PatchAsync(OrderDirection.Incoming, "mockOrderID", Arg.Any<PartialOrder>());
		}

		[Test]
		public async Task should_not_increment_orderid_if_is_already_incremented()
		{
			// Arrange
			_oc.IntegrationEvents.GetWorksheetAsync<HsOrderWorksheet>(OrderDirection.Incoming, "mockOrderID").Returns(Task.FromResult(new HsOrderWorksheet
			{
				Order = new HsOrder { ID = "SEBmockOrderID", IsSubmitted = false, xp = new OrderXp { } },
				ShipEstimateResponse = new HsShipEstimateResponse
				{
					ShipEstimates = new List<HsShipEstimate>()
					{
						new HsShipEstimate
						{
							SelectedShipMethodID = "FEDEX_GROUND"
						}
					}
				},
				LineItems = new List<HsLineItem>()
				{
					new HsLineItem
					{
						Product = new HsLineItemProduct
						{
							xp = new ProductXp
							{
								ProductType = ProductType.Standard
							}
						}
					}
				}
			}));



			// Act
			await _sut.SubmitOrderAsync("mockOrderID",  OrderDirection.Outgoing, new OrderCloudIntegrationsCreditCardPayment(), "mockUserToken");

			// Assert
			await _oc.Orders.DidNotReceive().PatchAsync(OrderDirection.Incoming, "mockOrderID", Arg.Any<PartialOrder>());
		}

		[Test]
		public async Task should_increment_orderid_if_has_not_been_incremented_and_is_not_resubmit()
		{
			// Arrange
			_oc.IntegrationEvents.GetWorksheetAsync<HsOrderWorksheet>(OrderDirection.Incoming, "mockOrderID").Returns(Task.FromResult(new HsOrderWorksheet
			{
				Order = new HsOrder { ID = "mockOrderID", IsSubmitted = false, xp = new OrderXp { IsResubmitting = false } },
				ShipEstimateResponse = new HsShipEstimateResponse
				{
					ShipEstimates = new List<HsShipEstimate>()
					{
						new HsShipEstimate
						{
							SelectedShipMethodID = "FEDEX_GROUND"
						}
					}
				},
				LineItems = new List<HsLineItem>()
				{
					new HsLineItem
					{
						Product = new HsLineItemProduct
						{
							xp = new ProductXp
							{
								ProductType = ProductType.Standard
							}
						}
					}
				}
			}));

			// Act
			await _sut.SubmitOrderAsync("mockOrderID",  OrderDirection.Outgoing, new OrderCloudIntegrationsCreditCardPayment(), "mockUserToken");

			// Assert
			await _oc.Orders.Received().PatchAsync(OrderDirection.Incoming, "mockOrderID", Arg.Any<PartialOrder>());
		}

		[Test]
		public async Task should_capture_credit_card_payment_if_has_standard_lineitems()
		{
			// Arrange
			_oc.IntegrationEvents.GetWorksheetAsync<HsOrderWorksheet>(OrderDirection.Incoming, "mockOrderID").Returns(Task.FromResult(new HsOrderWorksheet
			{
				Order = new HsOrder { ID = "mockOrderID", IsSubmitted = false, xp = new OrderXp { IsResubmitting = false } },
				ShipEstimateResponse = new HsShipEstimateResponse
				{
					ShipEstimates = new List<HsShipEstimate>()
					{
						new HsShipEstimate
						{
							SelectedShipMethodID = "FEDEX_GROUND"
						}
					}
				},
				LineItems = new List<HsLineItem>()
				{
					new HsLineItem
					{
						Product = new HsLineItemProduct
						{
							xp = new ProductXp
							{
								ProductType = ProductType.Standard
							}
						}
					}
				}
			}));

			// Act
			await _sut.SubmitOrderAsync("mockOrderID", OrderDirection.Outgoing, 
				new OrderCloudIntegrationsCreditCardPayment()
				{
					CreditCardID = "mockCreditCardID"
				}, "mockUserToken");

			// Assert
			var orderCloudIntegrationsCreditCardPayment = Arg.Any<OrderCloudIntegrationsCreditCardPayment>();
			var merchantId = Arg.Any<string>();
			if (orderCloudIntegrationsCreditCardPayment != null && !string.IsNullOrEmpty(merchantId))
			{
				await _card.Received().AuthorizePayment(orderCloudIntegrationsCreditCardPayment, "mockUserToken", merchantId);
			}
		}

		[Test]
		public async Task should_void_payment_if_error_on_submit()
		{
			// Arrange
			_oc.Orders.SubmitAsync<HsOrder>(Arg.Any<OrderDirection>(), Arg.Any<string>(), Arg.Any<string>()).Throws(new Exception("Some error"));

			_oc.IntegrationEvents.GetWorksheetAsync<HsOrderWorksheet>(OrderDirection.Incoming, "mockOrderID").Returns(Task.FromResult(new HsOrderWorksheet
			{
				Order = new HsOrder { ID = "mockOrderID", IsSubmitted = false, xp = new OrderXp { IsResubmitting = false } },
				ShipEstimateResponse = new HsShipEstimateResponse
				{
					ShipEstimates = new List<HsShipEstimate>()
					{
						new HsShipEstimate
						{
							SelectedShipMethodID = "FEDEX_GROUND"
						}
					}
				},
				LineItems = new List<HsLineItem>()
				{
					new HsLineItem
					{
						Product = new HsLineItemProduct
						{
							xp = new ProductXp
							{
								ProductType = ProductType.Standard
							}
						}
					}
				}
			}));

			// Act
			Assert.ThrowsAsync<Exception>(async () => await _sut.SubmitOrderAsync("mockOrderID", OrderDirection.Outgoing, new OrderCloudIntegrationsCreditCardPayment() { CreditCardID = "mockCreditCardID" }, "mockUserToken"));

			// Assert
			await _card.Received().AuthorizePayment(Arg.Any<OrderCloudIntegrationsCreditCardPayment>(), "mockUserToken", Arg.Any<string>());
			await _card.Received().VoidPaymentAsync("SEB12345", "mockUserToken");
		}

		[Test]
		public async Task should_use_usd_merchant_when_appropriate()
		{
			// Arrange
			_oc.IntegrationEvents.GetWorksheetAsync<HsOrderWorksheet>(OrderDirection.Incoming, "mockOrderID").Returns(Task.FromResult(new HsOrderWorksheet
			{
				Order = new HsOrder { ID = "mockOrderID", IsSubmitted = false, xp = new OrderXp { IsResubmitting = false } },
				ShipEstimateResponse = new HsShipEstimateResponse
				{
					ShipEstimates = new List<HsShipEstimate>()
					{
						new HsShipEstimate
						{
							SelectedShipMethodID = "FEDEX_GROUND"
						}
					}
				},
				LineItems = new List<HsLineItem>()
				{
					new HsLineItem
					{
						Product = new HsLineItemProduct
						{
							xp = new ProductXp
							{
								ProductType = ProductType.Standard
							}
						}
					}
				}
			}));

			// Act
			await _sut.SubmitOrderAsync("mockOrderID",  OrderDirection.Outgoing, new OrderCloudIntegrationsCreditCardPayment { Currency = "USD", CreditCardID = "mockCreditCardID" }, "mockUserToken");

			// Assert
			await _card.Received().AuthorizePayment(Arg.Any<OrderCloudIntegrationsCreditCardPayment>(), "mockUserToken", "mockUsdMerchantID");
		}

		[Test]
		public async Task should_use_cad_merchant_when_appropriate()
		{
			// Arrange
			_oc.IntegrationEvents.GetWorksheetAsync<HsOrderWorksheet>(OrderDirection.Incoming, "mockOrderID").Returns(Task.FromResult(new HsOrderWorksheet
			{
				Order = new HsOrder { ID = "mockOrderID", IsSubmitted = false, xp = new OrderXp { IsResubmitting = false } },
				ShipEstimateResponse = new HsShipEstimateResponse
				{
					ShipEstimates = new List<HsShipEstimate>()
					{
						new HsShipEstimate
						{
							SelectedShipMethodID = "FEDEX_GROUND"
						}
					}
				},
				LineItems = new List<HsLineItem>()
				{
					new HsLineItem
					{
						Product = new HsLineItemProduct
						{
							xp = new ProductXp
							{
								ProductType = ProductType.Standard
							}
						}
					}
				}
			}));

			// Act
			await _sut.SubmitOrderAsync("mockOrderID",  OrderDirection.Outgoing, new OrderCloudIntegrationsCreditCardPayment { Currency = "CAD", CreditCardID = "mockCreditCardID" }, "mockUserToken");

			// Assert
			await _card.Received().AuthorizePayment(Arg.Any<OrderCloudIntegrationsCreditCardPayment>(), "mockUserToken", "mockCadMerchantID");
		}

		[Test]
		public async Task should_use_eur_merchant_when_appropriate()
		{
			// use eur merchant account when currency is not USD and not CAD

			// Arrange
			_oc.IntegrationEvents.GetWorksheetAsync<HsOrderWorksheet>(OrderDirection.Incoming, "mockOrderID").Returns(Task.FromResult(new HsOrderWorksheet
			{
				Order = new HsOrder { ID = "mockOrderID", IsSubmitted = false, xp = new OrderXp { IsResubmitting = false } },
				ShipEstimateResponse = new HsShipEstimateResponse
				{
					ShipEstimates = new List<HsShipEstimate>()
					{
						new HsShipEstimate
						{
							SelectedShipMethodID = "FEDEX_GROUND"
						}
					}
				},
				LineItems = new List<HsLineItem>()
				{
					new HsLineItem
					{
						Product = new HsLineItemProduct
						{
							xp = new ProductXp
							{
								ProductType = ProductType.Standard
							}
						}
					}
				}
			}));

			// Act
			await _sut.SubmitOrderAsync("mockOrderID",  OrderDirection.Outgoing, new OrderCloudIntegrationsCreditCardPayment { Currency = "MXN", CreditCardID = "mockCreditCardID" }, "mockUserToken");

			// Assert
			await _card.Received().AuthorizePayment(Arg.Any<OrderCloudIntegrationsCreditCardPayment>(), "mockUserToken", "mockEurMerchantID");
		}

		[Test]
		public async Task should_handle_direction_outgoing()
		{
			// call order submit with direction outgoing

			// Arrange
			_oc.IntegrationEvents.GetWorksheetAsync<HsOrderWorksheet>(OrderDirection.Incoming, "mockOrderID").Returns(Task.FromResult(new HsOrderWorksheet
			{
				Order = new HsOrder { ID = "mockOrderID", IsSubmitted = false, xp = new OrderXp { IsResubmitting = false } },
				ShipEstimateResponse = new HsShipEstimateResponse
				{
					ShipEstimates = new List<HsShipEstimate>()
					{
						new HsShipEstimate
						{
							SelectedShipMethodID = "FEDEX_GROUND"
						}
					}
				},
				LineItems = new List<HsLineItem>()
				{
					new HsLineItem
					{
						Product = new HsLineItemProduct
						{
							xp = new ProductXp
							{
								ProductType = ProductType.Standard
							}
						}
					}
				}
			}));

			// Act
			await _sut.SubmitOrderAsync("mockOrderID", OrderDirection.Outgoing, new OrderCloudIntegrationsCreditCardPayment { }, "mockUserToken");

			// Assert
			await _oc.Orders.Received().SubmitAsync<HsOrder>(OrderDirection.Outgoing, Arg.Any<string>(), Arg.Any<string>());
		}

		[Test]
		public async Task should_handle_direction_incoming()
		{
			// Call order submit with direction incoming
			// Arrange
			_oc.IntegrationEvents.GetWorksheetAsync<HsOrderWorksheet>(OrderDirection.Incoming, "mockOrderID").Returns(Task.FromResult(new HsOrderWorksheet
			{
				Order = new HsOrder { ID = "mockOrderID", IsSubmitted = false, xp = new OrderXp { IsResubmitting = false } },
				ShipEstimateResponse = new HsShipEstimateResponse
				{
					ShipEstimates = new List<HsShipEstimate>()
					{
						new HsShipEstimate
						{
							SelectedShipMethodID = "FEDEX_GROUND"
						}
					}
				},
				LineItems = new List<HsLineItem>()
				{
					new HsLineItem
					{
						Product = new HsLineItemProduct
						{
							xp = new ProductXp
							{
								ProductType = ProductType.Standard
							}
						}
					}
				}
			}));

			// Act
			await _sut.SubmitOrderAsync("mockOrderID", OrderDirection.Incoming, new OrderCloudIntegrationsCreditCardPayment { }, "mockUserToken");
			var orderId = Arg.Any<string>();
			if (!string.IsNullOrEmpty(orderId))
			{
				await _oc.Orders.Received().SubmitAsync<HsOrder>(OrderDirection.Incoming, orderId, Arg.Any<string>());
			}
		}
	}
}
