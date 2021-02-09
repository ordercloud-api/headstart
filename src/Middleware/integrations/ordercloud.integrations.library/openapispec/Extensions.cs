using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Humanizer;
using Newtonsoft.Json.Linq;
using OrderCloud.SDK;

namespace ordercloud.integrations.library
{
    public static class Extensions
    {
        public static JObject AddComponents(this JObject spec, ApiMetaData data)
        {
            spec.Add("components", new JObject(
                new JProperty("securitySchemes", new SecurityObject(data).ToJObject()),
                new JProperty("schemas", new SchemaObject(data).ToJObject())));
            return spec;
        }

        public static JObject AddMetaData(this JObject spec, SwaggerConfig config)
        {
            //Set version number, name, description etc.. 
            spec.Add("openapi", "3.0.0");
            spec.Add("info", new JObject
            {
                { "title", config.Title },
                { "description", config.Description },
                { "version", config.Version },
                { "contact", new JObject(
                    new JProperty("name", config.Name),
                    new JProperty("url", config.Url),
                    new JProperty("email", config.ContactEmail)) }
            });
            spec.Add("servers", new JArray
            {
                new JObject(
                    new JProperty("url", config.Host),
                    new JProperty("description", config.Description))
            });
            return spec;
        }

        public static JObject AddResourceTags(this JObject spec, ApiMetaData data)
        {
            var tagsArray = new JArray();

            foreach (var section in data.Sections)
            {
                tagsArray.Add(new JObject(
                    new JProperty("name", section.Name),
                    new JProperty("description", String.Join("\n", section.Description)),
                    new JProperty("x-id", section.ID)));

                foreach (var resource in section.Resources)
                {
                    tagsArray.Add(new JObject(
                        new JProperty("name", resource.Description),
                        new JProperty("description", String.Join("\n", resource.Comments)),
                        new JProperty("x-section-id", section.ID)));
                }
            }
            spec.Add("tags", tagsArray);
            return spec;
        }

        public static JObject AddPathObjects(this JObject spec, ApiMetaData data)
        {
            var paths =
                from s in data.Sections
                from r in s.Resources
                from e in r.Endpoints
                orderby e.Route
                select new
                {
                    Path = $"/{e.Route.TrimStart("v1")}",
                    Verb = e.HttpVerb.Method.ToLower(),
                    EndpointObj = new EndpointObject(r, e, data).ToJObject()
                };

            var jp = new JObject();
            foreach (var p in paths.GroupBy(p => p.Path))
            {
                var je = new JObject();
                foreach (var e in p.OrderBy(x => x.Verb.SortOrder()))
                    je.Add(e.Verb, e.EndpointObj);
                jp.Add(p.Key, je);
            }
            spec.Add("paths", jp);
            return spec;
        }

        public static JArray BuildStringListEnum(this ApiModel resource, string pName)
        {
            // stopped here.. make this generic by the attribute type, so the two types can be evaluated to have an enum array or not
            var sortableProperties = resource.Properties
                .Where(p => {
                    return p.PropInfo.CustomAttributes.Any(a => a.AttributeType == typeof(SortableAttribute));
                })
                .Select(p => {
                    var attributes = p.PropInfo.GetAttributes<Attribute>();
                    var sortableAttr = attributes.OfType<SortableAttribute>().FirstOrDefault();
                    var propertyObj = new
                    {
                        p.Name,
                        sortableAttr?.Priority
                    };
                    return propertyObj;
                })
                .OrderBy(p => p.Priority == null).ThenBy(p => p.Priority);

            var searchableProperties = resource.Properties
                .Where(p => {
                    return p.PropInfo.CustomAttributes.Any(a => a.AttributeType == typeof(SearchableAttribute));
                })
                .Select(p => {
                    var attributes = p.PropInfo.GetAttributes<Attribute>();
                    var searchableAttr = attributes.OfType<SearchableAttribute>().FirstOrDefault();
                    var propertyObj = new
                    {
                        p.Name,
                        searchableAttr?.Priority
                    };
                    return propertyObj;
                })
                .OrderBy(p => p.Priority == null)
                .ThenBy(p => p.Priority);

            return pName == "sortBy" ?
                new JArray(sortableProperties.Select(p => p.Name)) :
                new JArray(searchableProperties.Select(p => p.Name));
        }

        public static string TypeFormatName(this Type type)
        {
            if (type.IsArray)
                return type.GetElementType().TypeFormatName();

            if (type.IsNullable())
                return type.GetGenericArguments()[0].TypeFormatName();

            type = type
                .UnwrapGeneric(typeof(Task<>))
                .UnwrapGeneric(typeof(Partial<>));

            if (type.IsGenericType)
            {
                var args = type.GetGenericArguments().Select(t => t.TypeFormatName());
                return $"{type.Name.Split('`')[0]}<{String.Join(", ", args)}>";
            }

            switch (type.Name)
            {
                case "Date":
                    return "date";
                case "Byte":
                    return "byte";
                case "Binary":
                    return "binary";
                case "DateTimeOffset":
                    return "date-time";
                case "Int32":
                    return "int32";
                case "Int64":
                    return "int64";
                case "Decimal":
                    return "float";
                case "Double":
                    return "double";
                default:
                    return null;
            }
        }

