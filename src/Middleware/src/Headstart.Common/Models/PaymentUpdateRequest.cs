using Headstart.Models.Headstart;
using System.Collections.Generic;

namespace Headstart.Common.Models
{
	public class PaymentUpdateRequest
	{
		public List<HSPayment> Payments { get; set; }
	}
}
