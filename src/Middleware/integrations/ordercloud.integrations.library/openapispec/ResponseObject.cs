using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrderCloud.SDK;

namespace ordercloud.integrations.library
{
    public class ResponseObject
    {
        private readonly JObject _responses = new JObject();
        public ResponseObject(ApiEndpoint endpoint)
        {
            var responseObj = new JObject { { "description", string.Join(".", endpoint.Comments) } };

			var returnType = endpoint.MethodInfo.ReturnType
				.UnwrapGeneric(typeof(Task<>))
				.UnwrapGeneric(typeof(ListPage<>))
				.UnwrapGeneric(typeof(ListPageWithFacets<>));
			var returnTypeSimpleName = returnType.PropertySimpleName();

			if (endpoint.IsList)
				returnTypeSimpleName = "List" + returnTypeSimpleName;

            if (returnTypeSimpleName != null && returnTypeSimpleName != "Task")
            {
				if (returnTypeSimpleName.IsBasicType())
				{
					var schemaObj = new PropertyObject(returnType).ToJObject();
					responseObj.Add("content", new JObject(
						new JProperty("application/json", new JObject(
							new JProperty("schema", schemaObj)))));
				}
				else 
                {
                    var schemaObj = new JObject { { "$ref", $"#/components/schemas/{returnTypeSimpleName}" } };
                    responseObj.Add("content", new JObject(
                        new JProperty("application/json", new JObject(
                            new JProperty("schema", schemaObj)))));
                }
            }

            _responses.Add(endpoint.HttpStatus.ToString(), responseObj);
        }

        public JObject ToJObject()
        {
            return _responses;
        }
    }
}
