using System;
using System.Linq;
using OrderCloud.SDK;
using Newtonsoft.Json;
using Headstart.Common;
using OrderCloud.Catalyst;
using Headstart.API.Commands;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Headstart.Common.Models.Headstart;
using ordercloud.integrations.cardconnect;
using Headstart.Common.Repositories.Models;

namespace Headstart.Jobs
{
	public class PaymentCaptureJob : BaseTimerJob
	{
		private readonly AppSettings _settings;
		private readonly IOrderCloudClient _oc;
		private readonly IOrderCloudIntegrationsCardConnectService _cardConnect;
		private readonly IRMACommand _rmaCommand;

		public PaymentCaptureJob(AppSettings settings, IOrderCloudClient oc, IOrderCloudIntegrationsCardConnectService cardConnect, IRMACommand rmaCommand)
		{
			_settings = settings;
			_oc = oc;
			_cardConnect = cardConnect;
			_rmaCommand = rmaCommand;
		}

		protected override bool ShouldRun => _settings.JobSettings.ShouldCaptureCreditCardPayments;

		protected override async Task ProcessJob()
		{
			var filters = new Dictionary<string, object>
			{
				[@"DateSubmitted"] = $@">{_settings.JobSettings.CaptureCreditCardsAfterDate}", // Limiting scope of orders until all past orders have been updated with xp.IsPaymentCaptured=true
				[@"xp.PaymentMethod"] = @"Credit Card",
				[@"xp.IsPaymentCaptured"] = @"false|!*", // TODO: once this is in place for a week set xp.IsPaymentCaptured to true on all past orders so this filter can be more performant
				[@"IsSubmitted"] = true,
				[@"xp.SubmittedOrderStatus"] = @"!Canceled"
			};

			var orders = await  _oc.Orders.ListAllAsync<HsOrder>(OrderDirection.Incoming, filters: filters);
			_logger.LogInformation($@"Found {orders.Count} orders to process.");

			await Throttler.RunAsync(orders, 100, 5, ProcessSingleOrder);
		}

		private async Task ProcessSingleOrder(HsOrder order)
		{
			try
			{
				var rmaList = await _rmaCommand.ListRMAsByOrderId(order.ID, CommerceRole.Seller, new MeUser { });
				if (rmaList.Items.Any(x => x.Status == RMAStatus.Complete))
				{
					LogSkip($@"{order.ID} has been refunded - RMA process handles authorizing new partial amount if necessary.");
					await _oc.Orders.PatchAsync(OrderDirection.Incoming, order.ID, new PartialOrder
					{
						xp = new
						{
							IsPaymentCaptured = true
						}
					});
					return;
				}

				var payment = await GetValidPaymentAsync(order);
				var transaction = GetValidTransaction(order.ID, payment);
				if (await HasBeenCapturedPreviouslyAsync(order, transaction))
				{
					LogSkip($@"{order.ID} has already been captured.");
					await _oc.Orders.PatchAsync(OrderDirection.Incoming, order.ID, new PartialOrder
					{
						xp = new
						{
							IsPaymentCaptured = true
						}
					});
				}
				else
				{
					await CapturePaymentAsync(order, payment, transaction);
					await _oc.Orders.PatchAsync(OrderDirection.Incoming, order.ID, new PartialOrder
					{
						xp = new
						{
							IsPaymentCaptured = true
						}
					});
					LogSuccess(order.ID);
				}
			}
			catch (OrderCloudException ex)
			{
				LogFailure($@"{ex?.InnerException?.Message} { JsonConvert.SerializeObject(ex.Errors)}. OrderID: {order.ID}.");
			}
			catch (PaymentCaptureJobException ex)
			{
				LogFailure(ex.Message);
			}
			catch (Exception ex)
			{
				LogFailure($@"{ex.Message}. OrderID: {order.ID}.");
			}
		}

