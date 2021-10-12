using System;
using System.Collections.Generic;
using System.Text;

namespace ordercloud.integrations.vertex
{
	public class VertexResponse<T>
	{
		public VertexResponseMeta meta { get; set; }
		public T data { get; set; }
	}

	public class VertexResponseMeta
	{
		public string app { get; set; }
		public DateTime timeReceived { get; set; }
		public int timeElapsed { get; set; }
	}

	public class VertexCalculateTaxResponse : VertexCalculateTaxRequest
	{

		public double subTotal { get; set; }
		public double total { get; set; }
		public double totalTax { get; set; }
		public new List<VertexResponseLineItem> lineItems { get; set; } = new List<VertexResponseLineItem>();
	}
}
