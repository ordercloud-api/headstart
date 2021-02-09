using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ordercloud.integrations.library
{
    public class MaxSizeAttribute : StringLengthAttribute
    {
        public MaxSizeAttribute(int length) : base(length) { }

        public override bool IsValid(object value)
        {
            return base.IsValid(value?.ToString());
        }
    }
}
