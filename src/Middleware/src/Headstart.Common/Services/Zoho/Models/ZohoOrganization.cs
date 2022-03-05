using Newtonsoft.Json;
using System.Collections.Generic;

namespace Headstart.Common.Services.Zoho.Models
{
    public class ZohoOrganizationList : ZohoListResponse
    {
        [JsonProperty(PropertyName = "organizations")]
        public List<ZohoOrganization> Items { get; set; } = new List<ZohoOrganization>();
    }

    public class ZohoOrganization
    {
        public string organization_id { get; set; } = string.Empty;

        public string name { get; set; } = string.Empty;

        public string contact_name { get; set; } = string.Empty;

        public string email { get; set; } = string.Empty;

        public bool is_default_org { get; set; }

        public int plan_type { get; set; }

        public bool tax_group_enabled { get; set; }

        public string plan_name { get; set; } = string.Empty;

        public string plan_period { get; set; } = string.Empty;

        public string language_code { get; set; } = string.Empty;

        public string fiscal_year_start_month { get; set; } = string.Empty;

        public string account_created_date { get; set; } = string.Empty;

        public string account_created_date_formatted { get; set; } = string.Empty;

        public string time_zone { get; set; } = string.Empty;

        public bool is_org_active { get; set; }

        public string currency_id { get; set; } = string.Empty;

        public string currency_code { get; set; } = string.Empty;

        public string currency_symbol { get; set; } = string.Empty;

        public string currency_format { get; set; } = string.Empty;

        public int price_precision { get; set; }

        public string date_format { get; set; } = string.Empty;

        public string field_separator { get; set; } = string.Empty;

        public string industry_type { get; set; } = string.Empty;

        public string industry_size { get; set; } = string.Empty;

        public string company_id_label { get; set; } = string.Empty;

        public string company_id_value { get; set; } = string.Empty;

        public string tax_id_label { get; set; } = string.Empty;

        public string tax_id_value { get; set; } = string.Empty;

        public ZohoAddress address { get; set; } = new ZohoAddress();

        public string org_address { get; set; } = string.Empty;

        public string remit_to_address { get; set; } = string.Empty;

        public string phone { get; set; } = string.Empty;

        public string fax { get; set; } = string.Empty;

        public string website { get; set; } = string.Empty;

        public string tax_basis { get; set; } = string.Empty;

        public string is_logo_uploaded { get; set; } = string.Empty;

        public string user_role { get; set; } = string.Empty;

        public string user_status { get; set; } = string.Empty;

        public string unverified_email { get; set; } = string.Empty;

        public string is_transaction_available { get; set; } = string.Empty;

        public string show_org_address_as_one_field { get; set; } = string.Empty;

        public string companyid_label { get; set; } = string.Empty;

        public string companyid_value { get; set; } = string.Empty;

        public string taxid_label { get; set; } = string.Empty;

        public string taxid_value { get; set; } = string.Empty;

        public string value { get; set; } = string.Empty;

        public int source { get; set; }

        public string version { get; set; } = string.Empty;

        public bool is_trial_expired { get; set; }

        public string role_id { get; set; } = string.Empty;

        public bool is_trial_period_extended { get; set; }

        public List<ZohoCustomFields> custom_fields { get; set; } = new List<ZohoCustomFields>();

        public bool is_new_customer_custom_fields { get; set; }

        public bool is_portal_enabled { get; set; }

        public string portal_name { get; set; } = string.Empty;
    }
}
