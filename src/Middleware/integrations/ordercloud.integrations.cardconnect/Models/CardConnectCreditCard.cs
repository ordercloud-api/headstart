using OrderCloud.SDK;

namespace ordercloud.integrations.cardconnect
{
    public class CardConnectCreditCard : CreditCard<CreditCardXP>
    {

    }

    public class CardConnectBuyerCreditCard : BuyerCreditCard<CreditCardXP>
    {

    }

    public class CreditCardXP
    {
        [Required]
        public Address CCBillingAddress { get; set; }
    }
}
