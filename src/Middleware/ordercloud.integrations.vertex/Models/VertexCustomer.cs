using System;
using System.Collections.Generic;
using System.Text;

namespace ordercloud.integrations.vertex
{
	public class VertexCustomer
	{
		public VertexCustomerCode customerCode { get; set; }
		public VertexLocation destination { get; set; }
		public VertexLocation administrativeDestination { get; set; }
		public VertexExemptionCertificate exemptionCertificate { get; set; }
		public List<VertexTaxRegistrations> taxRegistrations { get; set; }
		/// <summary>
		/// Indicates that the buyer is not required to collect taxes, and all line items associated with the purchase order are exempt from taxes. It is recommended that the buyer and vendor nexus requirements be created and maintained in the Vertex Central user interface. Note: this attribute is set to true if you pass an exemption certificate number in the XML message.
		/// </summary>
		public bool isTaxExempt { get; set; }
		/// <summary>
		/// The 'Reason Code' can be passed through the ERP application if functionality is provided, however most Lessee's will set up reason codes associated with Lessee's in the Vertex Central user interface by the tax department. The valid reason codes are expected from the calling application and are pre-defined by Vertex for use in the Tax Returns application. Note: To pass a standard (i.e., not user-defined) single-digit reason code, pad the code to four digits. Refer to the Supporting Information section of the XML Integration Guide for information on exemption and deduction reason codes.
		/// </summary>
		public string exemptionReasonCode { get; set; }
	}

	public class VertexCustomerCode
	{
		/// <summary>
		/// Indicates whether or not the participant is a business.
		/// </summary>
		public bool isBusinessIndicator { get; set; }
		/// <summary>
		/// A code used to represent groups of customers, vendors, dispatchers, or recipients who have similar taxability. Note: If you pass a classCode, you should not pass a TaxRegistrationNumber because the registration number does not apply to the entire class.
		/// </summary>
		public string classCode { get; set; }
		public string value { get; set; }
	}
}
