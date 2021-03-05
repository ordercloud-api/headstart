using System.Threading.Tasks;
using Headstart.Models.Attributes;
using Microsoft.AspNetCore.Mvc;
using ordercloud.integrations.cardconnect;
using ordercloud.integrations.library;
using OrderCloud.Catalyst;
using OrderCloud.SDK;

namespace Headstart.Common.Controllers.CardConnect
{
    [DocComments("\"Integration\" represents ME Credit Card Payments for Headstart")]
    [HSSection.Integration(ListOrder = 2)]
    public class MePaymentController : BaseController
    {
        private readonly ICreditCardCommand _card;
        private readonly AppSettings _settings;
        public MePaymentController(AppSettings settings, ICreditCardCommand card) 
        {
            _card = card;
            _settings = settings;
        }

        [DocName("POST Payment")]
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

    [DocComments("\"Integration\" represents ME Credit Card Tokenization for Headstart")]
    [HSSection.Integration(ListOrder = 3)]
    public class MeCreditCardAuthorizationController : BaseController
    {
        private readonly ICreditCardCommand _card;
        public MeCreditCardAuthorizationController(ICreditCardCommand card)
        {
            _card = card;
        }

        [DocName("POST Credit Card")]
        [HttpPost, Route("me/creditcards"), OrderCloudUserAuth(ApiRole.MeCreditCardAdmin, ApiRole.CreditCardAdmin)]
        public async Task<BuyerCreditCard> MePost([FromBody] OrderCloudIntegrationsCreditCardToken card)
        {
            return await _card.MeTokenizeAndSave(card, UserContext);
        }
    }

    [DocComments("\"Integration\" represents Credit Card Tokenization for Headstart")]
    [HSSection.Integration(ListOrder = 4)]
    public class CreditCardController : BaseController
    {
        private readonly ICreditCardCommand _card;
        public CreditCardController(ICreditCardCommand card)
        {
            _card = card;
        }

        [DocName("POST Credit Cards")]
        [HttpPost, Route("buyers/{buyerID}/creditcards"), OrderCloudUserAuth(ApiRole.CreditCardAdmin)]
        public async Task<CreditCard> Post([FromBody] OrderCloudIntegrationsCreditCardToken card, string buyerID)
        {
            return await _card.TokenizeAndSave(buyerID, card, UserContext);
        }
    }
}
