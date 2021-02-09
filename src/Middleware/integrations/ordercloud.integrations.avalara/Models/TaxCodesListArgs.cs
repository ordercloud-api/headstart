using System;
using System.Collections.Generic;
using System.Text;

namespace ordercloud.integrations.avalara
{
	public class TaxCodesListArgs
	{
		public int Top { get; set; }
		public int Skip { get; set; }
		public string Filter { get; set; }
		public string OrderBy { get; set; }
		public string CodeCategory { get; set; }
	}
}
