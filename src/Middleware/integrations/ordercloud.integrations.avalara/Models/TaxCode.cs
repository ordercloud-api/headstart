using ordercloud.integrations.library;
using System;
using System.Collections.Generic;
using System.Text;

namespace ordercloud.integrations.avalara
{
	[SwaggerModel]
	public class TaxCode
	{
		public string Category { get; set; }
		public string Code { get; set; }
		public string Description { get; set; }
	}
}
