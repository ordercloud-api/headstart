using System;
using System.Linq;
using System.Threading.Tasks;
using Headstart.Common;
using Headstart.Common.Services;
using Headstart.Common.Services.ShippingIntegration.Models;
using Headstart.Models;
using Headstart.Models.Headstart;
using OrderCloud.Catalyst;
using OrderCloud.Integrations.CardConnect;
using OrderCloud.Integrations.CardConnect.Mappers;
using OrderCloud.Integrations.CardConnect.Models;
using OrderCloud.SDK;
using OrderCloud.Integrations.ExchangeRates.Models;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using SitecoreExtensions = Sitecore.Foundation.SitecoreExtensions.Extensions;

namespace Headstart.API.Commands
{
	public interface ICreditCardCommand
	{
		Task<BuyerCreditCard> MeTokenizeAndSave(OrderCloudIntegrationsCreditCardToken card, DecodedToken decodedToken);
		Task<CreditCard> TokenizeAndSave(string buyerID, OrderCloudIntegrationsCreditCardToken card, DecodedToken decodedToken);
		Task<Payment> AuthorizePayment(OrderCloudIntegrationsCreditCardPayment payment, string userToken, string merchantID);
		Task VoidTransactionAsync(HSPayment payment, HSOrder order, string userToken);
		Task VoidPaymentAsync(string orderID, string userToken);
	}

	public class CreditCardCommand : ICreditCardCommand
	{
		private readonly IOrderCloudIntegrationsCardConnectService cardConnect;
		private readonly IOrderCloudClient oc;
		private readonly IHSExchangeRatesService hsExchangeRates;
		private readonly ISupportAlertService supportAlerts;
		private readonly AppSettings settings;

		/// <summary>
		/// The IOC based constructor method for the CreditCardCommand class object with Dependency Injection
		/// </summary>
		/// <param name="card"></param>
		/// <param name="oc"></param>
		/// <param name="hsExchangeRates"></param>
		/// <param name="supportAlerts"></param>
		/// <param name="settings"></param>
		public CreditCardCommand(
			IOrderCloudIntegrationsCardConnectService card, 
			IOrderCloudClient oc, 
			IHSExchangeRatesService hsExchangeRates, 
			ISupportAlertService supportAlerts, 
			AppSettings settings)
		{
			try
			{
				this.settings = settings;
				cardConnect = card;
	            this.oc = oc;
	            this.hsExchangeRates = hsExchangeRates;
	            this.supportAlerts = supportAlerts;
			}
			catch (Exception ex)
			{
				LoggingNotifications.LogApiResponseMessages(this.settings.LogSettings, SitecoreExtensions.Helpers.GetMethodName(), "",
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace, ex);
			}
		}

		/// <summary>
		/// Public re-usable TokenizeAndSave task method
		/// </summary>
		/// <param name="buyerID"></param>
		/// <param name="card"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The newly created CreditCard response object from the TokenizeAndSave process</returns>
		public async Task<CreditCard> TokenizeAndSave(string buyerID, OrderCloudIntegrationsCreditCardToken card, DecodedToken decodedToken)
		{
            var creditCard = await oc.CreditCards.CreateAsync(buyerID, await Tokenize(card, decodedToken.AccessToken), decodedToken.AccessToken);
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
            var buyerCreditCard = await oc.Me.CreateCreditCardAsync(await MeTokenize(card, decodedToken.AccessToken), decodedToken.AccessToken);
			return buyerCreditCard;
		}

