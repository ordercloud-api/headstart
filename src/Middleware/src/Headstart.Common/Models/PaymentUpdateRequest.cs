using System.Collections.Generic;
using Headstart.Models.Headstart;

namespace Headstart.Common.Models
{
    public class PaymentUpdateRequest
    {
        public List<HSPayment> Payments { get; set; }
    }
}
