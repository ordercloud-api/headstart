using System;
using System.Collections.Generic;
using System.Text;

namespace ordercloud.integrations.library
{
    public class ApiModel
    {
        public Type Type { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// if this is a list, for example, get the model that it's a list of
        /// </summary>
        public ApiModel InnerModel { get; set; }
        public IList<ApiProperty> Properties { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsWriteOnly { get; set; }
        public bool IsPartial { get; set; }
        public object Sample { get; set; }
    }
}
