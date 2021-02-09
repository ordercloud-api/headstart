using System;
using System.Collections.Generic;
using System.Text;

namespace ordercloud.integrations.smartystreets
{
	public class SmartyStreetsConfig
	{
		public string AuthID { get; set; }
		public string AuthToken { get; set; }
		public string RefererHost { get; set; } // The autocomplete pro endpoint requires the Referer header to be a pre-set value 
		public string WebsiteKey { get; set; }
	}
}
