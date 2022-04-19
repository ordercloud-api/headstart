namespace Headstart.Common.Services.Zoho.Models
{
	public class ZohoPageContext
	{
		public int page { get; set; }

		public int per_page { get; set; }

		public bool has_more_page { get; set; }

		public string report_name { get; set; } = string.Empty;

		public string applied_filter { get; set; } = string.Empty;

		public object[] custom_fields { get; set; }

		public string sort_column { get; set; } = string.Empty;

		public string sort_order { get; set; } = string.Empty;
	}
}