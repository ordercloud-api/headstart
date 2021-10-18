using System;
using System.Collections.Generic;
using System.Text;

namespace ordercloud.integrations.vertex
{
	public class VertexCustomer
	{
		public VertexCustomerCode customerCode { get; set; }
		public VertexLocation destination { get; set; }
	}

	public class VertexCustomerCode
	{
		/// <summary>
		/// A code used to represent groups of customers, vendors, dispatchers, or recipients who have similar taxability. Note: If you pass a classCode, you should not pass a TaxRegistrationNumber because the registration number does not apply to the entire class.
		/// </summary>
		public string classCode { get; set; }
		public string value { get; set; }
	}
}