        public static string PropertySimpleName(this Type type)
        {
            if (type.IsArray)
                return type.GetElementType().PropertySimpleName();

			if (type.IsCollection())
				return "array";

			if (type.IsNullable())
                return type.GetGenericArguments()[0].PropertySimpleName();

            type = type
                .UnwrapGeneric(typeof(Task<>))
                .UnwrapGeneric(typeof(Partial<>));

            if (type.IsGenericType)
            {
                var args = type.GetGenericArguments().Select(s => s.PropertySimpleName());
                return $"{type.Name.Split('`')[0]}<{String.Join(", ", args)}>";
            }

            switch (type.Name)
            {
                case "DateTimeOffset":
                    return "string";
                case "String":
                    return "string";
                case "Int32":
                    return "integer";
                case "Decimal":
                case "Double":
                    return "number";
                case "Boolean":
                    return "boolean";
                case "Object":
                case "JObject":
                case "JRaw":
                case "XP":
                    return "object";
                case "HttpResponseMessage":
                case "IHttpActionResult":
                case "Void":
                    return null;
                default:
                    return type.Name;
            }
        }

        /// <summary>
        /// Returns the T in List[T] or Task[T], for example, if contained in specified type. Otherwise returns self.
        /// </summary>
        public static Type UnwrapGeneric(this Type type, Type genericContainerType)
        {
            return (type?.WithoutGenericArgs() == genericContainerType)
                ? type?.GetGenericArguments().FirstOrDefault()
                : type;
        }

        public static int SortOrder(this string verb)
        {
            switch (verb.ToLower())
            {
                case "get":
                    return 1;
                case "list":
                    return 2;
                case "post":
                case "create":
                    return 3;
                case "save":
                case "put":
                    return 4;
                case "delete":
                    return 5;
                case "patch":
                    return 6;
                default: return 7;
            }
        }

        public static bool IsBasicType(this string type)
        {
            string[] basicTypes = { "integer", "number", "string", "boolean", "array", "object" };
            return basicTypes.Any(type.Contains);
        }

        public static bool IsValidFormatName(this string type)
        {
            if (type == null) return false;
            string[] validFormatNames =
                {"int32", "int64", "float", "double", "byte", "binary", "date", "date-time", "password"};
            return validFormatNames.Any(type.Contains);
        }

        public static string SimpleType(this PropertyInfo pi)
        {
            var typeAttr = pi.GetAttribute<DocType>();
            if (typeAttr != null)
                return typeAttr.TypeName;

            return pi.PropertyType.ParameterSimpleName();
        }

        public static string ParameterSimpleName(this Type type)
        {
            type = type.WithoutNullable();

            if (type.IsCollection())
                return "array";

            if (type.IsEnum)
                return "string";

            // DateTimeOffset has typecode object for some reason
            if (type == typeof(DateTimeOffset))
                return "date";

            switch (Type.GetTypeCode(type.WithoutNullable()))
            {
                case TypeCode.String:
                    return "string";
                case TypeCode.Boolean:
                    return "boolean";
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return "integer";
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return "float";
                case TypeCode.DateTime:
                    return "date";
                default:
                    return "object";
            }
        }

        public static Type WithoutNullable(this Type type)
        {
            return type.IsNullable() ? Nullable.GetUnderlyingType(type) : type;
        }
        
        public static bool IsListPage(this Type type)
        {
            var t = type?.WithoutGenericArgs();
            return t == typeof(ListPage<>) || t == typeof(ListPageWithFacets<>);
        }

        public static bool IsNumeric(this Type type)
        {
            if (type == null || type.IsEnum)
                return false;

            switch (Type.GetTypeCode(type.WithoutNullable()))
            {
                case TypeCode.Byte:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsModelType(this Type type)
        {
            var response = type.IsClass
                           && (type.Assembly == Assembly.GetAssembly(typeof(OrderCloudModel))
                               || type.HasAttribute<SwaggerModel>(true))
                           && type.WithoutGenericArgs() != typeof(ListArgs<>);
            return response;
        }

        public static string ControllerFriendlyName(this Type type)
        {
            var name = type.Name.Replace("Controller", "");
            switch (name)
            {
                default:
                    return name.Pluralize();
            }
        }

        public static Type ResponseType(this MethodInfo m)
        {
            var type = m.ReturnType?.UnwrapGeneric(typeof(Task<>));
            if (type == typeof(void))
                return null;
            return type == typeof(Task) ? null : type;
        }

        public static int HttpStatusCode(this Type returnType, HttpMethod verb)
        {
            if (returnType == null || returnType == typeof(void))
                return 204;
            if (verb == System.Net.Http.HttpMethod.Post && returnType.GetProperty("ID") != null)
                return 201;
            return 200;
        }
    }
}
