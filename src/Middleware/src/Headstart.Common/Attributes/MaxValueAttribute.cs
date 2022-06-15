using System.ComponentModel.DataAnnotations;

namespace Headstart.Common.Attributes
{
    public class MaxValueAttribute : RangeAttribute
    {
        public MaxValueAttribute(int value)
            : base(int.MinValue, value)
        {
        }

        public override string FormatErrorMessage(string name)
        {
            return $"{name} must be less than {this.Minimum}.";
        }
    }
}
