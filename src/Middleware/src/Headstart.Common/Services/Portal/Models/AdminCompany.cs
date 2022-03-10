using Headstart.Common.Models.Base;

namespace Headstart.Common.Services.Portal.Models
{
	public class AdminCompany : HsBaseObject
	{
		public string Name { get; set; } = string.Empty;

		public int OwnerDevId { get; set; }

		public object AutoForwardingUserId { get; set; }
	}
}