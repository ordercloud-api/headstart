using System;
using System.Collections.Generic;
using System.Text;

namespace ordercloud.integrations.library
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SearchableAttribute : Attribute
    {
        public int Priority { get; set; }

        public SearchableAttribute(int priority)
        {
            Priority = priority;
        }
    }
}
