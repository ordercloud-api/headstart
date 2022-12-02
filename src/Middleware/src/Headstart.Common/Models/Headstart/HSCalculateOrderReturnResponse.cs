using System;
using System.Collections.Generic;
using System.Text;
using OrderCloud.Catalyst;
using OrderCloud.SDK;

namespace Headstart.Common.Models
{
    public class HSCalculateOrderReturnResponse
    {
        public decimal RefundAmount { get; set; }

        public IEnumerable<LineItemReturnCalculation> ItemsToReturnCalcs { get; set; }
    }
}
