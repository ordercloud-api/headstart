using System.Threading.Tasks;
using Headstart.Common.Models;

namespace Headstart.Common.Services
{
    public interface IHSCreditCardProcessor
    {
        Task<HSCreditCard> Tokenize(CCToken card, CurrencyCode userCurrency);

        Task<HSPayment> AuthWithoutCapture(HSPayment payment, HSBuyerCreditCard buyerCreditCard, HSOrder order, CCPayment ccPayment, string userToken, decimal ccAmount);

        Task VoidAuthorization(HSOrder order, HSPayment payment, HSPaymentTransaction paymentTransaction, decimal? refundAmount = null, string orderReturnId = null);

        Task Capture(HSOrder order, HSPayment payment, HSPaymentTransaction transaction);

        Task<CCInquiryResult> Inquire(HSOrder order, HSPaymentTransaction creditCardPaymentTransaction);

        Task Refund(HSOrder worksheet, HSPayment payment, HSPaymentTransaction paymentTransaction, decimal refundAmount, string orderReturnId = "");

        Task<HSBuyerCreditCard> MeTokenize(CCToken card, CurrencyCode userCurrency);
    }
}
