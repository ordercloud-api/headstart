using System;
using System.Collections.Generic;
using System.Text;

namespace ordercloud.integrations.vertex
{
	public class VertexLineItem
	{
		public VertexCustomer customer { get; set; }
		/// <summary>
		/// A code from the host system that identifies the product, material, service, or SKU number. You can use the Vertex Central user interface to map your products to Product Categories. If the supplied Product and Product Class information is not recognized by the calculation engine, a general category indicating TPP is assigned. Required (1) if no Product Class information is supplied. If both are supplied, Product information supersedes Product Class information.
		/// </summary>
		public VertexProduct product { get; set; }
		/// <summary>
		/// A standardized, unique code for the product or service.
		/// </summary>
		public VertexMeasure quantity { get; set; }
		public double unitPrice { get; set; }
		public double extendedPrice { get; set; }
		public VertexDiscount discount { get; set; }
		/// <summary>
		/// An identifier that further defines the line item and corresponds to the transaction stored in the host system. This parameter is needed to perform synchronization services, but it is not used for reporting purposes.
		/// </summary>
		public string lineItemId { get; set; }
	}

	public class VertexMeasure
	{
		public double value { get; set; }
	}

	public class VertexProduct
	{
		public string productClass { get; set; }
		public string value { get; set; }
	} 

	public enum VertexTransactionType
	{
		SALE, RENTAL, LEASE
	}
}
