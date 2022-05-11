namespace Headstart.Common.Services.Zoho.Models
{
    /// <summary>
    /// Used to define the model object of ZohoPageContext.
    /// </summary>
    public class ZohoPageContext
    {
        public int page { get; set; }
        public int per_page { get; set; }
        public bool has_more_page { get; set; }
        public string report_name { get; set; }
        public string applied_filter { get; set; }
        public object[] custom_fields { get; set; }
        public string sort_column { get; set; }
        public string sort_order { get; set; }
    }
}
