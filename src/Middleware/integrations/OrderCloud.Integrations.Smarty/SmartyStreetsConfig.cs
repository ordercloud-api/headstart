namespace OrderCloud.Integrations.Smarty
{
    public class SmartyStreetSettings
    {
        /// <summary>
        /// Authentication ID used to connect with SmartyStreets
        /// Required if EnvironmentSettings:AddressValidationProvider is set to "Smarty".
        /// </summary>
        public string AuthID { get; set; }

        /// <summary>
        /// Authorization token used to connect with SmartyStreet
        /// Required if EnvironmentSettings:AddressValidationProvider is set to "Smarty".
        /// </summary>
        public string AuthToken { get; set; }
    }
}
