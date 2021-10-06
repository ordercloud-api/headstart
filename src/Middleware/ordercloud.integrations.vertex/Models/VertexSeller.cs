using System;
using System.Collections.Generic;
using System.Text;

namespace ordercloud.integrations.vertex
{
	public class VertexSeller
	{
		public VertexLocation physicalOrigin { get; set; }
		public VertexLocation administrativeOrigin { get; set; }
		public bool nexusIndicator { get; set; }
		public string nexusReasonCode { get; set; }
		public string company { get; set; }
		public string division { get; set; }
		public string department { get; set; }
		public List<VertexTaxRegistrations> taxRegistrations { get; set; }
	}
}
