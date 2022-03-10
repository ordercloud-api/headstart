using OrderCloud.SDK;
using Headstart.Common.Models.Misc;

namespace Headstart.Common.Models.Headstart
{
	public class HsSecurityProfile
	{
		public CustomRole Id { get; set; }
		
		public ApiRole[] Roles { get; set; }

		public CustomRole[] CustomRoles { get; set; }
	}
}