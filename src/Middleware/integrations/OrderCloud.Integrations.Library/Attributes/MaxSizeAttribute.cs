using System.ComponentModel.DataAnnotations;

namespace OrderCloud.Integrations.Library.Attributes
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
