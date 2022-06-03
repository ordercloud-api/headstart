using Headstart.Common.Models;
using OrderCloud.SDK;

namespace Headstart.Models
{
    public class HSAddressBuyer : Address<BuyerAddressXP>, IHSObject, IHSAddress
    {
    }

    public class HSAddressMeBuyer : BuyerAddress<BuyerAddressXP>, IHSObject, IHSAddress
    {
    }

    public class BuyerAddressXP
    {
        public string Email { get; set; }

        public string LocationID { get; set; }

        public string BillingNumber { get; set; }
    }
}
