using OrderCloud.SDK;

namespace Headstart.Common.Models.Headstart
{
	public class HSCostCenter : CostCenter<CostCenterXp>
	{
		public string Id { get; set; } = string.Empty;
	}

	public class CostCenterXp
	{
	}
}