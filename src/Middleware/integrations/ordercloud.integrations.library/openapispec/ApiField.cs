using System;
using System.Collections.Generic;
using System.Text;

namespace ordercloud.integrations.library
{
    public abstract class ApiField
    {
        public abstract Type Type { get; }
        public string Name { get; set; }
        public string SimpleType { get; set; }
        public string Description { get; set; }
        public bool Required { get; set; }
        public abstract bool HasDefaultValue { get; }
        public abstract object DefaultValue { get; }
    }
}