		private async Task<HsPayment> GetValidPaymentAsync(HsOrder order)
		{
			if (order.xp.Currency == null)
			{
				throw new PaymentCaptureJobException(@"Order.xp.Currency is null", order.ID);
			}

			var paymentList = await _oc.Payments.ListAsync<HsPayment>(OrderDirection.Incoming, order.ID, filters: new { Accepted = true, Type = "CreditCard" });
			if (paymentList.Items.Count == 0)
			{
				throw new PaymentCaptureJobException(@"No credit card payment on the order where Accepted=true", order.ID);
			}
			return paymentList.Items[0];
		}

		private HsPaymentTransaction GetValidTransaction(string orderId, HsPayment payment)
		{
			var ordered = payment.Transactions.OrderBy(x => x.DateExecuted);
			var transaction = payment.Transactions
				.OrderBy(x => x.DateExecuted)
				.LastOrDefault(x => x.Type == @"CreditCard" && x.Succeeded);
			if (transaction == null)
			{
				throw new PaymentCaptureJobException(@"No valid payment authorization on the order", orderId, payment.ID);
			}
			if (transaction?.xp?.CardConnectResponse == null)
			{
				throw new PaymentCaptureJobException(@"Missing transaction.xp.CardConnectResponse", orderId, payment.ID, transaction.ID);
			}

			var authHasBeenVoided = payment.Transactions.Any(t =>
				t.Type == @"CreditCardVoidAuthorization" &&
				t.Succeeded &&
				t.xp?.CardConnectResponse?.retref == transaction.xp?.CardConnectResponse?.retref
			);
			if (authHasBeenVoided)
			{
				throw new PaymentCaptureJobException(@"Payment authorization has been voided", orderId, payment.ID, transaction.ID);
			}
			return transaction;
		}

		private async Task<bool> HasBeenCapturedPreviouslyAsync(HsOrder order, HsPaymentTransaction transaction)
		{
			var inquire = await _cardConnect.Inquire(new CardConnectInquireRequest
			{
				merchid = transaction.xp.CardConnectResponse.merchid,
				orderid = order.ID,
				set = "1",
				currency = order.xp.Currency.ToString(),
				retref = transaction.xp.CardConnectResponse.retref
			});
			return !string.IsNullOrEmpty(inquire.capturedate);
		}

		private async Task CapturePaymentAsync(HsOrder order, HsPayment payment, HsPaymentTransaction transaction)
		{
			try
			{
				var response = await _cardConnect.Capture(new CardConnectCaptureRequest
				{
					merchid = transaction.xp.CardConnectResponse.merchid,
					retref = transaction.xp.CardConnectResponse.retref,
					currency = order.xp.Currency.ToString()
				});
				await _oc.Payments.CreateTransactionAsync(OrderDirection.Incoming, order.ID, payment.ID, CardConnectMapper.Map(payment, response));
			}
			catch (CardConnectInquireException ex)
			{
				throw new PaymentCaptureJobException($@"Error inquiring payment. Message: {ex.ApiError.Message}, ErrorCode: {ex.ApiError.ErrorCode}.", order.ID, payment.ID, transaction.ID);
			}
			catch (CardConnectCaptureException ex)
			{
				await _oc.Payments.CreateTransactionAsync(OrderDirection.Incoming, order.ID, payment.ID, CardConnectMapper.Map(payment, ex.Response));
				throw new PaymentCaptureJobException($@"Error capturing payment. Message: {ex.ApiError.Message}, ErrorCode: {ex.ApiError.ErrorCode}.", order.ID, payment.ID, transaction.ID);
			}
		}
	}

	public class PaymentCaptureJobException : Exception
	{
		public PaymentCaptureJobException(string message, string orderId)
			: base($"{message}. OrderID: {orderId}")
		{
		}

		public PaymentCaptureJobException(string message, string orderId, string paymentId)
			: base($"{message}. OrderID: {orderId}. PaymentID: {paymentId}")
		{
		}

		public PaymentCaptureJobException(string message, string orderId, string paymentId, string transactionId)
			: base($"{message}. OrderID: {orderId}. PaymentID: {paymentId}. TransactionID: {transactionId}")
		{
		}
	}
}