using System;
using System.Collections.Generic;
using System.Text;

namespace ordercloud.integrations.library
{
    /// <summary>
    /// Use on model properties and action params to document sample data
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DocSampleDataAttribute : Attribute
    {
        public DocSampleDataAttribute(object value)
        {
            Value = value;
        }

        public object Value { get; private set; }
    }
}
