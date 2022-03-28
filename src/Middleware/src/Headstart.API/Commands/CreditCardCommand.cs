using System;
using System.Linq;
using OrderCloud.SDK;
using Headstart.Common;
using OrderCloud.Catalyst;
using Sitecore.Diagnostics;
using System.Threading.Tasks;
using Headstart.Common.Services;
using Headstart.Common.Models.Headstart;
using ordercloud.integrations.exchangerates;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Foundation.SitecoreExtensions.MVC.Extensions;

namespace ordercloud.integrations.cardconnect
{
	public interface ICreditCardCommand
	{
		Task<BuyerCreditCard> MeTokenizeAndSave(OrderCloudIntegrationsCreditCardToken card, DecodedToken decodedToken);
		Task<CreditCard> TokenizeAndSave(string buyerId, OrderCloudIntegrationsCreditCardToken card, DecodedToken decodedToken);
		Task<Payment> AuthorizePayment(OrderCloudIntegrationsCreditCardPayment payment, string userToken, string merchantId);
		Task VoidTransactionAsync(HsPayment payment, HsOrder order, string userToken);
		Task VoidPaymentAsync(string orderId, string userToken);
	}

	public class CreditCardCommand : ICreditCardCommand
	{
		private readonly IOrderCloudIntegrationsCardConnectService _cardConnect;
		private readonly IOrderCloudClient _oc;
		private readonly IHsExchangeRatesService _hsExchangeRates;
		private readonly ISupportAlertService _supportAlerts;
		private readonly AppSettings _settings;
		private readonly ConfigSettings _configSettings = ConfigSettings.Instance;

