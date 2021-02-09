using ordercloud.integrations.cardconnect;
using ordercloud.integrations.library;
using OrderCloud.SDK;
using System;
using System.Collections.Generic;
using System.Text;

namespace Headstart.Models.Headstart
{
    [SwaggerModel]
    public class HSPayment : Payment<PaymentXP, HSPaymentTransaction>
    {

    }

    [SwaggerModel]
    public class HSPaymentTransaction: PaymentTransaction<TransactionXP>
    {

    }

    [SwaggerModel]
    public class PaymentXP
    {
        public string partialAccountNumber { get; set; }
        public string cardType { get; set; }
    }

    [SwaggerModel]
    public class TransactionXP
    {
        public CardConnectAuthorizationResponse CardConnectResponse { get; set; }
        public RMADetails RMADetails { get; set; }
    }

    [SwaggerModel]
    public class RMADetails
    {
        public string OrderRMANumber { get; set; }
        public string RefundComment { get; set; }
        public string FromSupplierID { get; set; }
        public string FromUserID { get; set; }
    }
}
