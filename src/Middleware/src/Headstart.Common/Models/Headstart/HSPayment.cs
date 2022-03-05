using OrderCloud.SDK;
using ordercloud.integrations.cardconnect;

namespace Headstart.Models.Headstart
{

    public class HSPayment : Payment<PaymentXP, HSPaymentTransaction> { }


    public class HSPaymentTransaction : PaymentTransaction<TransactionXP> { }

    public class PaymentXP
    {
        public string partialAccountNumber { get; set; } = string.Empty;

        public string cardType { get; set; } = string.Empty;
    }


    public class TransactionXP
    {
        public CardConnectAuthorizationResponse CardConnectResponse { get; set; } = new CardConnectAuthorizationResponse();

        public RMADetails RMADetails { get; set; } = new RMADetails();
    }


    public class RMADetails
    {
        public string OrderRMANumber { get; set; } = string.Empty;

        public string RefundComment { get; set; } = string.Empty;

        public string FromSupplierID { get; set; } = string.Empty;

        public string FromUserID { get; set; } = string.Empty;
    }
}