using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ordercloud.integrations.library
{
    public class MaxValueAttribute : RangeAttribute
    {
        public MaxValueAttribute(int value) : base(int.MinValue, value) { }

        public override string FormatErrorMessage(string name)
        {
            return $"{name} must be less than {this.Minimum}.";
        }
    }
}
