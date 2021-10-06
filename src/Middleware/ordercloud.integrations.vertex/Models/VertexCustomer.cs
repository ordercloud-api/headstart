using System;
using System.Collections.Generic;
using System.Text;

namespace ordercloud.integrations.vertex
{
	public class VertexCustomer
	{
		public string customerCode { get; set; }
		public VertexLocation destination { get; set; }
		public VertexLocation administrativeDestination { get; set; }
		public VertexExemptionCertificate exemptionCertificate { get; set; }
		public List<VertexTaxRegistrations> taxRegistrations { get; set; }
		public bool isTaxExempt { get; set; }
		public string exemptionReasonCode { get; set; }
	}
}
