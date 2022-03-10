using Headstart.Common.Models.Base;

namespace Headstart.Common.Services.Portal.Models
{
	public class Marketplace : HsBaseObject
	{
		public string Name { get; set; } = string.Empty;

		public PortalUser Owner { get; set; } = new PortalUser();

		public string Environment { get; set; } = string.Empty;
	}
}