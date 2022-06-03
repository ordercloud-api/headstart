using Headstart.Models.Misc;
using OrderCloud.SDK;

namespace Headstart.Common.Models
{
    public class HSSecurityProfile
    {
        public CustomRole ID { get; set; }

        public ApiRole[] Roles { get; set; }

        public CustomRole[] CustomRoles { get; set; }
    }
}
