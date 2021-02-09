using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json.Linq;
using ordercloud.integrations.library;
using OrderCloud.SDK;

namespace ordercloud.integrations.library
{
    public class SchemaObject
    {
        private readonly JObject _obj = new JObject();
        public SchemaObject(ApiMetaData data)
        {
            var schemas = new Dictionary<string, JToken>();

            foreach (var model in data.Models)
            {
                if (!model.Name.Contains("Partial"))
                {
                    var modelDefinitionKey = model.Name;
                    var schemaDef = new JObject { { "type", "object" } };

                    var propertiesDef = new JObject();
                    foreach (var prop in model.Properties)
                    {
                        var propDefinitionKey = prop.Name;
                        var propObject = new PropertyObject(prop.Type).ToJObject();
                        if (prop.HasDefaultValue)
                            propObject.Add("default", new JValue(prop.DefaultValue));

                        if (prop.ReadOnly || model.IsReadOnly)
                            propObject.Add("readOnly", true);

                        var docCommentsAttribute =
                            prop.PropInfo.GetCustomAttribute<DocCommentsAttribute>();

                        if (docCommentsAttribute != null)
                            propObject.Add("description", string.Join("\n", docCommentsAttribute.Comments));

                        var minValueAttribute =
                            prop.PropInfo.GetCustomAttribute<MinValueAttribute>();

                        if (minValueAttribute != null)
                            propObject.Add("minimum", minValueAttribute.Minimum.ToJRaw());

                        var maxValueAttribute =
                            prop.PropInfo.GetCustomAttribute<MaxValueAttribute>();

                        if (maxValueAttribute != null)
                            propObject.Add("maximum", maxValueAttribute.Maximum.ToJRaw());

                        var maxLengthAttribute =
                            prop.PropInfo.GetCustomAttribute<MaxLengthAttribute>();

                        if (maxLengthAttribute != null)
                            propObject.Add("maxLength", maxLengthAttribute.Length);

                        var minLengthAttribute =
                            prop.PropInfo.GetCustomAttribute<MinLengthAttribute>();

                        if (minLengthAttribute != null)
                            propObject.Add("minLength", minLengthAttribute.Length);

                        if (propDefinitionKey.Contains("Password"))
                        {
                            if (propObject.ContainsKey("format"))
                                propObject.Merge(new JObject(new JProperty("format", "password")));
                            else
                                propObject.Add("format", "password");
                        }
                        propertiesDef.Add(propDefinitionKey, propObject);
                    }

                    schemaDef.Add("example", model.Sample.ToJRaw());
                    schemaDef.Add("properties", propertiesDef);
                    schemas.Add(modelDefinitionKey, schemaDef);
                }
            }
            schemas.Add("Meta", new DefinitionObject(typeof(ListPageMeta)).ToJObject());
            schemas.Add("MetaWithFacets", new DefinitionObject(typeof(ListPageMetaWithFacets)).ToJObject());
            schemas.Add("ListFacet", new DefinitionObject(typeof(ListFacet)).ToJObject());
            schemas.Add("ListFacetValue", new DefinitionObject(typeof(ListFacetValue)).ToJObject());

            //Find all methods that return ListPage and create all the different types of List objects 
            foreach (var endpoint in data.Resources.SelectMany(r => r.Endpoints).Where(e => e.IsList))
            {
                var itemType = endpoint.ResponseModel.Type
                    .UnwrapGeneric(typeof(ListPage<>))
                    .UnwrapGeneric(typeof(ListPageWithFacets<>)).PropertySimpleName();

                var listTypeName = "List" + itemType;
                //Check if this list type doesn't exist yet 
                if (!schemas.ContainsKey(listTypeName))
                {
                    var listObj = new JObject { { "type", "object" } };
                    var listProperties = new JObject
                    {
                        {
                            "Items", new JObject(
                                new JProperty("type", "array"),
                                new JProperty("items",
                                    new JObject(new JProperty("$ref", "#/components/schemas/" + itemType)))
                            )
                        }
                    };
                    var metaDef = (endpoint.ResponseModel.Type.WithoutGenericArgs() == typeof(ListPageWithFacets<>)) ? "MetaWithFacets" : "Meta";
                    listProperties.Add("Meta", new JObject(new JProperty("$ref", "#/components/schemas/" + metaDef)));
                    listObj.Add("properties", listProperties);
                    schemas.Add(listTypeName, listObj);
                }
            }

            foreach (var def in schemas.OrderBy(d => d.Key))
                _obj.Add(def.Key, def.Value);
            _obj.Add("Authentication", new JObject(
                new JProperty("type", "object"),
                new JProperty("properties",
                    new JObject(new JProperty("access_token",
                    new JObject(new JProperty("type", "string"))))
            )));
        }

        public JObject ToJObject()
        {
            return _obj;
        }
    }
}
