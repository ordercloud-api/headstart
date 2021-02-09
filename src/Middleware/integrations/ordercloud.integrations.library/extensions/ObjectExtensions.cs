using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ordercloud.integrations.library
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// The most useful method known to mankind
        /// </summary>
        public static T To<T>(this object obj)
        {
            return (T)obj.To(typeof(T));
        }

        public static object To(this object obj, Type type)
        {
            try
            {
                if (obj == null || obj == DBNull.Value)
                    return null;

                if (type.IsInstanceOfType(obj))
                    return obj;

                if (type.IsNullable())
                    type = type.GetGenericArguments()[0];

                if (type.IsEnum)
                    return Enum.Parse(type, obj.ToString(), true);

                if (obj is JToken jt)
                    return jt.ToObject(type);

                if (type.UnderlyingSystemType == typeof(DateTimeOffset)) // TODO: figure out how to evaluate the actual Type
                    return DateTimeOffset.Parse(obj.ToString());

                return Convert.ChangeType(obj, type);
            }
            catch (Exception ex)
            {
                throw new Exception("Invalid Cast", ex);
            }

        }

        public static JRaw ToJRaw(this object obj)
        {
            return new JRaw(JsonConvert.SerializeObject(obj));
        }

        // Similar function to the To function above, but I have a situation where this works and To function didn't
        public static T Reserialize<T>(this object obj)
        {
            var serialized = JsonConvert.SerializeObject(obj);
            return JsonConvert.DeserializeObject<T>(serialized);
        }
    }
}
