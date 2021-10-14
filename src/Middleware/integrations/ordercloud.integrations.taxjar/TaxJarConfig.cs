using System;

namespace ordercloud.integrations.taxjar
{
	public class TaxJarConfig
	{
		public TaxJarEnvironment Environment { get; set; }
		public string ApiKey { get; set; }
	}

	public enum TaxJarEnvironment { Sandbox, Production }
}
