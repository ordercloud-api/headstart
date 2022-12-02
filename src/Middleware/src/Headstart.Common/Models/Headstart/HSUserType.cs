using System.Collections.Generic;

namespace Headstart.Common.Models
{
    public class HSUserType
    {
        public string UserGroupIDSuffix { get; set; }

        public string UserGroupName { get; set; }

        public UserGroupType UserGroupType { get; set; }

        public List<CustomRole> CustomRoles { get; set; }

        public string Description { get; set; }
    }
}
