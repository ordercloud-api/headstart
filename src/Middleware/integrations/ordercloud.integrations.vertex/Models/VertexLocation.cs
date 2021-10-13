using System;
using System.Collections.Generic;
using System.Text;

namespace ordercloud.integrations.vertex
{
	public class VertexLocation
	{
		public string streetAddress1 { get; set; }
		public string streetAddress2 { get; set; }
		public string city { get; set; }
		public string mainDivision { get; set; } // e.g. state
		public string subDivision { get; set; } // e.g. county
		public string postalCode { get; set; }
		public string country { get; set; }

	}
}
