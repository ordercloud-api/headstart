using System;
using System.Collections.Generic;
using System.Text;

namespace ordercloud.integrations.library
{
    /// <summary>
    /// Use to override the default test used for resources (i.e. "Cost Centers") and endpoints (i.e. "Get a Single Cost Center").
    /// Apply at the controller or action level, respectively.
    /// </summary>
    public class DocNameAttribute : Attribute
    {
        public DocNameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }
    }
}
