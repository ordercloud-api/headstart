using System.ComponentModel.DataAnnotations;

namespace Headstart.Common.Attributes
{
    public class MinValueAttribute : RangeAttribute
    {
        public MinValueAttribute(int value)
            : base(value, int.MaxValue)
        {
        }

        public MinValueAttribute(double value)
            : base(value, double.MaxValue)
        {
        }

        public override string FormatErrorMessage(string name)
        {
            return $"{name} must be {this.Minimum} or greater.";
        }
    }
}
