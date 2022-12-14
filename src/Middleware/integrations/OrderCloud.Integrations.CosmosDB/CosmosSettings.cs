namespace OrderCloud.Integrations.CosmosDB
{
    public class CosmosSettings
    {
        /// <summary>
        /// Azure database resource name
        /// Required for running Headstart.
        /// </summary>
        public string DatabaseName { get; set; }

        /// <summary>
        /// Endpoint for your Azure CosmosDB instance. Can be found under Overview > URI. Is in the form https://{COSMOS_DB_ACCOUNT_NAME}.documents.azure.com:443/
        /// Required for running Headstart.
        /// </summary>
        public string EndpointUri { get; set; }

        /// <summary>
        ///  Provides access to all the administrative resources for the database account [Microsoft Reference](https://docs.microsoft.com/en-us/azure/cosmos-db/secure-access-to-data#primary-keys)
        ///  Can be found in Keys > Primary Key.
        ///  Required for running Headstart.
        /// </summary>
        public string PrimaryKey { get; set; }

        /// <summary>
        /// Allows you to control how long a request should take before it times out
        /// Optional - defaults to 15 seconds.
        /// </summary>
        public int RequestTimeoutInSeconds { get; set; } = 15;
    }
}
