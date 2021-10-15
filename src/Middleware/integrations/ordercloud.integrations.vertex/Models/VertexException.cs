using OrderCloud.Catalyst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ordercloud.integrations.vertex
{
	public class VertexException : CatalystBaseException
	{
		public VertexException(List<VertexResponseError> errors) : 
			base(
				"VertexTaxCalculationError", 
				"The vertex api returned an error: https://restconnect.vertexsmb.com/vertex-restapi/v1/sale", 
				errors, 
				int.Parse(errors.First().status
			)) { }
	}
}
