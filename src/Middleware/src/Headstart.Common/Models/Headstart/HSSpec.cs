using OrderCloud.SDK;
using Headstart.Common.Models.Headstart.Extended;
using RequiredAttribute = System.ComponentModel.DataAnnotations.RequiredAttribute;

namespace Headstart.Common.Models.Headstart
{
	public class HsSpec : Spec
	{
		public string Id { get; set; } = string.Empty;
	}

	public class SpecXp
	{
		[Required]
		public SpecUI UI { get; set; } = new SpecUI();
	}
}