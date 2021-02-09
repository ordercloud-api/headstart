using OrderCloud.SDK;

namespace Headstart.Models.Misc
{
	public class HSSecurityProfile
	{
		public CustomRole CustomRole { get; set; }
		public ApiRole[] Roles { get; set; }
		public CustomRole[] CustomRoles { get; set; }
	}
}
