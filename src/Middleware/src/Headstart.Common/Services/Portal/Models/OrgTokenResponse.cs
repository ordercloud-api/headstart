using System;
using System.Collections.Generic;
using System.Text;

namespace Headstart.Common.Services.Portal.Models
{
    public class MarketplaceTokenResponse
    {
		public string[] coreuser_roles { get; set; }
		public string[] portaluser_roles { get; set; }
		public string[] granted_roles { get; set; }
		public string access_token { get; set; }
		public string refresh_token { get; set; }
		public string token_type { get => "bearer"; }
		public int expires_in { get; set; }
	}
}