		/// <summary>
		/// The IOC based constructor method for the CreditCardCommand class object with Dependency Injection
		/// </summary>
		/// <param name="card"></param>
		/// <param name="oc"></param>
		/// <param name="hsExchangeRates"></param>
		/// <param name="supportAlerts"></param>
		/// <param name="settings"></param>
		public CreditCardCommand(IOrderCloudIntegrationsCardConnectService card, IOrderCloudClient oc, IHsExchangeRatesService hsExchangeRates, ISupportAlertService supportAlerts, AppSettings settings)
		{
			try
			{
				_cardConnect = card;
				_oc = oc;
				_hsExchangeRates = hsExchangeRates;
				_supportAlerts = supportAlerts;
				_settings = settings;
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Public re-usable TokenizeAndSave task method
		/// </summary>
		/// <param name="buyerId"></param>
		/// <param name="card"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The newly created CreditCard response object from the TokenizeAndSave process</returns>
		public async Task<CreditCard> TokenizeAndSave(string buyerId, OrderCloudIntegrationsCreditCardToken card, DecodedToken decodedToken)
		{
			var creditCard = new CreditCard();
			try
			{
				creditCard = await _oc.CreditCards.CreateAsync(buyerId, await Tokenize(card, decodedToken.AccessToken), decodedToken.AccessToken);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return creditCard;
		}

		/// <summary>
		/// Public re-usable MeTokenizeAndSave task method
		/// </summary>
		/// <param name="card"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The newly created BuyerCreditCard response object from the MeTokenizeAndSave process</returns>
		public async Task<BuyerCreditCard> MeTokenizeAndSave(OrderCloudIntegrationsCreditCardToken card, DecodedToken decodedToken)
		{
			var buyerCreditCard = new BuyerCreditCard();
			try
			{
				buyerCreditCard = await _oc.Me.CreateCreditCardAsync(await MeTokenize(card, decodedToken.AccessToken), decodedToken.AccessToken);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return buyerCreditCard;
		}

		/// <summary>
		/// Public re-usable AuthorizePayment task method
		/// </summary>
		/// <param name="payment"></param>
		/// <param name="userToken"></param>
		/// <param name="merchantId"></param>
		/// <returns>The authorized Payment response object from the AuthorizePayment process</returns>
		/// <exception cref="CatalystBaseException"></exception>
		public async Task<Payment> AuthorizePayment(OrderCloudIntegrationsCreditCardPayment payment, string userToken, string merchantId)
		{
			Require.That((payment.CreditCardID != null) || (payment.CreditCardDetails != null), new ErrorCode(@"CreditCard.CreditCardAuth", @"The Request must include either CreditCardDetails or CreditCardID."));
			var cc = await GetMeCardDetails(payment, userToken);
			Require.That(payment.IsValidCvv(cc), new ErrorCode(@"CreditCardAuth.InvalidCvv", @"The CVV is required for Credit Card Payment."));
			Require.That(cc.Token != null, new ErrorCode(@"CreditCardAuth.InvalidToken", @"The Credit card must have valid authorization token."));
			Require.That(cc.xp.CCBillingAddress != null, new ErrorCode(@"Invalid Bill Address", @"The Credit card must have a billing address."));

			var orderWorksheet = await _oc.IntegrationEvents.GetWorksheetAsync<HsOrderWorksheet>(OrderDirection.Incoming, payment.OrderID);
			var order = orderWorksheet.Order;
			Require.That(!order.IsSubmitted, new ErrorCode(@"CreditCardAuth.AlreadySubmitted", @"The Order has already been submitted."));

			var ccAmount = orderWorksheet.Order.Total;
			var ocPaymentsList = (await _oc.Payments.ListAsync<HsPayment>(OrderDirection.Incoming, payment.OrderID, filters: @"Type=CreditCard"));
			var ocPayments = ocPaymentsList.Items;
			var ocPayment = ocPayments.Any() ? ocPayments[0] : null;
			if (ocPayment == null)
			{
				var ex = new CatalystBaseException(@"Payment.MissingCreditCardPayment", @"The Order is missing credit card payment.");
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
				throw ex;
			}
			try
			{
				if (ocPayment?.Accepted == true)
				{
					if (ocPayment.Amount == ccAmount)
					{
						return ocPayment;
					}
					await VoidTransactionAsync(ocPayment, order, userToken);
				}
				var call = await _cardConnect.AuthWithoutCapture(CardConnectMapper.Map(cc, order, payment, merchantId, ccAmount));
				ocPayment = await _oc.Payments.PatchAsync<HsPayment>(OrderDirection.Incoming, order.ID, ocPayment.ID, new PartialPayment { Accepted = true, Amount = ccAmount });
				return await _oc.Payments.CreateTransactionAsync(OrderDirection.Incoming, order.ID, ocPayment.ID, CardConnectMapper.Map(ocPayment, call));
			}
			catch (CreditCardAuthorizationException ex)
			{
				var ex1 = new CatalystBaseException($@"CreditCardAuth.{ex.ApiError.ErrorCode}", $@"{ex.ApiError.Message}.", ex.Response);
				ocPayment = await _oc.Payments.PatchAsync<HsPayment>(OrderDirection.Incoming, order.ID, ocPayment.ID, new PartialPayment { Accepted = false, Amount = ccAmount });
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", $@"{ex.Message}. {ex1.Message}", ex.StackTrace, this, true);
				return await _oc.Payments.CreateTransactionAsync(OrderDirection.Incoming, order.ID, ocPayment.ID, CardConnectMapper.Map(ocPayment, ex.Response));
			}
		}

		/// <summary>
		/// Public re-usable VoidPaymentAsync task method
		/// </summary>
		/// <param name="orderId"></param>
		/// <param name="userToken"></param>
		/// <returns></returns>
		public async Task VoidPaymentAsync(string orderId, string userToken)
		{
			try
			{
				var order = await _oc.Orders.GetAsync<HsOrder>(OrderDirection.Incoming, orderId);
				var paymentList = await _oc.Payments.ListAsync<HsPayment>(OrderDirection.Incoming, order.ID);
				var payment = paymentList.Items.Any() ? paymentList.Items[0] : null;
				if (payment == null)
				{
					return;
				}

				await VoidTransactionAsync(payment, order, userToken);
				await _oc.Payments.PatchAsync(OrderDirection.Incoming, orderId, payment.ID, new PartialPayment { Accepted = false });
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Public re-usable VoidTransactionAsync task method
		/// </summary>
		/// <param name="payment"></param>
		/// <param name="order"></param>
		/// <param name="userToken"></param>
		/// <returns></returns>
		/// <exception cref="CatalystBaseException"></exception>
		public async Task VoidTransactionAsync(HsPayment payment, HsOrder order, string userToken)
        {
			var transactionID = string.Empty;
			try
			{
				if (payment.Accepted == true)
				{
					var transaction = payment.Transactions.Where(x => x.Type.Equals(@"CreditCard", StringComparison.OrdinalIgnoreCase)).OrderBy(x => x.DateExecuted).LastOrDefault(t => t.Succeeded);
					var retref = transaction?.xp?.CardConnectResponse?.retref;
					if (retref != null)
					{
						transactionID = transaction.ID;
						var userCurrency = await _hsExchangeRates.GetCurrencyForUser(userToken);
						var response = await _cardConnect.VoidAuthorization(new CardConnectVoidRequest
						{
							currency = userCurrency.ToString(),
							merchid = GetMerchantID(userCurrency),
							retref = transaction.xp.CardConnectResponse.retref
						});
						await _oc.Payments.CreateTransactionAsync(OrderDirection.Incoming, order.ID, payment.ID, CardConnectMapper.Map(payment, response));
					}
				}
			}
			catch (CreditCardVoidException ex)
			{
				await _supportAlerts.VoidAuthorizationFailed(payment, transactionID, order, ex);
				await _oc.Payments.CreateTransactionAsync(OrderDirection.Incoming, order.ID, payment.ID, CardConnectMapper.Map(payment, ex.Response));
				var ex1 = new CatalystBaseException(@"Payment.FailedToVoidAuthorization", $@"{ex.ApiError.Message}.");
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", $@"{ex.Message}. {ex1.Message}", ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Private re-usable GetMerchantID task method
		/// </summary>
		/// <param name="userCurrency"></param>
		/// <returns>The MerchantID string value</returns>
		private string GetMerchantID(CurrencySymbol userCurrency)
		{
			var resp = string.Empty;
			try
			{
				if (userCurrency == CurrencySymbol.USD)
				{
					resp = _settings.CardConnectSettings.UsdMerchantID;
				}
				else if (userCurrency == CurrencySymbol.CAD)
				{
					resp = _settings.CardConnectSettings.CadMerchantID;
				}
				else
				{
					resp = _settings.CardConnectSettings.EurMerchantID;
				}
			}
			catch (CreditCardVoidException ex)
			{

				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Private re-usable GetMeCardDetails task method
		/// </summary>
		/// <param name="payment"></param>
		/// <param name="userToken"></param>
		/// <returns>The CardConnectBuyerCreditCard response object from the GetMeCardDetails process</returns>
		private async Task<CardConnectBuyerCreditCard> GetMeCardDetails(OrderCloudIntegrationsCreditCardPayment payment, string userToken)
		{
			var resp = await MeTokenize(payment.CreditCardDetails, userToken);
			try
			{
				if (payment.CreditCardID != null)
				{
					resp = await _oc.Me.GetCreditCardAsync<CardConnectBuyerCreditCard>(payment.CreditCardID, userToken);
				}
			}
			catch (CreditCardVoidException ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Private re-usable MeTokenize task method
		/// </summary>
		/// <param name="card"></param>
		/// <param name="userToken"></param>
		/// <returns>The CardConnectBuyerCreditCard response object from the MeTokenize process</returns>
		private async Task<CardConnectBuyerCreditCard> MeTokenize(OrderCloudIntegrationsCreditCardToken card, string userToken)
		{
			var resp = new CardConnectBuyerCreditCard();
			try
			{
				var userCurrency = await _hsExchangeRates.GetCurrencyForUser(userToken);
				var auth = await _cardConnect.Tokenize(CardConnectMapper.Map(card, userCurrency.ToString()));
				resp = BuyerCreditCardMapper.Map(card, auth);
			}
			catch (CreditCardVoidException ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Private re-usable Tokenize task method
		/// </summary>
		/// <param name="card"></param>
		/// <param name="userToken"></param>
		/// <returns>The CreditCard response object from the Tokenize process</returns>
		private async Task<CreditCard> Tokenize(OrderCloudIntegrationsCreditCardToken card, string userToken)
		{
			var resp = new CreditCard();
			try
			{
				var userCurrency = await _hsExchangeRates.GetCurrencyForUser(userToken);
				var auth = await _cardConnect.Tokenize(CardConnectMapper.Map(card, userCurrency.ToString()));
				resp = CreditCardMapper.Map(card, auth);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}
	}
}