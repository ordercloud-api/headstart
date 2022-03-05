using Newtonsoft.Json;
using System.Collections.Generic;

namespace Headstart.Common.Services.Zoho.Models
{
    public class ZohoSalesOrderList : ZohoListResponse
    {
        [JsonProperty(PropertyName = "salesorders")]
        public List<ZohoSalesOrder> Items { get; set; } = new List<ZohoSalesOrder>();
    }

    public class ZohoSalesOrder
    {
        public string salesorder_id { get; set; } = string.Empty;

        public string customer_name { get; set; } = string.Empty;

        public string customer_id { get; set; } = string.Empty;

        public string status { get; set; } = string.Empty;

        public string salesorder_number { get; set; } = string.Empty;

        public string reference_number { get; set; } = string.Empty;

        public string date { get; set; } = string.Empty;

        public string shipment_date { get; set; } = string.Empty;

        public string shipment_days { get; set; } = string.Empty;

        public string currency_id { get; set; } = string.Empty;

        public string currency_code { get; set; } = string.Empty;

        public double total { get; set; }

        public double bcy_total { get; set; }

        public string created_time { get; set; } = string.Empty;

        public string last_modified_time { get; set; } = string.Empty;

        public bool is_emailed { get; set; }

        public List<string> contact_persons { get; set; } = new List<string>();

        public string currency_symbol { get; set; } = string.Empty;

        public string estimate_id { get; set; } = string.Empty;

        public string delivery_method { get; set; } = string.Empty;

        public string delivery_method_id { get; set; } = string.Empty;

        public List<ZohoLineItem> line_items { get; set; } = new List<ZohoLineItem>();

        public double shipping_charge { get; set; }

        public double adjustment { get; set; }

        public string adjustment_description { get; set; } = string.Empty;

        public double sub_total { get; set; }

        public double tax_total { get; set; }

        public List<ZohoTax> taxes { get; set; } = new List<ZohoTax>();

        public int price_precision { get; set; }

        public ZohoAddress billing_address { get; set; } = new ZohoAddress();

        public string billing_address_id { get; set; } = string.Empty;

        public ZohoAddress shipping_address { get; set; } = new ZohoAddress();

        public string notes { get; set; } = string.Empty;

        public string terms { get; set; } = string.Empty;

        public List<ZohoCustomFields> custom_fields { get; set; } = new List<ZohoCustomFields>();

        public string template_id { get; set; } = string.Empty;

        public string template_name { get; set; } = string.Empty;

        public string template_type { get; set; } = string.Empty;

        public string attachment_name { get; set; } = string.Empty;

        public bool can_send_in_mail { get; set; }

        public string salesperson_id { get; set; } = string.Empty;

        public string salesperson_name { get; set; } = string.Empty;

        public bool is_discount_before_tax { get; set; }

        public double discount_amount { get; set; }

        public string discount_type { get; set; } = string.Empty;

        public double discount { get; set; }
    }
}