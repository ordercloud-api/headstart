using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ordercloud.integrations.library
{
    public class ApiMetaData
    {
        public IList<ApiSection> Sections { get; set; }
        public IEnumerable<ApiResource> Resources => Sections.SelectMany(a => a.Resources).OrderBy(r => r.Name);
        public IList<ApiModel> Models { get; set; }
        public IList<ApiEnum> Enums { get; set; }
        public IList<OpenApiSpecErrorCode> ErrorCodes { get; set; }
        public IList<string> Roles { get; set; }
    }
}
