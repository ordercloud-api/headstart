using System.Collections.Generic;
using System.Linq;
using OrderCloud.Catalyst;

namespace OrderCloud.Integrations.Vertex.Models
{
    public class VertexException : CatalystBaseException
    {
        public VertexException(List<VertexResponseError> errors)
            : base(
                "VertexTaxCalculationError",
                "The vertex api returned an error: https://restconnect.vertexsmb.com/vertex-restapi/v1/sale",
                errors,
                int.Parse(errors.First().status))
        {
        }
    }
}
