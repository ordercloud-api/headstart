using System;

namespace Headstart.Common.Services.CMS.Models
{
	public class History
	{
		public DateTimeOffset DateCreated { get; set; }

		public string CreatedByUserIs { get; set; } = string.Empty;

		public DateTimeOffset DateUpdated { get; set; }

		public string UpdatedByUserIs { get; set; } = string.Empty;
	}
}