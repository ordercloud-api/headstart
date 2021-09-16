using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace ordercloud.integrations.library
{
    public class PropertyObject
    {
        private readonly JObject _obj = new JObject();
        public PropertyObject(Type propType)
        {
            var complete = false;
            while (!complete)
            {
                var simpleName = propType.PropertySimpleName();
                var formatName = propType.TypeFormatName();

                if (propType.IsCollection())
                {
                    _obj.Add("type", (propType.IsGenericType && propType.GenericTypeArguments[0].IsEnum) ? "string" : "array");
                    propType = propType.GetCollectionItemType();
                    continue;
                }

                var objectToAdd = "type";
                if (_obj["type"] != null) objectToAdd = "items";


                if (propType.IsEnum || (propType.IsGenericType && propType.GenericTypeArguments[0].IsEnum))
                {
                    _obj.Add("enum",
                        propType.IsEnum
                            ? new JArray(propType.GetEnumNames())
                            : new JArray(propType.GenericTypeArguments[0].GetEnumNames()));

                    if (objectToAdd == "items")
                        _obj.Add("items", new JObject(new JProperty("type", "string")));
                    else
                        _obj.Add("type", "string");
                    complete = true;
                }
                else if (simpleName.IsBasicType())
                {
                    if (objectToAdd == "items")
                    {
                        var itemObj = new JObject(new JProperty("type", simpleName));

                        if (formatName.IsValidFormatName())
                            itemObj.Add("format", formatName);

                        _obj.Add("items", itemObj);
                        complete = true;
                    }
                    else
                    {
                        _obj.Add("type", simpleName);
                        if (formatName.IsValidFormatName())
                            _obj.Add("format", formatName);
                        complete = true;
                    }
                }
                else
                {
                    if (objectToAdd == "items")
                        _obj.Add("items", new JObject(new JProperty("$ref", "#/components/schemas/" + simpleName)));
                    else
                        _obj.Add("allOf", new JArray(new JObject(new JProperty("$ref", "#/components/schemas/" + simpleName))));
                    complete = true;
                }
            }
        }

        public JObject ToJObject()
        {
            return _obj;
        }
    }
}
