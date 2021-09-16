using System;
using System.Collections.Generic;
using System.Text;

namespace ordercloud.integrations.library
{
    /// <summary>
    /// Allows MeController to be documented as multiple resources. example: Me.ListAddresses is documented under UserPerspective/Addresses
    /// </summary>
    public class DocSubResourceAttribute : Attribute
    {
        public DocSubResourceAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }
    }
}
