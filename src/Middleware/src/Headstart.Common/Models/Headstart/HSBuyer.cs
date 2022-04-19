using OrderCloud.SDK;

namespace Headstart.Common.Models.Headstart
{
	public class SuperHsBuyer
	{
		public HsBuyer Buyer { get; set; } = new HsBuyer();

		public BuyerMarkup Markup { get; set; } = new BuyerMarkup();

		public ImpersonationConfig ImpersonationConfig { get; set; } = new ImpersonationConfig();
	}

	public class HsBuyer : Buyer<BuyerXp>
	{
		public string Id { get; set; } = string.Empty;
	}

	/// Just int for now, but leaving the door open for future configurations on how this markup functions
	public class BuyerMarkup
	{
		public int Percent { get; set; }
	}

	public class BuyerXp
	{
		// Temporary field while waiting on content docs
		public int MarkupPercent { get; set; }

		public string ChiliPublishFolder { get; set; } = string.Empty;

		public string Url { get; set; } = string.Empty;
	}
}