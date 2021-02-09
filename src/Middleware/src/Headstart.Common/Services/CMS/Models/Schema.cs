using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Headstart.Common.Services.CMS.Models
{
	public class DocSchema
	{
		public string ID { get; set; }
		public List<ResourceType> RestrictedAssignmentTypes { get; set; } = new List<ResourceType>(); // empty means no restrictions
		public JObject Schema { get; set; }
		public History History { get; set; }
	}
}
