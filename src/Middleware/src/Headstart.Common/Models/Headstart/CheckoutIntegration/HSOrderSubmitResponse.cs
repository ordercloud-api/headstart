using System.Collections.Generic;
using OrderCloud.SDK;

namespace Headstart.Common.Models
{
    public class HSOrderSubmitResponse : OrderSubmitResponse<OrderSubmitResponseXp>
    {
    }

    public class OrderSubmitResponseXp
    {
        public List<ProcessResult> ProcessResults { get; set; }
    }
}
