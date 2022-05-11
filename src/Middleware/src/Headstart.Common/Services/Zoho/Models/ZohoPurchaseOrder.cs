using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Headstart.Common.Services.Zoho.Models
{
    public class ZohoPurchaseOrderList : ZohoListResponse
    {
        [JsonProperty(PropertyName = "purchaseorders")]
        public List<ZohoPurchaseOrder> Items { get; set; }
    }

    public class ZohoPurchaseOrder
    {
        public string salesorder_id { get; set; }
        public string purchaseorder_id { get; set; }
        public string vendor_id { get; set; }
        public string vendor_name { get; set; }
        public string status { get; set; }
        public string purchaseorder_number { get; set; }
        public string reference_number { get; set; }
        public string date { get; set; }
        public string delivery_date { get; set; }
        public string currency_id { get; set; }
        public string currency_code { get; set; }
        public int price_precision { get; set; }
        public double total { get; set; }
        public string created_time { get; set; }
        public string last_modified_time { get; set; }
        public string expected_delivery_date { get; set; }
        public List<string> contact_persons { get; set; }
        public string currency_symbol { get; set; }
        public double exchange_rate { get; set; }
        public bool is_emailed { get; set; }
        public List<ZohoLineItem> line_items { get; set; }
        public double sub_total { get; set; }
        public ZohoAddress billing_address { get; set; }
        public string notes { get; set; }
        public string terms { get; set; }
        public string ship_via { get; set; }
        public string attention { get; set; }
        public ZohoAddress delivery_address { get; set; }
        public List<ZohoCustomFields> custom_fields { get; set; }
        public string attachment_name { get; set; }
        public bool can_send_in_mail { get; set; }
        public string template_id { get; set; }
        public string template_name { get; set; }
        public double tax_total { get; set; }
        public List<ZohoTax> taxes { get; set; }
        public string ship_via_id { get; set; }
        public string delivery_org_address_id { get; set; }
        public string delivery_customer_id { get; set; }
        public string template_type { get; set; }
        public bool can_mark_as_bill { get; set; }
        public bool can_mark_as_unbill { get; set; }
    }
}
