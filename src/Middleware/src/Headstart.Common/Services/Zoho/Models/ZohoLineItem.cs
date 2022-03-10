using Newtonsoft.Json;
using System.Collections.Generic;

namespace Headstart.Common.Services.Zoho.Models
{
	public class ZohoItemList : ZohoListResponse
	{
		[JsonProperty(PropertyName = "items")]
		public List<ZohoLineItem> Items { get; set; } = new List<ZohoLineItem>();
	}

	public class ZohoLineItem
	{
		public string item_id { get; set; } = string.Empty;

		public string line_item_id { get; set; } = string.Empty;

		public string name { get; set; } = string.Empty;

		public string description { get; set; } = string.Empty;

		public double rate { get; set; }

		public double? purchase_rate { get; set; }

		public double? quantity { get; set; }

		public string project_id { get; set; } = string.Empty;

		public string time_entry_ids { get; set; } = string.Empty;

		public string expense_id { get; set; } = string.Empty;

		public string expense_receipt_name { get; set; } = string.Empty;

		public string unit { get; set; } = string.Empty;

		// discount fields resulting in errror "Discount after tax cannot be applied when discount is given at the item level"
		// reevaluation fields when handling promotions
		//public double? discount_amount { get; set; }

		//public string discount { get; set; } = string.Empty;

		//public string tax_id { get; set; } = string.Empty;

		//public string tax_name { get; set; } = string.Empty;

		//public string tax_type { get; set; } = string.Empty;

		//public double? tax_percentage { get; set; }

		public double? item_total { get; set; }

		public string account_name { get; set; } = string.Empty;

		public string line_id { get; set; } = string.Empty;

		public string debit_or_credit { get; set; } = string.Empty;

		public string status { get; set; } = string.Empty;

		public string tax_authority_id { get; set; } = string.Empty;

		public string tax_exemption_id { get; set; } = string.Empty;

		public List<string> tags { get; set; } = new List<string>();

		public string source { get; set; } = string.Empty;

		public bool? is_linked_with_zohocrm { get; set; }

		public double? pricebook_rate { get; set; }

		public bool? is_taxable { get; set; }

		public string stock_on_hand { get; set; } = string.Empty;

		public string item_type { get; set; } = string.Empty;

		public string sku { get; set; } = string.Empty;

		public string purchase_description { get; set; } = string.Empty;

		public double? discount { get; set; }

		public string avatax_tax_code { get; set; } = string.Empty;

		public string brand { get; set; } = string.Empty;

		public string manufacturer { get; set; } = string.Empty;

		public double sales_rate { get; set; }

		public string purchase_account_name { get; set; } = string.Empty;

		public string created_time { get; set; } = string.Empty;

		public string last_modified_time { get; set; } = string.Empty;

		public string avatax_use_code_id { get; set; } = string.Empty;

		public string avatax_use_code_desc { get; set; } = string.Empty;

		public string avatax_tax_code_id { get; set; } = string.Empty;

		public string avatax_tax_code_desc { get; set; } = string.Empty;

		public string minimum_order_quantity { get; set; } = string.Empty;

		public string maximum_order_quantity { get; set; } = string.Empty;

		public string initial_stock { get; set; } = string.Empty;

		public string initial_stock_rate { get; set; } = string.Empty;

		public string vendor_id { get; set; } = string.Empty;

		public string vendor_name { get; set; } = string.Empty;

		public string upc { get; set; } = string.Empty;

		public string isbn { get; set; } = string.Empty;

		public string part_number { get; set; } = string.Empty;

		public bool is_combo_product { get; set; }

		public object[] sales_channels { get; set; }

		public object[] preferred_vendors { get; set; }
	}
}