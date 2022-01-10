using System;
using System.Collections.Generic;
using System.Text;

namespace ordercloud.integrations.vertex
{
	public class VertexResponse<T>
	{
		public VertexResponseMeta meta { get; set; }
		public T data { get; set; }
		public List<VertexResponseError> errors { get; set; } = new List<VertexResponseError>();
	}

	public class VertexResponseMeta
	{
		public string app { get; set; }
		public DateTime timeReceived { get; set; }
		public int timeElapsed { get; set; }
	}

	public class VertexResponseError
	{
		public string status { get; set; } // e.g. "401"
		public string code { get; set; } // e.g. "UNAUTHORIZED"
		public string title { get; set; } // e.g. "Unauthorized"
		public string detail { get; set; } // e.g. "invalid access token"
	}

	public class VertexCalculateTaxResponse : VertexCalculateTaxRequest
	{

		public double subTotal { get; set; }
		public double total { get; set; }
		public double totalTax { get; set; }
		public new List<VertexResponseLineItem> lineItems { get; set; } = new List<VertexResponseLineItem>();
	}
}
