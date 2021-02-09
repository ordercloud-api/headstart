using System;
using System.Collections.Generic;
using System.Text;

namespace ordercloud.integrations.easypost
{
	public class EasyPostCarrierAccount
	{
        public string id { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public string type { get; set; }
        public string description { get; set; }
        public string reference { get; set; }
        public string readable { get; set; }
        public Dictionary<string, object> credentials { get; set; }
        public Dictionary<string, object> test_credentials { get; set; }
    }
}
