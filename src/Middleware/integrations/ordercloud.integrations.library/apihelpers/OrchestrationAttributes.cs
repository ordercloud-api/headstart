using System;

namespace Headstart.Models.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class OrchestrationIgnoreAttribute : Attribute { }
}
