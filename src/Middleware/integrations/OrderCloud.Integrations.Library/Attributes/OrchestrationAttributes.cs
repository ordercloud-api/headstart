using System;

namespace OrderCloud.Integrations.Library.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class OrchestrationIgnoreAttribute : Attribute
    {
    }
}
