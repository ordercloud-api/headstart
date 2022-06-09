using System.Threading.Tasks;
using Headstart.Common.Commands;
using Headstart.Common.Models;
using Microsoft.AspNetCore.Mvc;
using OrderCloud.Catalyst;
using OrderCloud.SDK;

namespace Headstart.API.Controllers
{
    /// <summary>
    /// ME Credit Card Payments for Headstart.
    /// </summary>
    public class MePaymentController : CatalystController
    {
        private readonly ICreditCardCommand creditCardCommand;

        public MePaymentController(ICreditCardCommand creditCardCommand)
        {
            this.creditCardCommand = creditCardCommand;
        }

        /// <summary>
        /// POST Payment.
        /// </summary>
        [HttpPost, Route("me/payments"), OrderCloudUserAuth(ApiRole.Shopper)]
        public async Task<Payment> Post([FromBody] CCPayment payment)
        {
            return await creditCardCommand.AuthorizePayment(payment, UserContext.AccessToken);
        }
    }

    /// <summary>
    ///  ME Credit Card Tokenization for Headstart.
    /// </summary>
    public class MeCreditCardAuthorizationController : CatalystController
    {
        private readonly ICreditCardCommand creditCardCommand;

        public MeCreditCardAuthorizationController(ICreditCardCommand creditCardCommand)
        {
            this.creditCardCommand = creditCardCommand;
        }

        /// <summary>
        /// POST Credit Card.
        /// </summary>
        [HttpPost, Route("me/creditcards"), OrderCloudUserAuth(ApiRole.MeCreditCardAdmin, ApiRole.CreditCardAdmin)]
        public async Task<BuyerCreditCard> MePost([FromBody] CCToken card)
        {
            return await this.creditCardCommand.MeTokenizeAndSave(card, UserContext);
        }
    }

    /// <summary>
    /// Credit Card Tokenization for Headstart.
    /// </summary>
    public class CreditCardController : CatalystController
    {
        private readonly ICreditCardCommand creditCardCommand;

        public CreditCardController(ICreditCardCommand creditCardCommand)
        {
            this.creditCardCommand = creditCardCommand;
        }

        /// <summary>
        /// POST Credit Cards.
        /// </summary>
        [HttpPost, Route("buyers/{buyerID}/creditcards"), OrderCloudUserAuth(ApiRole.CreditCardAdmin)]
        public async Task<CreditCard> Post([FromBody] CCToken card, string buyerID)
        {
            return await this.creditCardCommand.TokenizeAndSave(buyerID, card, UserContext);
        }
    }
}
