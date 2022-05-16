﻿using System.ComponentModel.DataAnnotations;

namespace OrderCloud.Integrations.Library.Cosmos
{
    public class CosmosInteropIDAttribute : RegularExpressionAttribute
    {
        public CosmosInteropIDAttribute()
            : base(CosmosInteropID.ValidationRegex)
        {
            AutoGen = true;
        }

        /// <summary>
        /// generate if not set. defaults to true.
        /// </summary>
        public bool AutoGen { get; set; }

        public override bool IsValid(object value)
        {
            var s = value as string;

            // ID can be null, that triggers us to generate one
            return string.IsNullOrEmpty(s) || (s.Length <= 100 && base.IsValid(value));
        }

        public override string FormatErrorMessage(string name)
        {
            return $"{name} can only contain characters Aa-Zz 0-9 - _";
        }

        protected override ValidationResult IsValid(object value, ValidationContext ctx)
        {
            // if model is sent with a null or empty ID, generate one
            if (string.IsNullOrEmpty(value as string) && AutoGen)
            {
                var idProp = ctx.ObjectType.GetProperty(ctx.MemberName);
                idProp.SetValue(ctx.ObjectInstance, CosmosInteropID.New());
                return null;
            }

            return base.IsValid(value, ctx);
        }
    }
}
