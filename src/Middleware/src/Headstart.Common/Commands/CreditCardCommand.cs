using System.Linq;
using System.Threading.Tasks;
using Headstart.Common.Extensions;
using Headstart.Common.Models;
using Headstart.Common.Services;
using OrderCloud.Catalyst;
using OrderCloud.SDK;

namespace Headstart.Common.Commands
{
    public class CreditCardCommand : ICreditCardCommand
    {
        private readonly ICreditCardProcessor creditCardService;
        private readonly IOrderCloudClient oc;
        private readonly ICurrencyConversionCommand currencyConversionCommand;

        public CreditCardCommand(
            ICreditCardProcessor creditCardService,
            IOrderCloudClient oc,
            ICurrencyConversionCommand currencyConversionCommand)
        {
            this.creditCardService = creditCardService;
            this.oc = oc;
            this.currencyConversionCommand = currencyConversionCommand;
        }

        public async Task<CreditCard> TokenizeAndSave(string buyerID, CCToken card, DecodedToken decodedToken)
        {
            var creditCard = await oc.CreditCards.CreateAsync(buyerID, await Tokenize(card, decodedToken.AccessToken), decodedToken.AccessToken);
            return creditCard;
        }

        public async Task<BuyerCreditCard> MeTokenizeAndSave(CCToken card, DecodedToken decodedToken)
        {
            var buyerCreditCard = await oc.Me.CreateCreditCardAsync(await MeTokenize(card, decodedToken.AccessToken), decodedToken.AccessToken);
            return buyerCreditCard;
        }

        public async Task<Payment> AuthorizePayment(CCPayment payment, string userToken)
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

            return await creditCardService.AuthWithoutCapture(ocPayment, cc, order, payment, userToken, ccAmount);
        }

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

        public async Task VoidTransactionAsync(HSPayment payment, HSOrder order, string userToken)
        {
            var transactionID = string.Empty;
            if (payment.Accepted == true)
            {
                var transaction = payment.Transactions
                                    .Where(x => x.Type == "CreditCard")
                                    .OrderBy(x => x.DateExecuted)
                                    .LastOrDefault(t => t.Succeeded);
                var retref = transaction?.xp?.CCTransactionResult?.TransactionID;
                if (retref != null)
                {
                    transactionID = transaction.ID;
                    var userCurrency = await currencyConversionCommand.GetCurrencyForUser(userToken);
                    await creditCardService.VoidAuthorization(order, payment, transaction);
                }
            }
        }

        private async Task<HSBuyerCreditCard> GetMeCardDetails(CCPayment payment, string userToken)
        {
            if (payment.CreditCardID != null)
            {
                return await oc.Me.GetCreditCardAsync<HSBuyerCreditCard>(payment.CreditCardID, userToken);
            }

            return await MeTokenize(payment.CreditCardDetails, userToken);
        }

        private async Task<HSBuyerCreditCard> MeTokenize(CCToken card, string userToken)
        {
            var userCurrency = await currencyConversionCommand.GetCurrencyForUser(userToken);
            return await creditCardService.MeTokenize(card, userCurrency);
        }

        private async Task<CreditCard> Tokenize(CCToken card, string userToken)
        {
            var userCurrency = await currencyConversionCommand.GetCurrencyForUser(userToken);
            return await creditCardService.Tokenize(card, userCurrency);
        }
    }
}
