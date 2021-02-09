using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace ordercloud.integrations.library
{
    public class ApiProperty : ApiField
    {
        public PropertyInfo PropInfo { get; set; }
        public override Type Type => PropInfo.PropertyType;
        public override bool HasDefaultValue => PropInfo.HasAttribute<DefaultValueAttribute>();
        public override object DefaultValue => PropInfo.GetAttribute<DefaultValueAttribute>()?.Value;
        public bool ReadOnly { get; set; }
        public bool WriteOnly { get; set; }
        public object SampleData { get; set; }
    }
}
