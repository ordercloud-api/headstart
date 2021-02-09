using System;
using System.Collections.Generic;
using System.Text;

namespace ordercloud.integrations.library
{
    /// <summary>
    /// Used to decorate any class that you wish to include in Swagger spec (Open API 3.0) file gen
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class SwaggerModel : Attribute { }
}