		/// <summary>
		/// Public re-usable AuthorizePayment task method
		/// </summary>
		/// <param name="payment"></param>
		/// <param name="userToken"></param>
		/// <param name="merchantID"></param>
		/// <returns>The authorized Payment response object from the AuthorizePayment process</returns>
		/// <exception cref="CatalystBaseException"></exception>
		public async Task<Payment> AuthorizePayment(OrderCloudIntegrationsCreditCardPayment payment, string userToken, string merchantID)
		{
            Require.That(
                (payment.CreditCardID != null) || (payment.CreditCardDetails != null),
				new ErrorCode("CreditCard.CreditCardAuth", "Request must include either CreditCardDetails or CreditCardID"));

			var cc = await GetMeCardDetails(payment, userToken);

			Require.That(payment.IsValidCvv(cc), new ErrorCode("CreditCardAuth.InvalidCvv", "CVV is required for Credit Card Payment"));
			Require.That(cc.Token != null, new ErrorCode("CreditCardAuth.InvalidToken", "Credit card must have valid authorization token"));
			Require.That(cc.xp.CCBillingAddress != null, new ErrorCode("Invalid Bill Address", "Credit card must have a billing address"));

            var orderWorksheet = await oc.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.Incoming, payment.OrderID);
			var order = orderWorksheet.Order;

			Require.That(!order.IsSubmitted, new ErrorCode("CreditCardAuth.AlreadySubmitted", "Order has already been submitted"));

			var ccAmount = orderWorksheet.Order.Total;

            var ocPaymentsList = await oc.Payments.ListAsync<HSPayment>(OrderDirection.Incoming, payment.OrderID, filters: "Type=CreditCard");
			var ocPayments = ocPaymentsList.Items;
			var ocPayment = ocPayments.Any() ? ocPayments[0] : null;
			if (ocPayment == null)
			{
				throw new CatalystBaseException("Payment.MissingCreditCardPayment", "Order is missing credit card payment");
			}
			try
			{
				if (ocPayment?.Accepted == true)
				{
					if (ocPayment.Amount == ccAmount)
					{
						return ocPayment;
					}
                    else
                    {
					await VoidTransactionAsync(ocPayment, order, userToken);
				}
                }

                var call = await cardConnect.AuthWithoutCapture(CardConnectMapper.Map(cc, order, payment, merchantID, ccAmount));
                ocPayment = await oc.Payments.PatchAsync<HSPayment>(OrderDirection.Incoming, order.ID, ocPayment.ID, new PartialPayment { Accepted = true, Amount = ccAmount });
                return await oc.Payments.CreateTransactionAsync(OrderDirection.Incoming, order.ID, ocPayment.ID, CardConnectMapper.Map(ocPayment, call));
			}
			catch (CreditCardAuthorizationException ex)
			{
                ocPayment = await oc.Payments.PatchAsync<HSPayment>(OrderDirection.Incoming, order.ID, ocPayment.ID, new PartialPayment { Accepted = false, Amount = ccAmount });
                await oc.Payments.CreateTransactionAsync(OrderDirection.Incoming, order.ID, ocPayment.ID, CardConnectMapper.Map(ocPayment, ex.Response));
				throw new CatalystBaseException($"CreditCardAuth.{ex.ApiError.ErrorCode}", ex.ApiError.Message, ex.Response);
			}
		}

		/// <summary>
		/// Public re-usable VoidPaymentAsync task method
		/// </summary>
		/// <param name="orderID"></param>
		/// <param name="userToken"></param>
		/// <returns></returns>
		public async Task VoidPaymentAsync(string orderID, string userToken)
		{
            var order = await oc.Orders.GetAsync<HSOrder>(OrderDirection.Incoming, orderID);
            var paymentList = await oc.Payments.ListAsync<HSPayment>(OrderDirection.Incoming, order.ID);
			var payment = paymentList.Items.Any() ? paymentList.Items[0] : null;
            if (payment == null)
            {
                return;
            }

			await VoidTransactionAsync(payment, order, userToken);
            await oc.Payments.PatchAsync(OrderDirection.Incoming, orderID, payment.ID, new PartialPayment { Accepted = false });
		}

