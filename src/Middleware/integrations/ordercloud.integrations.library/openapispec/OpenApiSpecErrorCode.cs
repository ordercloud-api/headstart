using System;
using System.Collections.Generic;
using System.Text;

namespace ordercloud.integrations.library
{
    public class OpenApiSpecErrorCode
    {
        public string FullCode { get; set; }
        public string Category => FullCode.Split('.')[0];
        public string Name => FullCode.Split('.')[1];
        public string Description { get; set; }
    }
}
