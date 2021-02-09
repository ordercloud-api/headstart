using ordercloud.integrations.library;
using OrderCloud.SDK;
using System;
using System.Collections.Generic;
using System.Text;

namespace ordercloud.integrations.avalara
{
	[SwaggerModel]
	public class TaxCertificate
	{
		[ApiReadOnly]
		public int ID { get; set; }
		[Required]
		public DateTimeOffset SignedDate { get; set; }
		[Required]
		public DateTimeOffset ExpirationDate { get; set; }
		// Typically state, e.g. 'Michigan'
		// https://developer.avalara.com/api-reference/avatax/rest/v2/models/ExposureZoneModel/
		[Required]
		public string ExposureZoneName { get; set; }
		[Required]
		public string Base64UrlEncodedPDF { get; set; }
		[ApiReadOnly]
		public string FileName { get; set; }
		public string ExemptionNumber { get; set; }
		[ApiReadOnly]
		public bool Expired => ExpirationDate < DateTimeOffset.Now;
	}
}
