namespace ordercloud.integrations.taxjar
{
    public enum TaxJarEnvironment
    {
        Sandbox,
        Production,
    }

    public class TaxJarConfig
    {
        public TaxJarEnvironment Environment { get; set; }

        public string ApiKey { get; set; }
    }
}
