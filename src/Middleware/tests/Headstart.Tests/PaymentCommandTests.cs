using Headstart.API.Commands;
using Headstart.Tests.Mocks;
using NSubstitute;
using NUnit.Framework;
using ordercloud.integrations.cardconnect;
using OrderCloud.SDK;
using System.Threading.Tasks;
using Headstart.Common.Models.Headstart;

namespace Headstart.Tests
{
	public class PaymentCommandTests
	{
		private IOrderCloudClient _oc;
		private ICreditCardCommand _ccCommand;
		private IPaymentCommand _sut;
		private string mockOrderID = "mockOrderID";
		private string mockUserToken = "mockUserToken";
		private string mockCCPaymentID = "mockCCPaymentID";
		private string mockPoPaymentID = "mockPoPaymentID";
		private string creditcard1 = "creditcard1";
		private string creditcard2 = "creditcard2";
		private string mockSpendingAccountID = "mockSpendingAccountID";

		[SetUp]
		public void Setup()
		{
			// oc
			_oc = Substitute.For<IOrderCloudClient>();
			_oc.IntegrationEvents.GetWorksheetAsync<HsOrderWorksheet>(OrderDirection.Incoming, mockOrderID)
				.Returns(Task.FromResult(new HsOrderWorksheet { Order = new HsOrder
				{
					ID = mockOrderID, Total = 20
				}}));
			_oc.Payments.CreateAsync<HsPayment>(OrderDirection.Incoming, mockOrderID, Arg.Any<HsPayment>()) 
				.Returns(Task.FromResult(PaymentMocks.CCPayment(creditcard1, 20)));

			// ccCommand
			_ccCommand = Substitute.For<ICreditCardCommand>();
			_ccCommand.VoidTransactionAsync(Arg.Any<HsPayment>(), Arg.Any<HsOrder>(), mockUserToken)
				.Returns(Task.FromResult(0));

			_sut = new PaymentCommand(_oc, _ccCommand);
		}

		[Test]
		public async Task should_delete_stale_payments()
		{
			// Arrange
			var mockedCreditCardTotal = 20;
			var existing = PaymentMocks.PaymentList(PaymentMocks.SpendingAccountPayment(20));
			_oc.Payments.ListAsync<HsPayment>(OrderDirection.Incoming, mockOrderID)
				.Returns(Task.FromResult(existing));
			var requested = PaymentMocks.Payments(PaymentMocks.CCPayment(creditcard1));

			// Act
			var result = await _sut.SavePayments(mockOrderID, requested, mockUserToken);

			// Assert
			await _oc.Payments.Received().DeleteAsync(OrderDirection.Incoming, mockOrderID, mockSpendingAccountID);
			await _oc.Payments.Received().CreateAsync<HsPayment>(OrderDirection.Outgoing, mockOrderID, Arg.Is<HsPayment>(p => p.ID == mockCCPaymentID && p.Type == PaymentType.CreditCard && p.Amount == mockedCreditCardTotal), mockUserToken);
		}

		[Test] public async Task should_handle_new_cc_payment()
		{
			// Arrange
			var mockedCreditCardTotal = 20;
			var existing = PaymentMocks.EmptyPaymentsList();
			_oc.Payments.ListAsync<HsPayment>(OrderDirection.Incoming, mockOrderID)
				.Returns(Task.FromResult(existing));
			var requested = PaymentMocks.Payments(PaymentMocks.CCPayment(creditcard1));

			// Act
			var result = await _sut.SavePayments(mockOrderID, requested, mockUserToken);

			// Assert
			await _oc.Payments.DidNotReceive().DeleteAsync(OrderDirection.Incoming, mockOrderID, Arg.Any<string>());
			await _oc.Payments.Received().CreateAsync<HsPayment>(OrderDirection.Outgoing, mockOrderID, Arg.Is<HsPayment>(p => p.ID == mockCCPaymentID && p.Type == PaymentType.CreditCard && p.Amount == mockedCreditCardTotal && p.Accepted == false), mockUserToken);
		}

		[Test]
		public async Task should_handle_same_cc_different_amount()
		{
			// if the credit card hasn't changed but the amount has
			// then we should void any existing transactions if necessary and update the payment 

			// Arrange
			var mockedCreditCardTotal = 20;
			var existing = PaymentMocks.PaymentList(PaymentMocks.CCPayment(creditcard1, 30));
			_oc.Payments.ListAsync<HsPayment>(OrderDirection.Incoming, mockOrderID)
				.Returns(Task.FromResult(existing));
			var requested = PaymentMocks.Payments(PaymentMocks.CCPayment(creditcard1));

			// Act
			var result = await _sut.SavePayments(mockOrderID, requested, mockUserToken);

			// Assert
			await _oc.Payments.DidNotReceive().DeleteAsync(OrderDirection.Incoming, mockOrderID, Arg.Any<string>());
			await _ccCommand.Received().VoidTransactionAsync(Arg.Is<HsPayment>(p => p.ID == mockCCPaymentID), Arg.Is<HsOrder>(o => o.ID == mockOrderID), mockUserToken);
			await _oc.Payments.Received().PatchAsync<HsPayment>(OrderDirection.Incoming, mockOrderID, mockCCPaymentID, Arg.Is<PartialPayment>(p => p.Amount == mockedCreditCardTotal && p.Accepted == false));
		}

