using System.Collections.Generic;
using Newtonsoft.Json;

namespace Headstart.Common.Services.Zoho.Models
{
    public class ZohoSalesOrderList : ZohoListResponse
    {
        [JsonProperty(PropertyName = "salesorders")]
        public List<ZohoSalesOrder> Items { get; set; }
    }

    public class ZohoSalesOrder
    {
        public string salesorder_id { get; set; }
        public string customer_name { get; set; }
        public string customer_id { get; set; }
        public string status { get; set; }
        public string salesorder_number { get; set; }
        public string reference_number { get; set; }
        public string date { get; set; }
        public string shipment_date { get; set; }
        public string shipment_days { get; set; }
        public string currency_id { get; set; }
        public string currency_code { get; set; }
        public double total { get; set; }
        public double bcy_total { get; set; }
        public string created_time { get; set; }
        public string last_modified_time { get; set; }
        public bool is_emailed { get; set; }
        public List<string> contact_persons { get; set; }
        public string currency_symbol { get; set; }
        //public double exchange_rate { get; set; }

        // discount fields resulting in errror "Discount after tax cannot be applied when discount is given at the item level"
        // reevaluation fields when handling promotions
        //public decimal discount_amount { get; set; }
        //public decimal discount { get; set; }
        //public bool is_discount_before_tax { get; set; }

        // discount fields resulting in errror "Discount after tax cannot be applied when discount is given at the item level"
        // reevaluation fields when handling promotions
        //public string discount_type { get; set; }
        public string estimate_id { get; set; }
        public string delivery_method { get; set; }
        public string delivery_method_id { get; set; }
        public List<ZohoLineItem> line_items { get; set; }
        public double shipping_charge { get; set; }
        public double adjustment { get; set; }
        public string adjustment_description { get; set; }
        public double sub_total { get; set; }
        public double tax_total { get; set; }
        public List<ZohoTax> taxes { get; set; }
        public int price_precision { get; set; }
        public ZohoAddress billing_address { get; set; }
        public string billing_address_id { get; set; }
        public ZohoAddress shipping_address { get; set; }
        public string notes { get; set; }
        public string terms { get; set; }
        public List<ZohoCustomFields> custom_fields { get; set; }
        public string template_id { get; set; }
        public string template_name { get; set; }
        public string template_type { get; set; }
        public string attachment_name { get; set; }
        public bool can_send_in_mail { get; set; }
        public string salesperson_id { get; set; }
        public string salesperson_name { get; set; }
        public bool is_discount_before_tax { get; set; }
        public double discount_amount { get; set; }
        public string discount_type { get; set; }
        public double discount { get; set; }
    }
}
