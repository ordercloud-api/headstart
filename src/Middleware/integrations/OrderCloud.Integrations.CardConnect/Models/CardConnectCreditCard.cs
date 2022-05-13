using OrderCloud.SDK;

namespace OrderCloud.Integrations.CardConnect.Models
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
