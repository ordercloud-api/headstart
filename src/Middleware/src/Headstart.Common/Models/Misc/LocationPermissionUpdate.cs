using OrderCloud.SDK;
using System.Collections.Generic;

namespace Headstart.Common.Models.Misc
{
	public class LocationPermissionUpdate
	{
		public List<UserGroupAssignment> AssignmentsToAdd { get; set; } = new List<UserGroupAssignment>();

		public List<UserGroupAssignment> AssignmentsToDelete { get; set; } = new List<UserGroupAssignment>();
	}

	public class LocationApprovalThresholdUpdate
	{
		public decimal Threshold { get; set; }
	}
}