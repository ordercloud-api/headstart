using OrderCloud.SDK;

namespace Headstart.Common.Models.Headstart
{
	public class HsSpecOption : SpecOption<SpecOptionXp>
	{
		public string Id { get; set; } = string.Empty;
	}

	public class SpecOptionXp
	{
		public string Description { get; set; } = string.Empty;
		public string SpecId { get; set; } = string.Empty;
	}
}