		[Test]
		public async Task should_handle_different_cc_same_amount()
		{
			// if the credit card has changed we need to delete the payment
			// but should void the existing authorization before that

			// Arrange
			var mockedCreditCardTotal = 20;
			var existing = PaymentMocks.PaymentList(PaymentMocks.CCPayment(creditcard1, 20, mockCCPaymentID));
			_oc.Payments.ListAsync<HsPayment>(OrderDirection.Incoming, mockOrderID)
				.Returns(Task.FromResult(existing));
			var requested = PaymentMocks.Payments(PaymentMocks.CCPayment(creditcard2, 20));

			// Act
			var result = await _sut.SavePayments(mockOrderID, requested, mockUserToken);

			// Assert
			await _oc.Payments.Received().DeleteAsync(OrderDirection.Incoming, mockOrderID, mockCCPaymentID);
			await _ccCommand.Received().VoidTransactionAsync(Arg.Is<HsPayment>(p => p.ID == mockCCPaymentID), Arg.Is<HsOrder>(o => o.ID == mockOrderID), mockUserToken);
			await _oc.Payments.Received().CreateAsync<HsPayment>(OrderDirection.Outgoing, mockOrderID, Arg.Is<HsPayment>(p => p.CreditCardID == creditcard2 && p.Amount == mockedCreditCardTotal && p.Accepted == false), mockUserToken);
		}

		[Test]
		public async Task should_handle_different_cc_different_amount()
		{
			// if the credit card has changed we need to delete the payment
			// but should void the existing authorization before that

			// Arrange
			var mockedCreditCardTotal = 20;
			var existing = PaymentMocks.PaymentList(PaymentMocks.CCPayment(creditcard1, 40, mockCCPaymentID));
			_oc.Payments.ListAsync<HsPayment>(OrderDirection.Incoming, mockOrderID)
				.Returns(Task.FromResult(existing));
			var requested = PaymentMocks.Payments(PaymentMocks.CCPayment(creditcard2));

			// Act
			var result = await _sut.SavePayments(mockOrderID, requested, mockUserToken);

			// Assert
			await _oc.Payments.Received().DeleteAsync(OrderDirection.Incoming, mockOrderID, mockCCPaymentID);
			await _ccCommand.Received().VoidTransactionAsync(Arg.Is<HsPayment>(p => p.ID == mockCCPaymentID), Arg.Is<HsOrder>(o => o.ID == mockOrderID), mockUserToken);
			await _oc.Payments.Received().CreateAsync<HsPayment>(OrderDirection.Outgoing, mockOrderID, Arg.Is<HsPayment>(p => p.CreditCardID == creditcard2 && p.Amount == mockedCreditCardTotal && p.Accepted == false), mockUserToken);
		}

		[Test]
		public async Task should_handle_same_cc_same_amount()
		{
			// do nothing, payment doesn't need updating

			// Arrange
			var existing = PaymentMocks.PaymentList(PaymentMocks.CCPayment(creditcard1, 20, mockCCPaymentID));
			_oc.Payments.ListAsync<HsPayment>(OrderDirection.Incoming, mockOrderID)
				.Returns(Task.FromResult(existing));
			var requested = PaymentMocks.Payments(PaymentMocks.CCPayment(creditcard1));

			// Act
			var result = await _sut.SavePayments(mockOrderID, requested, mockUserToken);

			// Assert
			await _oc.Payments.DidNotReceive().DeleteAsync(OrderDirection.Incoming, mockOrderID, Arg.Any<string>());
			await _ccCommand.DidNotReceive().VoidTransactionAsync(Arg.Any<HsPayment>(), Arg.Any<HsOrder>(), mockUserToken);
			await _oc.Payments.DidNotReceive().CreateAsync<HsPayment>(Arg.Any<OrderDirection>(), mockOrderID, Arg.Any<HsPayment>(), mockUserToken);
			await _oc.Payments.DidNotReceive().PatchAsync<HsPayment>(Arg.Any<OrderDirection>(), mockOrderID, Arg.Any<string>(), Arg.Any<PartialPayment>());
		}

		[Test]
		public async Task should_handle_new_po_payment()
		{
			// Arrange
			var mockedPOTotal = 20;
			var existing = PaymentMocks.EmptyPaymentsList();
			_oc.Payments.ListAsync<HsPayment>(OrderDirection.Incoming, mockOrderID)
				.Returns(Task.FromResult(existing));
			var requested = PaymentMocks.Payments(PaymentMocks.POPayment());

			// Act
			var result = await _sut.SavePayments(mockOrderID, requested, mockUserToken);

			// Assert
			await _oc.Payments.Received().CreateAsync<HsPayment>(OrderDirection.Incoming, mockOrderID, Arg.Is<HsPayment>(p => p.ID == mockPoPaymentID && p.Amount == mockedPOTotal));
		}

		[Test]
		public async Task should_handle_existing_po_payment()
		{
			// Arrange
			var mockedPOTotal = 20;
			var existing = PaymentMocks.PaymentList(PaymentMocks.POPayment(40));
			_oc.Payments.ListAsync<HsPayment>(OrderDirection.Incoming, mockOrderID)
				.Returns(Task.FromResult(existing));
			var requested = PaymentMocks.Payments(PaymentMocks.POPayment());

			// Act
			var result = await _sut.SavePayments(mockOrderID, requested, mockUserToken);

			// Assert
			await _oc.Payments.Received().PatchAsync<HsPayment>(OrderDirection.Incoming, mockOrderID, mockPoPaymentID, Arg.Is<PartialPayment>(p => p.Amount == mockedPOTotal));
		}
	}
}
