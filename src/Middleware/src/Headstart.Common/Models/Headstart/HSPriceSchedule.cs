using OrderCloud.SDK;

namespace Headstart.Common.Models.Headstart
{
	public class HsPriceSchedule : PriceSchedule<PriceScheduleXp>
	{
		public string Id { get; set; } = string.Empty;
	}

	public class PriceScheduleXp
	{
	}
}