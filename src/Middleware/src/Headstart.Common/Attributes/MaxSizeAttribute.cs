using System.ComponentModel.DataAnnotations;

namespace Headstart.Common.Attributes
{
    public class MaxSizeAttribute : StringLengthAttribute
    {
        public MaxSizeAttribute(int length)
            : base(length)
        {
        }

        public override bool IsValid(object value)
        {
            return base.IsValid(value?.ToString());
        }
    }
}
