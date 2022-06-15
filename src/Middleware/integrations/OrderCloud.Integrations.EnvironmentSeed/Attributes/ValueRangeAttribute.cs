using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace OrderCloud.Integrations.EnvironmentSeed.Attributes
{
    public class ValueRangeAttribute : ValidationAttribute
    {
        public string[] AllowableValues { get; set; }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (AllowableValues?.Contains(value?.ToString(), StringComparer.OrdinalIgnoreCase) == true)
            {
                return ValidationResult.Success;
            }

            var msg = $"Please enter one of the allowable values: {string.Join(", ", AllowableValues ?? new string[] { "No allowable values found" })}.";
            return new ValidationResult(msg);
        }
    }
}
