using System.Collections.Generic;
using Headstart.Common.Models.Headstart;

namespace Headstart.Common.Models
{
	public class PaymentUpdateRequest
	{
		public List<HsPayment> Payments { get; set; } = new List<HsPayment>();
	}
}