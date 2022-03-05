namespace Headstart.Common.Services.Zoho.Models
{
    public class ZohoTax
    {
        public string tax_name { get; set; } = string.Empty;

        public double tax_amount { get; set; }

        public string tax_id { get; set; } = string.Empty;

        public double tax_percentage { get; set; }

        public string tax_type { get; set; } = string.Empty;

        public bool is_compound { get; set; }

        public bool is_default_tax { get; set; }
    }
}