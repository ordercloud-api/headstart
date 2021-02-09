using System;
using System.Collections.Generic;
using System.Text;

namespace ordercloud.integrations.easypost
{
	public class EasyPostRate
	{
        public string id { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public string mode { get; set; }
        public string service { get; set; }
        public string rate { get; set; }
        public string list_rate { get; set; }
        public string retail_rate { get; set; }
        public string currency { get; set; }
        public string list_currency { get; set; }
        public string retail_currency { get; set; }
        public int? est_delivery_days { get; set; }
        public DateTime? delivery_date { get; set; }
        public bool delivery_date_guaranteed { get; set; }
        public int? delivery_days { get; set; }
        public string carrier { get; set; }
        public string shipment_id { get; set; }
        public string carrier_account_id { get; set; }
    }
}
