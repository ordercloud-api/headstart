using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json.Linq;
using OrderCloud.SDK;

namespace ordercloud.integrations.library
{
    public class ParamObject
    {
        private readonly List<Tuple<string, JRaw>> _param;
        public ParamObject(ApiResource resource, ApiEndpoint endpoint, ApiMetaData data)
        {
            _param = new List<Tuple<string, JRaw>>();
            if (endpoint.SubResource != null)
                // Used to get correct enumerables on query params such as sortBy and searchOn for the Me resource
                resource = data.Resources.FirstOrDefault(r => r.Name == endpoint.SubResource);

            var paramArray = new JArray();
            foreach (var p in endpoint.PathArgs)
            {
                var paramObj = new JObject
                {
                    {"name", p.Name},
                    {"in", "path"},
                    {"description", string.Join("\n", p.Description)},
                    {"required", p.Required}
                };
                var type = p.SimpleType;
                if (type == "date") type = "string";

                var paramSchema = new JObject(new JProperty("type", type));

                if (p.Type.IsEnum)
                {
                    paramSchema.Add("enum", new JArray(p.Type.GetEnumNames()));
                }

                paramObj.Add("schema", paramSchema);
                paramArray.Add(paramObj);
            }

            foreach (var p in endpoint.QueryArgs)
            {
                var paramObj = new JObject
                {
                    {"name", p.Name},
                    {"in", "query"},
                    {"description", string.Join("\n", p.Description)},
                    {"required", p.Required}
                };
                var type = p.SimpleType;
                if (type == "date") type = "string";


                var paramSchema = new JObject();

                //TODO: figure out how to identify List<string> types vs. simple string types.
                if (p.Name == "sortBy" || p.Name == "searchOn")
                {
                    var model = (endpoint.ResponseModel.Type.WithoutGenericArgs() == typeof(ListPage<>)
                        ? endpoint.ResponseModel.InnerModel
                        : endpoint.ResponseModel).BuildStringListEnum(p.Name);

                    //TODO: evaluate the best way to handle this.. It's hard coded to OC
                    paramSchema.Add("type", "array");
                    paramSchema.Add("items", new JObject(
                        new JProperty("type", "string"),
                        new JProperty("enum", model.Count > 0 ? model : new JArray("ID"))));
                }
                else
                {
                    paramSchema.Add("type", p.Name == "filters" ? "object" : type);

                    if (p.Type.IsEnum)
                    {
                        paramSchema.Add("enum", new JArray(p.Type.GetEnumNames()));
                    }
                }
                paramObj.Add("schema", paramSchema);

                if (p.Name != "defaultSearchOn" && p.Name != "defaultSortBy")
                    paramArray.Add(paramObj);
            }

            if (endpoint.RequestModel != null)
            {
                var requestObj = new JObject { { "required", true }, { "description", "" } };
                //Body params are always required
                var type = endpoint.RequestModel.Type.PropertySimpleName();
                var schema = new JObject(
                    new JProperty("allOf", new JArray(new JObject(new JProperty("$ref", "#/components/schemas/" + type)))));
                if (!endpoint.HttpVerb.Equals(new HttpMethod("patch")))
                {
                    var requiredFields = new JArray();
                    foreach (var property in endpoint.RequestModel.Properties)
                    {
                        if (property.Required)
                        {
                            requiredFields.Add(property.Name);
                        }
                    };
                    if (requiredFields.Count > 0)
                    {
                        schema.Add("required", requiredFields);
                    }
                }

                requestObj.Add("content", new JObject(
                    new JProperty("application/json", new JObject(
                        new JProperty("schema", schema)
                    ))));

                _param.Add(new Tuple<string, JRaw>("requestBody", requestObj.ToJRaw()));
            }

            if (paramArray.Count > 0)
            {
                _param.Add(new Tuple<string, JRaw>("parameters", paramArray.ToJRaw()));
            }
        }

        public List<Tuple<string, JRaw>> ToTuples()
        {
            return _param;
        }
    }
}
