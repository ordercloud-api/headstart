namespace OrderCloud.Integrations.TaxJar
{
    public enum TaxJarEnvironment
    {
        Sandbox,
        Production,
    }

    public class TaxJarSettings
    {
        /// <summary>
        /// The TaxJar environment, "Sandbox" or "Production".
        /// </summary>
        public TaxJarEnvironment Environment { get; set; }

        /// <summary>
        /// The TaxJar API key
        /// Required if EnvironmentSettings:TaxProvider is set to "TaxJar"
        /// </summary>
        public string ApiKey { get; set; }
    }
}