		/// <summary>
		/// Public re-usable VoidTransactionAsync task method
		/// </summary>
		/// <param name="payment"></param>
		/// <param name="order"></param>
		/// <param name="userToken"></param>
		/// <returns></returns>
		/// <exception cref="CatalystBaseException"></exception>
		public async Task VoidTransactionAsync(HSPayment payment, HSOrder order, string userToken)
		{
            var transactionID = string.Empty;
			try
			{
				if (payment.Accepted == true)
				{
					var transaction = payment.Transactions
						.Where(x => x.Type == "CreditCard")
						.OrderBy(x => x.DateExecuted)
						.LastOrDefault(t => t.Succeeded);
					var retref = transaction?.xp?.CardConnectResponse?.retref;
					if (retref != null)
					{
						transactionID = transaction.ID;
                        var userCurrency = await hsExchangeRates.GetCurrencyForUser(userToken);
                        var response = await cardConnect.VoidAuthorization(new CardConnectVoidRequest
						{
							currency = userCurrency.ToString(),
							merchid = GetMerchantID(userCurrency),
                            retref = transaction.xp.CardConnectResponse.retref,
						});
                        await oc.Payments.CreateTransactionAsync(OrderDirection.Incoming, order.ID, payment.ID, CardConnectMapper.Map(payment, response));
					}
				}
			}
			catch (CreditCardVoidException ex)
			{
                await supportAlerts.VoidAuthorizationFailed(payment, transactionID, order, ex);
                await oc.Payments.CreateTransactionAsync(OrderDirection.Incoming, order.ID, payment.ID, CardConnectMapper.Map(payment, ex.Response));
				throw new CatalystBaseException("Payment.FailedToVoidAuthorization", ex.ApiError.Message);
			}
		}

		/// <summary>
		/// Private re-usable GetMerchantID task method
		/// </summary>
		/// <param name="userCurrency"></param>
		/// <returns>The MerchantID string value</returns>
		private string GetMerchantID(CurrencySymbol userCurrency)
		{
			if (userCurrency == CurrencySymbol.USD)
			{
                return settings.CardConnectSettings.UsdMerchantID;
			}
			else if (userCurrency == CurrencySymbol.CAD)
			{
                return settings.CardConnectSettings.CadMerchantID;
			}
			else
			{
                return settings.CardConnectSettings.EurMerchantID;
			}
		}

		/// <summary>
		/// Private re-usable GetMeCardDetails task method
		/// </summary>
		/// <param name="payment"></param>
		/// <param name="userToken"></param>
		/// <returns>The CardConnectBuyerCreditCard response object from the GetMeCardDetails process</returns>
		private async Task<CardConnectBuyerCreditCard> GetMeCardDetails(OrderCloudIntegrationsCreditCardPayment payment, string userToken)
		{
			if (payment.CreditCardID != null)
			{
                return await oc.Me.GetCreditCardAsync<CardConnectBuyerCreditCard>(payment.CreditCardID, userToken);
			}
			return await MeTokenize(payment.CreditCardDetails, userToken);
		}

		/// <summary>
		/// Private re-usable MeTokenize task method
		/// </summary>
		/// <param name="card"></param>
		/// <param name="userToken"></param>
		/// <returns>The CardConnectBuyerCreditCard response object from the MeTokenize process</returns>
		private async Task<CardConnectBuyerCreditCard> MeTokenize(OrderCloudIntegrationsCreditCardToken card, string userToken)
		{
            var userCurrency = await hsExchangeRates.GetCurrencyForUser(userToken);
            var auth = await cardConnect.Tokenize(CardConnectMapper.Map(card, userCurrency.ToString()));
			return BuyerCreditCardMapper.Map(card, auth);
		}

		/// <summary>
		/// Private re-usable Tokenize task method
		/// </summary>
		/// <param name="card"></param>
		/// <param name="userToken"></param>
		/// <returns>The CreditCard response object from the Tokenize process</returns>
		private async Task<CreditCard> Tokenize(OrderCloudIntegrationsCreditCardToken card, string userToken)
		{
            var userCurrency = await hsExchangeRates.GetCurrencyForUser(userToken);
            var auth = await cardConnect.Tokenize(CardConnectMapper.Map(card, userCurrency.ToString()));
			return CreditCardMapper.Map(card, auth);
		}
	}
}
