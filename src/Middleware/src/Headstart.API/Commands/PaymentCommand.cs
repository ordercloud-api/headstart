using System;
using System.Linq;
using OrderCloud.SDK;
using Headstart.Common;
using Sitecore.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Headstart.Common.Models.Headstart;
using ordercloud.integrations.cardconnect;
using Sitecore.Foundation.SitecoreExtensions.Extensions;

namespace Headstart.API.Commands
{
	public interface IPaymentCommand
	{
		Task<IList<HsPayment>> SavePayments(string orderID, List<HsPayment> requestedPayments, string userToken);
	}

	public class PaymentCommand : IPaymentCommand
	{
		private readonly IOrderCloudClient _oc;
		private readonly ICreditCardCommand _ccCommand;
		private readonly AppSettings _settings;

		/// <summary>
		/// The IOC based constructor method for the PaymentCommand class object with Dependency Injection
		/// </summary>
		/// <param name="oc"></param>
		/// <param name="ccCommand"></param>
		/// <param name="settings"></param>
		public PaymentCommand(IOrderCloudClient oc, ICreditCardCommand ccCommand, AppSettings settings)
		{
			try
			{
				_settings = settings;
				_oc = oc;
				_ccCommand = ccCommand;
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Public re-usable SavePayments task method
		/// </summary>
		/// <param name="orderId"></param>
		/// <param name="requestedPayments"></param>
		/// <param name="userToken"></param>
		/// <returns>The list of HsPayment response objects from the SavePayments process</returns>
		public async Task<IList<HsPayment>> SavePayments(string orderId, List<HsPayment> requestedPayments, string userToken)
		{
			var resp = new List<HsPayment>();
			try
			{
				var worksheet = await _oc.IntegrationEvents.GetWorksheetAsync<HsOrderWorksheet>(OrderDirection.Incoming, orderId);
				var existingPayments = (await _oc.Payments.ListAsync<HsPayment>(OrderDirection.Incoming, orderId)).Items;
				existingPayments = await DeleteStalePaymentsAsync(requestedPayments, existingPayments, worksheet.Order, userToken);

				foreach (var requestedPayment in requestedPayments)
				{
					var existingPayment = existingPayments.FirstOrDefault(p => p.Type == requestedPayment.Type);
					if (requestedPayment.Type == PaymentType.CreditCard)
					{
						await UpdateCCPaymentAsync(requestedPayment, existingPayment, worksheet, userToken);
					}
					if (requestedPayment.Type == PaymentType.PurchaseOrder)
					{
						await UpdatePoPaymentAsync(requestedPayment, existingPayment, worksheet);
					}
				}

				resp = (await _oc.Payments.ListAsync<HsPayment>(OrderDirection.Incoming, orderId)).Items.ToList();
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;            
		}

		/// <summary>
		/// Private re-usable UpdateCCPaymentAsync task method
		/// </summary>
		/// <param name="requestedPayment"></param>
		/// <param name="existingPayment"></param>
		/// <param name="worksheet"></param>
		/// <param name="userToken"></param>
		/// <returns></returns>
		private async Task UpdateCCPaymentAsync(HsPayment requestedPayment, HsPayment existingPayment, HsOrderWorksheet worksheet, string userToken)
		{
			try
			{
				var paymentAmount = worksheet.Order.Total;
				if (existingPayment == null)
				{
					requestedPayment.Amount = paymentAmount;
					requestedPayment.Accepted = false;
					requestedPayment.Type = requestedPayment.Type;
					await _oc.Payments.CreateAsync<HsPayment>(OrderDirection.Outgoing, worksheet.Order.ID, requestedPayment, userToken); // Need user token because admins cant see personal credit cards
				}
				else if (existingPayment.CreditCardID == requestedPayment.CreditCardID && existingPayment.Amount == paymentAmount)
				{
					// Do nothing, payment doesn't need updating
					return;
				}
				else if (existingPayment.CreditCardID == requestedPayment.CreditCardID)
				{
					await _ccCommand.VoidTransactionAsync(existingPayment, worksheet.Order, userToken);
					await _oc.Payments.PatchAsync<HsPayment>(OrderDirection.Incoming, worksheet.Order.ID, existingPayment.ID, new PartialPayment
					{
						Accepted = false,
						Amount = paymentAmount,
						xp = requestedPayment.xp
					});
				}
				else
				{
					// We need to delete payment because you can't have payments totaling more than order total and you can't set payments to $0
					await DeleteCreditCardPaymentAsync(existingPayment, worksheet.Order, userToken);
					requestedPayment.Amount = paymentAmount;
					requestedPayment.Accepted = false;
					await _oc.Payments.CreateAsync<HsPayment>(OrderDirection.Outgoing, worksheet.Order.ID, requestedPayment, userToken); // need user token because admins cant see personal credit cards
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Private re-usable UpdatePoPaymentAsync task method
		/// </summary>
		/// <param name="requestedPayment"></param>
		/// <param name="existingPayment"></param>
		/// <param name="worksheet"></param>
		/// <returns></returns>
		private async Task UpdatePoPaymentAsync(HsPayment requestedPayment, HsPayment existingPayment, HsOrderWorksheet worksheet)
		{
			try
			{
				var paymentAmount = worksheet.Order.Total;
				if (existingPayment == null)
				{
					requestedPayment.Amount = paymentAmount;
					await _oc.Payments.CreateAsync<HsPayment>(OrderDirection.Incoming, worksheet.Order.ID, requestedPayment);
				}
				else
				{
					await _oc.Payments.PatchAsync<HsPayment>(OrderDirection.Incoming, worksheet.Order.ID, existingPayment.ID, new PartialPayment
					{
						Amount = paymentAmount
					});
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Private re-usable DeleteCreditCardPaymentAsync task method
		/// </summary>
		/// <param name="payment"></param>
		/// <param name="order"></param>
		/// <param name="userToken"></param>
		/// <returns></returns>
		private async Task DeleteCreditCardPaymentAsync(HsPayment payment, HsOrder order, string userToken)
		{
			try
			{
				await _ccCommand.VoidTransactionAsync(payment, order, userToken);
				await _oc.Payments.DeleteAsync(OrderDirection.Incoming, order.ID, payment.ID);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Private re-usable DeleteStalePaymentsAsync task method
		/// </summary>
		/// <param name="requestedPayments"></param>
		/// <param name="existingPayments"></param>
		/// <param name="order"></param>
		/// <param name="userToken"></param>
		/// <returns>The list of HsPayment response objects from the DeleteStalePaymentsAsync process</returns>
		private async Task<IList<HsPayment>> DeleteStalePaymentsAsync(IList<HsPayment> requestedPayments, IList<HsPayment> existingPayments, HsOrder order, string userToken)
		{
			try
			{
				// The requestedPayments represents the payments that should be on the order
				// if there are any existing payments not reflected in requestedPayments then they should be deleted
				foreach (var existingPayment in existingPayments.ToList())
				{
					if (requestedPayments.Any(p => p.Type == existingPayment.Type))
					{
						continue;
					}

					if (existingPayment.Type == PaymentType.PurchaseOrder)
					{
						await _oc.Payments.DeleteAsync(OrderDirection.Incoming, order.ID, existingPayment.ID);
						existingPayments.Remove(existingPayment);
					}
					if (existingPayment.Type == PaymentType.CreditCard)
					{
						await DeleteCreditCardPaymentAsync(existingPayment, order, userToken);
						existingPayments.Remove(existingPayment);
					}
					else
					{
						await _oc.Payments.DeleteAsync(OrderDirection.Incoming, order.ID, existingPayment.ID);
						existingPayments.Remove(existingPayment);
					}
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return existingPayments;
		}
	}
}