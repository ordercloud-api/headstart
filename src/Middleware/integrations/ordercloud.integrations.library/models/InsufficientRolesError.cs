using System;
using System.Collections.Generic;
using System.Text;
using OrderCloud.SDK;

namespace ordercloud.integrations.library
{
    public class InsufficientRolesError
    {
        public IList<ApiRole> RequiredRoles { get; set; }
        public IList<ApiRole> AssignedRoles { get; set; }
    }
}
