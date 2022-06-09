using System;

namespace Headstart.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SortableAttribute : Attribute
    {
        public SortableAttribute()
        {
        }

        public SortableAttribute(int priority)
        {
            Priority = priority;
        }

        public int? Priority { get; set; }

        public bool Descending { get; set; }
    }
}
