using System;
using System.Collections.Generic;
using System.Text;

namespace ordercloud.integrations.library
{
    public class DocType : Attribute
    {
        public string TypeName { get; protected set; }

        private DocType()
        {
            TypeName = GetType().Name.Replace("Attribute", "").ToLower();
        }

        public class StringAttribute : DocType { }
        public class IntegerAttribute : DocType { }
        public class DateAttribute : DocType { }
        public class ArrayAttribute : DocType { }
        public class ObjectAttribute : DocType { }
        public class NoneAttribute : DocType
        {
            public NoneAttribute()
            {
                TypeName = "";
            }
        }
    }
}
