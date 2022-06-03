namespace Headstart.Common.Settings
{
    public enum AppEnvironment
    {
        Test,
        Staging,
        Production,
    }

    public enum ShippingProvider
    {
        EasyPost,
        Custom,
    }

    public enum TaxProvider
    {
        Avalara,
        Vertex,
        Taxjar,
    }

    public class EnvironmentSettings
    {
        public string BuildNumber { get; set; } // set during deploy

        public string Commit { get; set; } // set during deploy

        public AppEnvironment Environment { get; set; }

        public string MiddlewareBaseUrl { get; set; }

        public ShippingProvider ShippingProvider { get; set; } = ShippingProvider.EasyPost;

        public TaxProvider TaxProvider { get; set; } = TaxProvider.Avalara;
    }
}
