using ordercloud.integrations.taxjar;
using ordercloud.integrations.vertex;
using ordercloud.integrations.library;
using ordercloud.integrations.cardconnect;
using ordercloud.integrations.smartystreets;

namespace Headstart.Common
{
	public class AppSettings
	{
		public ApplicationInsightsSettings ApplicationInsightsSettings { get; set; } = new ApplicationInsightsSettings();

		public AvalaraSettings AvalaraSettings { get; set; } = new AvalaraSettings();

		public CosmosSettings CosmosSettings { get; set; } = new CosmosSettings();

		public EasyPostSettings EasyPostSettings { get; set; } = new EasyPostSettings();

		public EnvironmentSettings EnvironmentSettings { get; set; } = new EnvironmentSettings();

		public FlurlSettings FlurlSettings { get; set; } = new FlurlSettings();

		public JobSettings JobSettings { get; set; } = new JobSettings();

		public OrderCloudSettings OrderCloudSettings { get; set; } = new OrderCloudSettings();

		public OrderCloudIntegrationsCardConnectConfig CardConnectSettings { get; set; } = new OrderCloudIntegrationsCardConnectConfig();

		public SendgridSettings SendgridSettings { get; set; } = new SendgridSettings();

		public ServiceBusSettings ServiceBusSettings { get; set; } = new ServiceBusSettings();

		public SmartyStreetsConfig SmartyStreetSettings { get; set; } = new SmartyStreetsConfig();

		public VertexConfig VertexSettings { get; set; } = new VertexConfig();

		public TaxJarConfig TaxJarSettings { get; set; } = new TaxJarConfig();

		public StorageAccountSettings StorageAccountSettings { get; set; } = new StorageAccountSettings();

		public UI UI { get; set; } = new UI();

		public ZohoSettings ZohoSettings { get; set; } = new ZohoSettings();

		public LogSettings LogSettings { get; set; } = new LogSettings();
	}

	public class LogSettings
	{
		public bool EnableCustomFileLogging { get; set; } = false;
		public string AppLogFileKey { get; set; } = "CustomApiLogs";
	}

	public class ApplicationInsightsSettings
	{
		public string InstrumentationKey { get; set; } = string.Empty;
	}

	public enum AppEnvironment { Test, Staging, Production }

	public class AvalaraSettings
	{
		public string BaseApiUrl { get; set; } = string.Empty;

		public int AccountId { get; set; }

		public string LicenseKey { get; set; } = string.Empty;

		public string CompanyCode { get; set; } = string.Empty;

		public int CompanyId { get; set; }
	}

	public class EasyPostSettings
	{
		public string ApiKey { get; set; } = string.Empty;

		public string SMGFedexAccountId { get; set; } = string.Empty;

		public string ProvisionFedexAccountId { get; set; } = string.Empty;

		public string SEBDistributionFedexAccountId { get; set; } = string.Empty;

		public decimal NoRatesFallbackCost { get; set; }

		public int NoRatesFallbackTransitDays { get; set; }

		public int FreeShippingTransitDays { get; set; }

		public string USPSAccountId { get; set; } = string.Empty;
	}

	public class EnvironmentSettings
	{
		public AppEnvironment Environment { get; set; }

		public string BuildNumber { get; set; } = string.Empty; // set during deploy

		public string Commit { get; set; } = string.Empty; // set during deploy

		public string MiddlewareBaseUrl { get; set; } = string.Empty;

		public TaxProvider TaxProvider { get; set; } = TaxProvider.Avalara;
	}

	public enum TaxProvider
	{
		Avalara, 
		Vertex, 
		Taxjar
	}


	public class FlurlSettings
	{
		public int TimeoutInSeconds { get; set; }
	}

	public class JobSettings
	{
		public bool ShouldCaptureCreditCardPayments { get; set; } = false;

		public bool ShouldRunZoho { get; set; } = false;

		public string CaptureCreditCardsAfterDate { get; set; } = string.Empty; // TODO: remove this once all orders have IsPaymentCaptured set
	}

	public class OrderCloudSettings
	{
		public string ApiUrl { get; set; } = string.Empty;

		public string MiddlewareClientId { get; set; } = string.Empty;

		public string MiddlewareClientSecret { get; set; } = string.Empty;

		public string MarketplaceId { get; set; } = string.Empty;

		// Used for display purposes
		public string MarketplaceName { get; set; } = string.Empty;

		public string WebhookHashKey { get; set; } = string.Empty;

		public string IncrementorPrefix { get; set; } = string.Empty;

		// Comma-separated list
		public string ClientIDsWithAPIAccess { get; set; } = string.Empty;
	}

	public class SendgridSettings
	{
		public string ApiKey { get; set; } = string.Empty;

		public string FromEmail { get; set; } = string.Empty;

		public string CriticalSupportEmails { get; set; } = string.Empty; // (Optional) Comma delimited list of emails that should be contacted when criticial failures occur that require manual intervention
        
		public string SupportCaseEmail { get; set; } = string.Empty; // (Optional) Email to send support cases to
        
		public string BillingEmail { get; set; } = string.Empty; // (Optional) Email to send for payment, billing, or refund queries
        
		public string OrderSubmitTemplateId { get; set; } = string.Empty; // (Optional but required to send OrderSubmit emails) ID for the template to be used for OrderSubmit emails
        
		public string OrderApprovalTemplateId { get; set; } = string.Empty; // (Optional but required to send OrderApproval emails) ID for template to be used for OrderApproval emails
        
		public string LineItemStatusChangeTemplateId { get; set; } = string.Empty; // (Optional but required to send LineItemStatusChange emails) ID for template to be used for LineItemStatusChange emails
        
		public string QuoteOrderSubmitTemplateId { get; set; } = string.Empty; // (Optional but required to send QuoteOrderSubmit emails) ID for template to be used for QuoteOrderSubmit emails
        
		public string NewUserTemplateId { get; set; } = string.Empty; // (Optional but required to send NewUser emails) ID for template to be used for NewUser emails
        
		public string ProductInformationRequestTemplateId { get; set; } = string.Empty; // (Optional but required to send ProductInformationRequest emails) ID for template to be used for ProductInformationRequest emails
       
		public string PasswordResetTemplateId { get; set; } = string.Empty; // (Optional but required to send PasswordReset emails) ID for template to be used for PasswordReset emails
        
		public string CriticalSupportTemplateId { get; set; } = string.Empty; // (Optional but required to send CriticalSupport emails) ID for template to be used for CriticalSupport emails
	}

	public class ServiceBusSettings
	{
		public string ConnectionString { get; set; } = string.Empty;

		public string ZohoQueueName { get; set; } = string.Empty;
	}

	public class SmartyStreetSettings
	{
		public string AuthId { get; set; } = string.Empty;

		public string AuthToken { get; set; } = string.Empty;

		public string RefererHost { get; set; } = string.Empty; // The autocomplete pro endpoint requires the Referer header to be a pre-set value

		public string WebsiteKey { get; set; }
	}

	public class UI
	{
		public string BaseAdminUrl { get; set; } = string.Empty;
	}

	public class ZohoSettings
	{
		public string AccessToken { get; set; } = string.Empty;

		public string ClientId { get; set; } = string.Empty;

		public string ClientSecret { get; set; } = string.Empty;

		public string OrgId { get; set; } = string.Empty;

		public bool PerformOrderSubmitTasks { get; set; } = false;
	}
}