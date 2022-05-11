using System;
using System.Collections.Generic;
using System.Text;

namespace Headstart.Common.Services.CMS.Models
{
	public class AssetAssignment
	{
		public string AssetID { get; set; }
		public string ResourceID { get; set; }
		public ResourceType? ResourceType { get; set; }
		public string ParentResourceID { get; set; } = null;
		public ParentResourceType? ParentResourceType { get; set; }
	}
}
