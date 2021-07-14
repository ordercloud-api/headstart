using ordercloud.integrations.library;
using OrderCloud.SDK;

namespace Headstart.Models
{
    public class SuperHSBuyer
    {
        public HSBuyer Buyer { get; set; }
        public BuyerMarkup Markup { get; set; }
        public ImpersonationConfig ImpersonationConfig { get; set; }
    }

    public class HSBuyer : Buyer<BuyerXp>, IHSObject
    {
        
    }

	// just int for now, but leaving the door open for future configurations on how this markup functions
	public class BuyerMarkup
    {
        public int Percent { get; set; }
    }

    public class BuyerXp
    {
        // temporary field while waiting on content docs
        public int MarkupPercent { get; set; }
        public string ChiliPublishFolder { get; set; }
        public string URL { get; set; }
    }
}
