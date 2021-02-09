using System;
using System.Collections.Generic;
using System.Text;

namespace ordercloud.integrations.library
{
    public class InvalidPropertyError
    {
        public InvalidPropertyError(Type type, string name)
        {
            Property = $"{type.Name}.{name}";
        }
        public string Property { get; set; }
    }
}
