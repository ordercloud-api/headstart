using System.Collections.Generic;
using Newtonsoft.Json;

namespace Headstart.Common.Services.Zoho.Models
{
    public class ZohoItemList : ZohoListResponse
    {
        [JsonProperty(PropertyName = "items")]
        public List<ZohoLineItem> Items { get; set; }
    }
    public class ZohoLineItem
    {

        public string item_id { get; set; }
        public string line_item_id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        //public double item_order { get; set; } // debug removal
        //public double bcy_rate { get; set; } // debug removal
        public double rate { get; set; }
        public double? purchase_rate { get; set; }
        public double? quantity { get; set; }
        public string project_id { get; set; }
        public string time_entry_ids { get; set; }
        public string expense_id { get; set; }
        public string expense_receipt_name { get; set; }
        public string unit { get; set; }

        // discount fields resulting in errror "Discount after tax cannot be applied when discount is given at the item level"
        // reevaluation fields when handling promotions
        //public double? discount_amount { get; set; }
        //public string discount { get; set; }
        //public string tax_id { get; set; }
        //public string tax_name { get; set; }
        //public string tax_type { get; set; }
        //public double? tax_percentage { get; set; }
        public double? item_total { get; set; }
        //public string account_id { get; set; }
        public string account_name { get; set; }
        public string line_id { get; set; }
        public string debit_or_credit { get; set; }
        //public double amount { get; set; }
        public string status { get; set; }
        public string tax_authority_id { get; set; }
        public string tax_exemption_id { get; set; }
        //public bool is_invoiced { get; set; }
        public List<string> tags { get; set; }
        public string source { get; set; }
        public bool? is_linked_with_zohocrm { get; set; }
        public double? pricebook_rate { get; set; }
        public bool? is_taxable { get; set; }
        public string stock_on_hand { get; set; }
        //public string purchase_account_id { get; set; }
        public string item_type { get; set; }
        public string sku { get; set; }
        public string purchase_description { get; set; }
        public double? discount { get; set; }
        public string avatax_tax_code { get; set; }
        // adding additional fields
        public string brand { get; set; }
        public string manufacturer { get; set; }
        public double sales_rate { get; set; }
        public string purchase_account_name { get; set; }
        public string created_time { get; set; }
        public string last_modified_time { get; set; }
        public string avatax_use_code_id { get; set; }
        public string avatax_use_code_desc { get; set; }
        public string avatax_tax_code_id { get; set; }
        public string avatax_tax_code_desc { get; set; }
        public string minimum_order_quantity { get; set; }
        public string maximum_order_quantity { get; set; }
        public string initial_stock { get; set; }
        public string initial_stock_rate { get; set; }
        public string vendor_id { get; set; }
        public string vendor_name { get; set; }
        public string upc { get; set; }
        public string isbn { get; set; }
        public string part_number { get; set; }
        public bool is_combo_product { get; set; }
        public object[] sales_channels { get; set; }
        public object[] preferred_vendors { get; set; }
    }
}
