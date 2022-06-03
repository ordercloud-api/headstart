using OrderCloud.SDK;

namespace Headstart.Common.Models
{
    public class SuperHSBuyer
    {
        public HSBuyer Buyer { get; set; }

        public ImpersonationConfig ImpersonationConfig { get; set; }
    }

    public class HSBuyer : Buyer<BuyerXp>
    {
    }

    public class BuyerXp
    {
        public int MarkupPercent { get; set; }

        public string URL { get; set; }
    }
}
