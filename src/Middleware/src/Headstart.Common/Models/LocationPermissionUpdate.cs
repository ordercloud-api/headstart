using System.Collections.Generic;
using OrderCloud.SDK;

namespace Headstart.Common.Models
{
    public class LocationPermissionUpdate
    {
        public List<UserGroupAssignment> AssignmentsToAdd { get; set; }

        public List<UserGroupAssignment> AssignmentsToDelete { get; set; }
    }

    public class LocationApprovalThresholdUpdate
    {
        public decimal Threshold { get; set; }
    }
}
