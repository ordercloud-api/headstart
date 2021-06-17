using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrderCloud.SDK;
using ordercloud.integrations.cardconnect;
using Headstart.Common.Services;
using Headstart.Common.Services.ShippingIntegration.Models;
using Headstart.Models;
using Headstart.Models.Headstart;

namespace Headstart.API.Commands
{

    public interface IPaymentCommand
    {
        Task<IList<HSPayment>> SavePayments(string orderID, List<HSPayment> requestedPayments, string userToken);
    }

    public class PaymentCommand : IPaymentCommand
    {
        private readonly IOrderCloudClient _oc;
        private readonly ICreditCardCommand _ccCommand;
        public PaymentCommand(
            IOrderCloudClient oc,
            ICreditCardCommand ccCommand
        )
        {
            _oc = oc;
            _ccCommand = ccCommand;
        }

        public async Task<IList<HSPayment>> SavePayments(string orderID, List<HSPayment> requestedPayments, string userToken)
        {
            var worksheet = await _oc.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.Incoming, orderID);
            var existingPayments = (await _oc.Payments.ListAsync<HSPayment>(OrderDirection.Incoming, orderID)).Items;
            existingPayments =  await DeleteStalePaymentsAsync(requestedPayments, existingPayments, worksheet.Order, userToken);

            foreach(var requestedPayment in requestedPayments)
            {
                var existingPayment = existingPayments.FirstOrDefault(p => p.Type == requestedPayment.Type);
                if(requestedPayment.Type == PaymentType.CreditCard) { await UpdateCCPaymentAsync(requestedPayment, existingPayment, worksheet, userToken); }
                if(requestedPayment.Type == PaymentType.PurchaseOrder) { await UpdatePoPaymentAsync(requestedPayment, existingPayment, worksheet); }
            }

            return (await _oc.Payments.ListAsync<HSPayment>(OrderDirection.Incoming, orderID)).Items;
        }

        private async Task UpdateCCPaymentAsync(HSPayment requestedPayment, HSPayment existingPayment, HSOrderWorksheet worksheet, string userToken)
        {
            var paymentAmount = worksheet.Order.Total;
            if (existingPayment == null)
            {
                requestedPayment.Amount = paymentAmount;
                requestedPayment.Accepted = false;
                requestedPayment.Type = requestedPayment.Type;
                await _oc.Payments.CreateAsync<HSPayment>(OrderDirection.Outgoing, worksheet.Order.ID, requestedPayment, userToken); // need user token because admins cant see personal credit cards
            }
            else if(existingPayment.CreditCardID == requestedPayment.CreditCardID && existingPayment.Amount == paymentAmount)
            {
                // do nothing, payment doesnt need updating
                return;
            }
            else if (existingPayment.CreditCardID == requestedPayment.CreditCardID)
            {
                await _ccCommand.VoidTransactionAsync(existingPayment, worksheet.Order, userToken);
                await _oc.Payments.PatchAsync<HSPayment>(OrderDirection.Incoming, worksheet.Order.ID, existingPayment.ID, new PartialPayment
                {
                    Accepted = false,
                    Amount = paymentAmount,
                    xp = requestedPayment.xp
                });
            }
            else
            {
                // we need to delete payment because you can't have payments totaling more than order total and you can't set payments to $0
                await DeleteCreditCardPaymentAsync(existingPayment, worksheet.Order, userToken);
                requestedPayment.Amount = paymentAmount;
                requestedPayment.Accepted = false;
                await _oc.Payments.CreateAsync<HSPayment>(OrderDirection.Outgoing, worksheet.Order.ID, requestedPayment, userToken); // need user token because admins cant see personal credit cards
            }
        }

        private async Task UpdatePoPaymentAsync(HSPayment requestedPayment, HSPayment existingPayment, HSOrderWorksheet worksheet)
        {
            var paymentAmount = worksheet.Order.Total;
            if (existingPayment == null)
            {
                requestedPayment.Amount = paymentAmount;
                await _oc.Payments.CreateAsync<HSPayment>(OrderDirection.Incoming, worksheet.Order.ID, requestedPayment);
            } 
            else
            {
                await _oc.Payments.PatchAsync<HSPayment>(OrderDirection.Incoming, worksheet.Order.ID, existingPayment.ID, new PartialPayment
                {
                    Amount = paymentAmount
                });
            }
        }

        private async Task DeleteCreditCardPaymentAsync(HSPayment payment, HSOrder order, string userToken)
        {
            await _ccCommand.VoidTransactionAsync(payment, order, userToken);
            await _oc.Payments.DeleteAsync(OrderDirection.Incoming, order.ID, payment.ID);
        }


        private async Task<IList<HSPayment>> DeleteStalePaymentsAsync(IList<HSPayment> requestedPayments, IList<HSPayment> existingPayments, HSOrder order, string userToken)
        {
            // requestedPayments represents the payments that should be on the order
            // if there are any existing payments not reflected in requestedPayments then they should be deleted
            foreach (var existingPayment in existingPayments.ToList())
            {
                if (!requestedPayments.Any(p => p.Type == existingPayment.Type))
                {
                    if (existingPayment.Type == PaymentType.PurchaseOrder)
                    {
                        await _oc.Payments.DeleteAsync(OrderDirection.Incoming, order.ID, existingPayment.ID);
                        existingPayments.Remove(existingPayment);
                    }
                    if (existingPayment.Type == PaymentType.CreditCard)
                    {
                        await DeleteCreditCardPaymentAsync(existingPayment, order, userToken);
                        existingPayments.Remove(existingPayment);
                    }
                    else
                    {
                        await _oc.Payments.DeleteAsync(OrderDirection.Incoming, order.ID, existingPayment.ID);
                        existingPayments.Remove(existingPayment);
                    }
                }
            }
            return existingPayments;

        }
    };
}