using System.Threading.Tasks;
using Headstart.Common.Models;
using OrderCloud.Catalyst;
using OrderCloud.SDK;

namespace Headstart.Common.Commands
{
    public interface ICreditCardCommand
    {
        Task<BuyerCreditCard> MeTokenizeAndSave(CCToken card, DecodedToken decodedToken);

        Task<CreditCard> TokenizeAndSave(string buyerID, CCToken card, DecodedToken decodedToken);

        Task<Payment> AuthorizePayment(CCPayment payment, string userToken);

        Task VoidTransactionAsync(HSPayment payment, HSOrder order, string userToken);

        Task VoidPaymentAsync(string orderID, string userToken);
    }
}
