using ordercloud.integrations.cardconnect;
using ordercloud.integrations.exchangerates;
using ordercloud.integrations.library;
using ordercloud.integrations.smartystreets;

namespace Headstart.Common
{
    [DocIgnore]
    public class AppSettings
    {
        public UI UI { get; set; }
		public EnvironmentSettings EnvironmentSettings { get; set; } = new EnvironmentSettings();
		public ApplicationInsightsSettings ApplicationInsightsSettings { get; set; } = new ApplicationInsightsSettings();
        public AvalaraSettings AvalaraSettings { get; set; }
        public BlobSettings BlobSettings { get; set; }
        public CosmosSettings CosmosSettings { get; set; } = new CosmosSettings();
        public OrderCloudSettings OrderCloudSettings { get; set; } = new OrderCloudSettings();
        public OrderCloudIntegrationsCardConnectConfig CardConnectSettings { get; set; } = new OrderCloudIntegrationsCardConnectConfig();
        public ZohoSettings ZohoSettings { get; set; } = new ZohoSettings();
		public SmartyStreetsConfig SmartyStreetSettings { get; set; } = new SmartyStreetsConfig();
        public EasyPostSettings EasyPostSettings { get; set; } = new EasyPostSettings();
        public SendgridSettings SendgridSettings { get; set; } = new SendgridSettings();
        public FlurlSettings FlurlSettings { get; set; } = new FlurlSettings();
        public CMSSettings CMSSettings { get; set; } = new CMSSettings();
        public AnytimeDashboardSettings AnytimeDashboardSettings { get; set; } = new AnytimeDashboardSettings();
        public WaxDashboardSettings WaxDashboardSettings { get; set; } = new WaxDashboardSettings();

    }

    public class CMSSettings
	{
        public string BaseUrl { get; set; }
	}

    public class UI
    {
        public string BaseAdminUrl { get; set; }
    }

    public class EnvironmentSettings
    {
        public AppEnvironment Environment { get; set; }
        public string BuildNumber { get; set; } // set during deploy
        public string Commit { get; set; } // set during deploy
        public string BaseUrl { get; set; }
        public string AFStorefrontBaseUrl { get; set; }
        public string AFStorefrontClientID { get; set; }
        public string WTCStorefrontBaseUrl { get; set; }
        public string WTCStorefrontClientID { get; set; }
    }

    public enum AppEnvironment { Test, Staging, Production }

    public class ApplicationInsightsSettings
    {
        public string InstrumentationKey { get; set; }
    }

	public class SmartyStreetSettings
	{
		public string AuthID { get; set; }
		public string AuthToken { get; set; }
		public string RefererHost { get; set; } // The autocomplete pro endpoint requires the Referer header to be a pre-set value 
		public string WebsiteKey { get; set; }
	}

    public class ZohoSettings
    {
        public string AccessToken { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string OrgID { get; set; }
        public bool PerformOrderSubmitTasks { get; set; }
    }

	public class OrderCloudSettings
	{
		public string ApiUrl { get; set; }
        public string MiddlewareClientID { get; set; }
        public string MiddlewareClientSecret { get; set; }
        public string WebhookHashKey { get; set; }
        public string ProvisionSupplierID { get; set; }
        public string SEBDistributionSupplierID { get; set; }
        public string FirstChoiceSupplierID { get; set; }
        public string MedlineSupplierID { get; set; }
        public string LaliciousSupplierID { get; set; }
        public string IncrementorPrefix { get; set; }
        public OrdercloudDataConfig DataConfig { get; set; } = new OrdercloudDataConfig();
    }

    public class OrdercloudDataConfig
    {
        public string AfAllLocationsCatalogID { get; set; }
        public string AfCAOnlyCatalogID { get; set; }
        public string AfUSOnlyCatalogID { get; set; }
        public string WtcAllLocationsCatalogID { get; set; }
        public string WtcWestCatalogID { get; set; }
        public string WtcEastCatalogID { get; set; }
        public string AfBuyerID { get; set; }
        public string WtcBuyerID { get; set; }
    }

	public class AvalaraSettings
	{
        public string BaseApiUrl { get; set; }
		public int AccountID { get; set; }
		public string LicenseKey { get; set; }
		public string CompanyCode { get; set; }
        public int CompanyID { get; set; }
	}
    public class EasyPostSettings
	{
        public string APIKey { get; set; }
        public string SMGFedexAccountId { get; set; }
        public string ProvisionFedexAccountId { get; set; } 
        public string SEBDistributionFedexAccountId { get; set; }
        public decimal NoRatesFallbackCost { get; set; }
        public int NoRatesFallbackTransitDays { get; set; }
        public int FreeShippingTransitDays { get; set; }
        public string USPSAccountId { get; set; }
    }

    public class SendgridSettings
    {
        public string ApiKey { get; set; }
        public string FromEmail { get; set; }
        public string SupportEmails { get; set; } // comma delimited list of emails that should be contacted when critical failures occur
        public string SEBSupportCaseEmail { get; set; }
        public string SEBBillingEmail { get; set; }
        public string OrderSubmitTemplateID { get; set; }
        public string OrderApprovalTemplateID { get; set; }
        public string LineItemStatusChangeTemplateID { get; set; }
        public string QuoteOrderSubmitTemplateID { get; set; }
        public string BuyerNewUserTemplateID { get; set; }
        public string InformationRequestTemplateID { get; set; }
        public string ProductUpdateTemplateID { get; set; }
        public string BuyerPasswordResetTemplateID { get; set; }
        public string SupportTemplateID { get; set; }
    }

    public class FlurlSettings
    {
        public int TimeoutInSeconds { get; set; }
    }

    public class AnytimeDashboardSettings
    {
        public string ApiUrl { get; set; }
        public string AuthUrl { get; set; }
        // Username and password are in here because it seems we cannot get a client-grant token with the secret?
        public string Username { get; set; }
        public string Password { get; set; }
        public string ApiToken { get; set; }
        public string ClientSecret { get; set; }
    }

    public class WaxDashboardSettings
    {
        public string ApiUrl { get; set; }
        public string AuthUrl { get; set; }
        public string UserClientID { get; set; } // "Four51 User" clientID. Used for SSO. 
        public string M2MClientID { get; set; } // "Machine to Machine" clientID. Used to query the api. 
        public string UserClientSecret { get; set; }
        public string M2MClientSecret { get; set; }
        public string CodeVerifier { get; set; } // A random string that get hashed an used to confirm the sender
    }
}
