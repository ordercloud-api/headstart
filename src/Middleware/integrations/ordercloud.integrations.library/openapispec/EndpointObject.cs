using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace ordercloud.integrations.library
{
    public class EndpointObject
    {
        private readonly JObject _obj = new JObject();

        public EndpointObject(ApiResource res, ApiEndpoint method, ApiMetaData data)
        {
            //Add responses obj
            _obj.Add("responses", new ResponseObject(method).ToJObject());
            //Add operationId and tags
            _obj.Add("operationId", res.Name + '.' + method.Name);
            _obj.Add("tags", new JArray(res.Description));
            //Add parameters obj
            var param = new ParamObject(res, method, data).ToTuples();
            param.ForEach(p => _obj.Add(p.Item1, p.Item2));

            _obj.Add("summary", method.Description);
            _obj.Add("description", string.Join("<br/></br>", method.Comments));

            //Add scopes needed to access this endpoint 
            _obj.Add("security", new JArray(new JObject(new JProperty("OAuth2", method.RequiredRoles))));
        }

        public JObject ToJObject()
        {
            return _obj;
        }
    }
}
