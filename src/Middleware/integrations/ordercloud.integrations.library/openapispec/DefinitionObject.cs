using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace ordercloud.integrations.library
{
    public class DefinitionObject
    {
        private readonly JObject _definition = new JObject { { "type", "object" } };
        public DefinitionObject(Type type)
        {
            var obj = new JObject();
            foreach (var prop in type.GetProperties())
            {
                obj.Add(prop.Name, new PropertyObject(prop.PropertyType).ToJObject());
                if (!prop.PropertyType.IsEnum &&
                    (!prop.PropertyType.IsGenericType || !prop.PropertyType.GenericTypeArguments[0].IsEnum)) continue;
            }
            _definition.Add("properties", obj);
        }

        public JObject ToJObject()
        {
            return _definition;
        }
    }
}
