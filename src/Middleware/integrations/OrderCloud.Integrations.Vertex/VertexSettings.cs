namespace OrderCloud.Integrations.Vertex
{
    public class VertexSettings
    {
        /// <summary>
        /// The Vertex company name
        /// Required when EnvironmentSettings:TaxProvider is "Vertex".
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// The Vertex client id
        /// Required when EnvironmentSettings:TaxProvider is "Vertex".
        /// </summary>
        public string ClientID { get; set; }

        /// <summary>
        /// The Vertex client secret
        /// Required when EnvironmentSettings:TaxProvider is "Vertex".
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// The Vertex username
        /// Required when EnvironmentSettings:TaxProvider is "Vertex".
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The Vertex password
        /// Required when EnvironmentSettings:TaxProvider is "Vertex".
        /// </summary>
        public string Password { get; set; }
    }
}
