using Avalara.AvaTax.RestClient;
using OrderCloud.SDK;
using System;
using System.Collections.Generic;
using System.Text;

namespace Headstart.Models.Headstart
{
    public class HSOrderCalculateResponse : OrderCalculateResponse<OrderCalculateResponseXp>
    {
    }

    public class OrderCalculateResponseXp
    {
        public TransactionModel TaxResponse { get; set; }
    }
}
