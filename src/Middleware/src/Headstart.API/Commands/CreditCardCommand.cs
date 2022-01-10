using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Headstart.Common;
using Headstart.Common.Services;
using Headstart.Common.Services.ShippingIntegration.Models;
using Headstart.Models;
using Headstart.Models.Headstart;
using ordercloud.integrations.exchangerates;
using ordercloud.integrations.library;
using OrderCloud.Catalyst;
using OrderCloud.SDK;

namespace ordercloud.integrations.cardconnect
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
		private readonly IOrderCloudIntegrationsCardConnectService _cardConnect;
		private readonly IOrderCloudClient _oc;
		private readonly IHSExchangeRatesService _hsExchangeRates;
		private readonly ISupportAlertService _supportAlerts;
		private readonly AppSettings _settings;

		public CreditCardCommand(
			IOrderCloudIntegrationsCardConnectService card,
			IOrderCloudClient oc,
			IHSExchangeRatesService hsExchangeRates,
			ISupportAlertService supportAlerts,
			AppSettings settings
		)
		{
			_cardConnect = card;
			_oc = oc;
			_hsExchangeRates = hsExchangeRates;
			_supportAlerts = supportAlerts;
			_settings = settings;
		}

		public async Task<CreditCard> TokenizeAndSave(string buyerID, OrderCloudIntegrationsCreditCardToken card, DecodedToken decodedToken)
		{
			var creditCard = await _oc.CreditCards.CreateAsync(buyerID, await Tokenize(card, decodedToken.AccessToken), decodedToken.AccessToken);
			return creditCard;
		}

		public async Task<BuyerCreditCard> MeTokenizeAndSave(OrderCloudIntegrationsCreditCardToken card, DecodedToken decodedToken)
		{
			var buyerCreditCard = await _oc.Me.CreateCreditCardAsync(await MeTokenize(card, decodedToken.AccessToken), decodedToken.AccessToken);
			return buyerCreditCard;
		}

		public async Task<Payment> AuthorizePayment(
			OrderCloudIntegrationsCreditCardPayment payment,
            string userToken,
			string merchantID
		)
		{
			Require.That((payment.CreditCardID != null) || (payment.CreditCardDetails != null),
				new ErrorCode("CreditCard.CreditCardAuth", "Request must include either CreditCardDetails or CreditCardID"));

			var cc = await GetMeCardDetails(payment, userToken);

			Require.That(payment.IsValidCvv(cc), new ErrorCode("CreditCardAuth.InvalidCvv", "CVV is required for Credit Card Payment"));
			Require.That(cc.Token != null, new ErrorCode("CreditCardAuth.InvalidToken", "Credit card must have valid authorization token"));
			Require.That(cc.xp.CCBillingAddress != null, new ErrorCode("Invalid Bill Address", "Credit card must have a billing address"));

			var orderWorksheet = await _oc.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.Incoming, payment.OrderID);
			var order = orderWorksheet.Order;

			Require.That(!order.IsSubmitted, new ErrorCode("CreditCardAuth.AlreadySubmitted", "Order has already been submitted"));

			var ccAmount = orderWorksheet.Order.Total;

			var ocPaymentsList = (await _oc.Payments.ListAsync<HSPayment>(OrderDirection.Incoming, payment.OrderID, filters: "Type=CreditCard" ));
			var ocPayments = ocPaymentsList.Items;
			var ocPayment = ocPayments.Any() ? ocPayments[0] : null;
			if(ocPayment == null)
            {
				throw new CatalystBaseException("Payment.MissingCreditCardPayment", "Order is missing credit card payment");
            }
            try
            {
				if(ocPayment?.Accepted == true)
                {
					if(ocPayment.Amount == ccAmount)
                    {
						return ocPayment;
                    } else
                    {
						await VoidTransactionAsync(ocPayment, order, userToken);
                    }
                }
                var call = await _cardConnect.AuthWithoutCapture(CardConnectMapper.Map(cc, order, payment, merchantID, ccAmount));
				ocPayment = await _oc.Payments.PatchAsync<HSPayment>(OrderDirection.Incoming, order.ID, ocPayment.ID, new PartialPayment { Accepted = true, Amount = ccAmount });
				return await _oc.Payments.CreateTransactionAsync(OrderDirection.Incoming, order.ID, ocPayment.ID, CardConnectMapper.Map(ocPayment, call));
			}
            catch (CreditCardAuthorizationException ex)
            {
                ocPayment = await _oc.Payments.PatchAsync<HSPayment>(OrderDirection.Incoming, order.ID, ocPayment.ID, new PartialPayment { Accepted = false, Amount = ccAmount });
				await _oc.Payments.CreateTransactionAsync(OrderDirection.Incoming, order.ID, ocPayment.ID, CardConnectMapper.Map(ocPayment, ex.Response));
				throw new CatalystBaseException($"CreditCardAuth.{ex.ApiError.ErrorCode}", ex.ApiError.Message, ex.Response);
			}
		}

		public async Task VoidPaymentAsync(string orderID, string userToken)
        {
			var order = await _oc.Orders.GetAsync<HSOrder>(OrderDirection.Incoming, orderID);
			var paymentList = await _oc.Payments.ListAsync<HSPayment>(OrderDirection.Incoming, order.ID);
			var payment = paymentList.Items.Any() ? paymentList.Items[0] : null;
			if(payment == null) { return; }

			await VoidTransactionAsync(payment, order, userToken);
			await _oc.Payments.PatchAsync(OrderDirection.Incoming, orderID, payment.ID, new PartialPayment { Accepted = false });
        }

		public async Task VoidTransactionAsync(HSPayment payment, HSOrder order, string userToken)
        {
			var transactionID = "";
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
				throw new CatalystBaseException("Payment.FailedToVoidAuthorization", ex.ApiError.Message);
			}
		}

		private string GetMerchantID(CurrencySymbol userCurrency)
		{
			if (userCurrency == CurrencySymbol.USD)
				return _settings.CardConnectSettings.UsdMerchantID;
			else if (userCurrency == CurrencySymbol.CAD)
				return _settings.CardConnectSettings.CadMerchantID;
			else
				return _settings.CardConnectSettings.EurMerchantID;
		}

		private async Task<CardConnectBuyerCreditCard> GetMeCardDetails(OrderCloudIntegrationsCreditCardPayment payment, string userToken)
		{
			if (payment.CreditCardID != null)
			{
				return await _oc.Me.GetCreditCardAsync<CardConnectBuyerCreditCard>(payment.CreditCardID, userToken);
			}
			return await MeTokenize(payment.CreditCardDetails, userToken);
		}

		private async Task<CardConnectBuyerCreditCard> MeTokenize(OrderCloudIntegrationsCreditCardToken card, string userToken)
		{
			var userCurrency = await _hsExchangeRates.GetCurrencyForUser(userToken);
			var auth = await _cardConnect.Tokenize(CardConnectMapper.Map(card, userCurrency.ToString()));
			return BuyerCreditCardMapper.Map(card, auth);
		}

		private async Task<CreditCard> Tokenize(OrderCloudIntegrationsCreditCardToken card, string userToken)
		{
			var userCurrency = await _hsExchangeRates.GetCurrencyForUser(userToken);
			var auth = await _cardConnect.Tokenize(CardConnectMapper.Map(card, userCurrency.ToString()));
			return CreditCardMapper.Map(card, auth);
		}
	}
}
