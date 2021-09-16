using System;
using System.Collections.Generic;
using System.Text;

namespace ordercloud.integrations.library
{
    [AttributeUsage(AttributeTargets.Class)]
    public abstract class DocSection : Attribute
    {
        public string ID { get; }
        public int ListOrder { get; set; }

        protected DocSection()
        {
            ID = GetType().Name.Replace("Attribute", "");
            ListOrder = int.MaxValue;
        }
    }
}
