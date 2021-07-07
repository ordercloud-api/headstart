using Headstart.Models.Headstart;
using ordercloud.integrations.library;
using OrderCloud.SDK;
using System;
using System.Collections.Generic;
using System.Text;

namespace Headstart.Common.Models
{
    public class PaymentUpdateRequest
    {
        public List<HSPayment> Payments { get; set; }
    }
}
