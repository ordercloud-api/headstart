using OrderCloud.SDK;

namespace Headstart.Models
{
    public class HSAddressBuyer : Address<BuyerAddressXP>, IHSObject
    {
    }

    public class HSAddressMeBuyer : BuyerAddress<BuyerAddressXP>, IHSObject
    {
    }

    public class BuyerAddressXP
    {
        public string Email { get; set; }

        public string LocationID { get; set; }

        public string BillingNumber { get; set; }
    }
}
