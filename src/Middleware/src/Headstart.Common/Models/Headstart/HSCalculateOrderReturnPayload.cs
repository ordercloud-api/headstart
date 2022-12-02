using System;
using System.Collections.Generic;
using System.Text;

namespace Headstart.Common.Models
{
    public class HSCalculateOrderReturnPayload
    {
        public HSOrderReturn OrderReturn { get; set; }

        public HSOrderWorksheet OrderWorksheet { get; set; }
    }
}
