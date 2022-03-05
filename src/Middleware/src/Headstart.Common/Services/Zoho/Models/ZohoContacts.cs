using Newtonsoft.Json;
using System.Collections.Generic;

namespace Headstart.Common.Services.Zoho.Models
{
    public class ZohoContactList : ZohoListResponse
    {
        [JsonProperty(PropertyName = "contacts")]
        public List<ZohoContact> Items { get; set; } = new List<ZohoContact>();
    }

    public class ZohoSingleContact : ZohoListResponse
    {
        [JsonProperty(PropertyName = "contact")]
        public ZohoContact Item { get; set; } = new ZohoContact();
    }

    public class ZohoContact
    {
        public string contact_id { get; set; } = string.Empty;

        public string contact_name { get; set; } = string.Empty;

        public string company_name { get; set; } = string.Empty;

        public string website { get; set; } = string.Empty;

        public string contact_type { get; set; } = string.Empty;

        public bool is_portal_enabled { get; set; }

        public string currency_id { get; set; } = string.Empty;

        public string currency_code { get; set; } = string.Empty; // added during order mapping

        public string currency_symbol { get; set; } = string.Empty; // added during order mapping

        public int payment_terms { get; set; }

        public string payment_terms_label { get; set; } = string.Empty;

        public string notes { get; set; } = string.Empty;

        public ZohoAddress billing_address { get; set; } = new ZohoAddress();

        public ZohoAddress shipping_address { get; set; } = new ZohoAddress();

        public List<ZohoContactPerson> contact_persons { get; set; } = new List<ZohoContactPerson>();

        public ZohoDefaultTemplates default_templates { get; set; } = new ZohoDefaultTemplates();

        public List<ZohoCustomFields> custom_fields { get; set; } = new List<ZohoCustomFields>();

        public string owner_id { get; set; } = string.Empty;

        public string tax_exemption_id { get; set; } = string.Empty;

        public string tax_authority_id { get; set; } = string.Empty;

        public string tax_id { get; set; } = string.Empty;

        // commenting out this field for now, call currently fails when going into zoho now
        // some configuration changes in zoho have the potential to allow us to use this
        // but it's not entirely clear what is needed to be done
        //public bool is_taxable { get; set; } = true;

        public string facebook { get; set; } = string.Empty;
        public string twitter { get; set; } = string.Empty;
    }
}