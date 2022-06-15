using OrderCloud.SDK;

namespace Headstart.Common.Models
{
    public class HSCreditCard : CreditCard<HSCreditCardXP>, IHSCreditCard
    {
    }

    public class HSBuyerCreditCard : BuyerCreditCard<HSCreditCardXP>, IHSCreditCard
    {
    }

    public class HSCreditCardXP
    {
        [Required]
        public Address CCBillingAddress { get; set; }
    }
}
