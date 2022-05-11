﻿using OrderCloud.SDK;
using Headstart.Common;
using OrderCloud.Catalyst;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ordercloud.integrations.cardconnect;

namespace Headstart.API.Controllers
{
	public class MePaymentController : CatalystController
	{
		private readonly ICreditCardCommand _card;
		private readonly AppSettings _settings;

		/// <summary>
		/// The IOC based constructor method for the MePaymentController class object with Dependency Injection
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="card"></param>
		public MePaymentController(AppSettings settings, ICreditCardCommand card) 
		{
			_card = card;
			_settings = settings;
		}

		/// <summary>
		/// Posts the Payment orders  (POST method)
		/// </summary>
		/// <param name="payment"></param>
		/// <returns>The order payment response</returns>
		[HttpPost, Route("me/payments"), OrderCloudUserAuth(ApiRole.Shopper)]
		public async Task<Payment> Post([FromBody] OrderCloudIntegrationsCreditCardPayment payment)
		{
			var merchantId = string.Empty;
			if (payment.Currency == "USD")
			{
				merchantId = _settings.CardConnectSettings.UsdMerchantID;				
			}
			else
			{
				merchantId = payment.Currency == "CAD" ? _settings.CardConnectSettings.CadMerchantID : _settings.CardConnectSettings.EurMerchantID;
			}
			return await _card.AuthorizePayment(payment, UserContext.AccessToken, merchantId);
		}
	}

	public class MeCreditCardAuthorizationController : CatalystController
	{
		private readonly ICreditCardCommand _card;

		/// <summary>
		/// The IOC based constructor method for the MeCreditCardAuthorizationController class object with Dependency Injection
		/// </summary>
		/// <param name="card"></param>
		public MeCreditCardAuthorizationController(ICreditCardCommand card)
		{
			_card = card;
		}

		/// <summary>
		/// Creates a Buyer Credit Card (POST method)
		/// </summary>
		/// <param name="card"></param>
		/// <returns>The response for the newly created credit card object</returns>
		[HttpPost, Route("me/creditcards"), OrderCloudUserAuth(ApiRole.MeCreditCardAdmin, ApiRole.CreditCardAdmin)]
		public async Task<BuyerCreditCard> MePost([FromBody] OrderCloudIntegrationsCreditCardToken card)
		{
			return await _card.MeTokenizeAndSave(card, UserContext);
		}
	}


	public class CreditCardController : CatalystController
	{
		private readonly ICreditCardCommand _card;

		/// <summary>
		/// The IOC based constructor method for the CreditCardController class object with Dependency Injection
		/// </summary>
		/// <param name="card"></param>
		public CreditCardController(ICreditCardCommand card)
		{
			_card = card;
		}

		/// <summary>
		/// Posts the Buyer Credit Card data, by buyerID (POST method)
		/// </summary>
		/// <param name="card"></param>
		/// <param name="buyerId"></param>
		/// <returns>The response for the newly created credit card object for the specified buyerID</returns>
		[HttpPost, Route("buyers/{buyerId}/creditcards"), OrderCloudUserAuth(ApiRole.CreditCardAdmin)]
		public async Task<CreditCard> Post([FromBody] OrderCloudIntegrationsCreditCardToken card, string buyerId)
		{
			return await _card.TokenizeAndSave(buyerId, card, UserContext);
		}
	}
}