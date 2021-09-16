using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ordercloud.integrations.library
{
    public class ApiParameter : ApiField
    {
        public ParameterInfo ParamInfo { get; set; }
        public bool IsListArg { get; set; }
        public override Type Type => ParamInfo.ParameterType;
        public override bool HasDefaultValue => ParamInfo.HasDefaultValue;
        public override object DefaultValue => ParamInfo.DefaultValue;
    }
}
