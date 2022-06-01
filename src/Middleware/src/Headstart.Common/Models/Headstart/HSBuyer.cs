using OrderCloud.SDK;

namespace Headstart.Models
{
    public class SuperHSBuyer
    {
        public HSBuyer Buyer { get; set; }

        public ImpersonationConfig ImpersonationConfig { get; set; }
    }

    public class HSBuyer : Buyer<BuyerXp>, IHSObject
    {
    }

    public class BuyerXp
    {
        public int MarkupPercent { get; set; }
    }
}
