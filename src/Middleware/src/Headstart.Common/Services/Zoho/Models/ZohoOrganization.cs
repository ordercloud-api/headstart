using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Headstart.Common.Services.Zoho.Models
{
    public class ZohoOrganizationList : ZohoListResponse
    {
        [JsonProperty(PropertyName = "organizations")]
        public List<ZohoOrganization> Items { get; set; }
    }
    public class ZohoOrganization
    {
        public string organization_id { get; set; }
        public string name { get; set; }
        public string contact_name { get; set; }
        public string email { get; set; }
        public bool is_default_org { get; set; }
        public int plan_type { get; set; }
        public bool tax_group_enabled { get; set; }
        public string plan_name { get; set; }
        public string plan_period { get; set; }
        public string language_code { get; set; }
        public string fiscal_year_start_month { get; set; }
        public string account_created_date { get; set; }
        public string account_created_date_formatted { get; set; }
        public string time_zone { get; set; }
        public bool is_org_active { get; set; }
        public string currency_id { get; set; }
        public string currency_code { get; set; }
        public string currency_symbol { get; set; }
        public string currency_format { get; set; }
        public int price_precision { get; set; }
        public string date_format { get; set; }
        public string field_separator { get; set; }
        public string industry_type { get; set; }
        public string industry_size { get; set; }
        public string company_id_label { get; set; }
        public string company_id_value { get; set; }
        public string tax_id_label { get; set; }
        public string tax_id_value { get; set; }
        public ZohoAddress address { get; set; }
        public string org_address { get; set; }
        public string remit_to_address { get; set; }
        public string phone { get; set; }
        public string fax { get; set; }
        public string website { get; set; }
        public string tax_basis { get; set; }
        public string is_logo_uploaded { get; set; }
        public string user_role { get; set; }
        public string user_status { get; set; }
        public string unverified_email { get; set; }
        public string is_transaction_available { get; set; }
        public string show_org_address_as_one_field { get; set; }
        public string companyid_label { get; set; }
        public string companyid_value { get; set; }
        public string taxid_label { get; set; }
        public string taxid_value { get; set; }
        public string value { get; set; }
        public int source { get; set; }
        public string version { get; set; }
        public bool is_trial_expired { get; set; }
        public string role_id { get; set; }
        public bool is_trial_period_extended { get; set; }
        public List<ZohoCustomFields> custom_fields { get; set; }
        public bool is_new_customer_custom_fields { get; set; }
        public bool is_portal_enabled { get; set; }
        public string portal_name { get; set; }
        bool is_registered_for_tax { get; set; }
        string tax_payment_period { get; set; }
    }
}
