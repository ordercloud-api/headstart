using System.Threading.Tasks;
using Headstart.Models.Attributes;
using Microsoft.AspNetCore.Mvc;
using ordercloud.integrations.cardconnect;
using ordercloud.integrations.library;
using OrderCloud.Catalyst;
using OrderCloud.SDK;

namespace Headstart.Common.Controllers.CardConnect
{
    /// <summary>
    /// ME Credit Card Payments for Headstart
    /// </summary>
    public class MePaymentController : CatalystController
    {
        private readonly ICreditCardCommand _card;
        private readonly AppSettings _settings;
        public MePaymentController(AppSettings settings, ICreditCardCommand card) 
        {
            _card = card;
            _settings = settings;
        }
        /// <summary>
        /// POST Payment
        /// </summary>
        [HttpPost, Route("me/payments"), OrderCloudUserAuth(ApiRole.Shopper)]
        public async Task<Payment> Post([FromBody] OrderCloudIntegrationsCreditCardPayment payment)
        {
            string merchantID;
            if (payment.Currency == "USD")
                merchantID = _settings.CardConnectSettings.UsdMerchantID;
            else if (payment.Currency == "CAD")
                merchantID = _settings.CardConnectSettings.CadMerchantID;
            else
                merchantID = _settings.CardConnectSettings.EurMerchantID;
                
            return await _card.AuthorizePayment(payment, UserContext.AccessToken, merchantID);
        }
    }
    /// <summary>
    ///  ME Credit Card Tokenization for Headstart
    /// </summary>
    public class MeCreditCardAuthorizationController : CatalystController
    {
        private readonly ICreditCardCommand _card;
        public MeCreditCardAuthorizationController(ICreditCardCommand card)
        {
            _card = card;
        }
        /// <summary>
        /// POST Credit Card
        /// </summary>
        [HttpPost, Route("me/creditcards"), OrderCloudUserAuth(ApiRole.MeCreditCardAdmin, ApiRole.CreditCardAdmin)]
        public async Task<BuyerCreditCard> MePost([FromBody] OrderCloudIntegrationsCreditCardToken card)
        {
            return await _card.MeTokenizeAndSave(card, UserContext);
        }
    }
    /// <summary>
    /// Credit Card Tokenization for Headstart
    /// </summary>
    public class CreditCardController : CatalystController
    {
        private readonly ICreditCardCommand _card;
        public CreditCardController(ICreditCardCommand card)
        {
            _card = card;
        }
        /// <summary>
        /// POST Credit Cards
        /// </summary>
        [HttpPost, Route("buyers/{buyerID}/creditcards"), OrderCloudUserAuth(ApiRole.CreditCardAdmin)]
        public async Task<CreditCard> Post([FromBody] OrderCloudIntegrationsCreditCardToken card, string buyerID)
        {
            return await _card.TokenizeAndSave(buyerID, card, UserContext);
        }
    }
}
