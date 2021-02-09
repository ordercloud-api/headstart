using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace ordercloud.integrations.library
{
    public class SecurityObject
    {
        private readonly JObject _obj;
        public SecurityObject(ApiMetaData data)
        {
            var scopes = new JObject();
            foreach (var role in data.Roles)
            {
                scopes.Add(role, "");
            }

            var flowDefinition = new JObject(
                new JProperty("tokenUrl", "/oauth/token"),
                new JProperty("scopes", scopes)
            );

            _obj = new JObject
            {
                {
                    "OAuth2", new JObject(
                        new JProperty("type", "oauth2"),
                        new JProperty("flows", new JObject(
                            new JProperty("password", flowDefinition),
                            new JProperty("clientCredentials", flowDefinition)
                        )))
                }
            };
        }

        public JObject ToJObject()
        {
            return _obj;
        }
    }
}
