using System.Threading.Tasks;
using Headstart.API.Commands;
using Microsoft.AspNetCore.Mvc;
using OrderCloud.Catalyst;
using OrderCloud.Integrations.CardConnect.Models;
using OrderCloud.SDK;

namespace Headstart.API.Controllers
{
    /// <summary>
    /// ME Credit Card Payments for Headstart.
    /// </summary>
    public class MePaymentController : CatalystController
    {
        private readonly ICreditCardCommand card;
        private readonly AppSettings settings;

        public MePaymentController(AppSettings settings, ICreditCardCommand card)
        {
            this.card = card;
            this.settings = settings;
        }

        /// <summary>
        /// POST Payment.
        /// </summary>
        [HttpPost, Route("me/payments"), OrderCloudUserAuth(ApiRole.Shopper)]
        public async Task<Payment> Post([FromBody] OrderCloudIntegrationsCreditCardPayment payment)
        {
            string merchantID;
            if (payment.Currency == "USD")
            {
                merchantID = settings.CardConnectSettings.UsdMerchantID;
            }
            else if (payment.Currency == "CAD")
            {
                merchantID = settings.CardConnectSettings.CadMerchantID;
            }
            else
            {
                merchantID = settings.CardConnectSettings.EurMerchantID;
            }

            return await card.AuthorizePayment(payment, UserContext.AccessToken, merchantID);
        }
    }

    /// <summary>
    ///  ME Credit Card Tokenization for Headstart.
    /// </summary>
    public class MeCreditCardAuthorizationController : CatalystController
    {
        private readonly ICreditCardCommand card;

        public MeCreditCardAuthorizationController(ICreditCardCommand card)
        {
            this.card = card;
        }

        /// <summary>
        /// POST Credit Card.
        /// </summary>
        [HttpPost, Route("me/creditcards"), OrderCloudUserAuth(ApiRole.MeCreditCardAdmin, ApiRole.CreditCardAdmin)]
        public async Task<BuyerCreditCard> MePost([FromBody] OrderCloudIntegrationsCreditCardToken card)
        {
            return await this.card.MeTokenizeAndSave(card, UserContext);
        }
    }

    /// <summary>
    /// Credit Card Tokenization for Headstart.
    /// </summary>
    public class CreditCardController : CatalystController
    {
        private readonly ICreditCardCommand card;

        public CreditCardController(ICreditCardCommand card)
        {
            this.card = card;
        }

        /// <summary>
        /// POST Credit Cards.
        /// </summary>
        [HttpPost, Route("buyers/{buyerID}/creditcards"), OrderCloudUserAuth(ApiRole.CreditCardAdmin)]
        public async Task<CreditCard> Post([FromBody] OrderCloudIntegrationsCreditCardToken card, string buyerID)
        {
            return await this.card.TokenizeAndSave(buyerID, card, UserContext);
        }
    }
}
