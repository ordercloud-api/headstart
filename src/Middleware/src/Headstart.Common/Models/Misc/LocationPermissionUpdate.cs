using System.Collections.Generic;
using OrderCloud.SDK;

namespace Headstart.Models.Misc
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
