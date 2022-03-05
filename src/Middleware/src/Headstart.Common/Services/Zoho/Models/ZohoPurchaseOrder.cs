using Newtonsoft.Json;
using System.Collections.Generic;

namespace Headstart.Common.Services.Zoho.Models
{
    public class ZohoPurchaseOrderList : ZohoListResponse
    {
        [JsonProperty(PropertyName = "purchaseorders")]
        public List<ZohoPurchaseOrder> Items { get; set; } = new List<ZohoPurchaseOrder>();
    }

    public class ZohoPurchaseOrder
    {
        public string salesorder_id { get; set; } = string.Empty;

        public string purchaseorder_id { get; set; } = string.Empty;

        public string vendor_id { get; set; } = string.Empty;

        public string vendor_name { get; set; } = string.Empty;

        public string status { get; set; } = string.Empty;

        public string purchaseorder_number { get; set; } = string.Empty;

        public string reference_number { get; set; } = string.Empty;

        public string date { get; set; } = string.Empty;

        public string delivery_date { get; set; } = string.Empty;

        public string currency_id { get; set; } = string.Empty;

        public string currency_code { get; set; } = string.Empty;

        public int price_precision { get; set; }

        public double total { get; set; }

        public string created_time { get; set; } = string.Empty;

        public string last_modified_time { get; set; } = string.Empty;

        public string expected_delivery_date { get; set; } = string.Empty;

        public List<string> contact_persons { get; set; } = new List<string>();

        public string currency_symbol { get; set; } = string.Empty;

        public double exchange_rate { get; set; }

        public bool is_emailed { get; set; }

        public List<ZohoLineItem> line_items { get; set; } = new List<ZohoLineItem>();

        public double sub_total { get; set; }

        public ZohoAddress billing_address { get; set; } = new ZohoAddress();

        public string notes { get; set; } = string.Empty;

        public string terms { get; set; } = string.Empty;

        public string ship_via { get; set; } = string.Empty;

        public string attention { get; set; } = string.Empty;

        public ZohoAddress delivery_address { get; set; } = new ZohoAddress();

        public List<ZohoCustomFields> custom_fields { get; set; } = new List<ZohoCustomFields>();

        public string attachment_name { get; set; } = string.Empty;

        public bool can_send_in_mail { get; set; }

        public string template_id { get; set; } = string.Empty;

        public string template_name { get; set; } = string.Empty;

        public double tax_total { get; set; }

        public List<ZohoTax> taxes { get; set; } = new List<ZohoTax>();

        public string ship_via_id { get; set; } = string.Empty;

        public string delivery_org_address_id { get; set; } = string.Empty;

        public string delivery_customer_id { get; set; } = string.Empty;

        public string template_type { get; set; } = string.Empty;

        public bool can_mark_as_bill { get; set; }

        public bool can_mark_as_unbill { get; set; }
    }
}