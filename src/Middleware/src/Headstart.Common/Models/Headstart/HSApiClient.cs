using OrderCloud.SDK;
using System;
using System.Collections.Generic;
using System.Text;

namespace Headstart.Common.Models
{
    public class HSApiClient : ApiClient<ApiClientXP>
    {
    }

    public class ApiClientXP
    {
        public bool IsStorefront { get; set; }
    }
}
