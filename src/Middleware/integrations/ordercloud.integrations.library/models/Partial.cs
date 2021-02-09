using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ordercloud.integrations.library
{
    public interface IPartial
    {
        JObject Values { get; set; }
    }

    [JsonConverter(typeof(PartialConverter))]
    public class Partial<T> : IPartial, IValidatableObject
    {
        public JObject Values { get; set; }

        public TProp GetProperty<TProp>(Expression<Func<T, TProp>> prop)
        {
            var token = Values[prop.GetPropertyInfo().Name];
            return (token != null) ? token.Value<TProp>() : default(TProp);
        }

        public bool HasProperty<TProp>(Expression<Func<T, TProp>> prop)
        {
            return Values[prop.GetPropertyInfo().Name] != null;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            foreach (var jprop in Values)
            {
                var prop = typeof(T).GetProperty(jprop.Key);
                if (prop != null)
                {
                    object val;
                    if (TryGetValue(jprop.Value, prop.PropertyType, out val))
                    {
                        validationContext.MemberName = jprop.Key;
                        foreach (var validator in prop.GetAttributes<ValidationAttribute>())
                        {
                            yield return validator.GetValidationResult(val, validationContext);
                        }
                    }
                    else
                    {
                        yield return new ValidationResult("Incorrect data type.", new[] { prop.Name });
                    }
                }
            }
        }

        private bool TryGetValue(JToken token, Type type, out object val)
        {
            try
            {
                val = token.ToObject(type);
                return true;
            }
            catch (JsonReaderException)
            {
                val = null;
                return false;
            }
        }
    }

    public class PartialConverter : JsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var partial = Activator.CreateInstance(objectType) as IPartial;
            partial.Values = serializer.Deserialize<JObject>(reader);
            return partial;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType) => objectType.WithoutGenericArgs() == typeof(Partial<>);
        public override bool CanRead => true;
        public override bool CanWrite => false;
    }
}
