using System;

namespace Headstart.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class OrchestrationIgnoreAttribute : Attribute
    {
    }
}
