using OrderCloud.SDK;

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
