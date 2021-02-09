using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace ordercloud.integrations.library
{
    public class OpenApiGenerator<TController, TAttribute> where TController : Controller
        where TAttribute : Attribute, IApiAuthAttribute
    {
        private ApiMetaData _data;
        private JObject _spec;
        public OpenApiGenerator()
        {
        }

        public JObject Specification()
        {
            return this._spec;
        }

        public OpenApiGenerator<TController, TAttribute> CollectMetaData(string refPath, IDictionary<string, IErrorCode> errors)
        {
            this._data = ApiReflector.GetMetaData<TController, TAttribute>(refPath, errors);
            return this;
        }

        public OpenApiGenerator<TController, TAttribute> DefineSpec(SwaggerConfig config)
        {
            this._spec = new JObject()
                .AddMetaData(config)
                .AddResourceTags(_data)
                .AddComponents(_data)
                .AddPathObjects(_data);
            return this;
        }
    }
}
