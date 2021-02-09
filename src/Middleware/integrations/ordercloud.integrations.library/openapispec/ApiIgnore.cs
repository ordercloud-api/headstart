using System;
using System.Collections.Generic;
using System.Text;

namespace ordercloud.integrations.library
{
    ///// <summary>
    ///// Use to indicate that the property can't be set through the API. Use for things like OrderStatus and DateCreated.
    ///// (Can't just make property read-only because we need to set it when mapping from business object.)
    ///// </summary>
    //[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    //public class ApiReadOnlyAttribute : Attribute { }

    /////// <summary>
    /////// Use to indicate that the property can't be read through the API. Rarely used, passwords and credit card #s might be all.
    /////// </summary>
    //[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    //public class ApiWriteOnlyAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class ApiIgnoreAttribute : Attribute
    {
    }
}
