using ordercloud.integrations.cardconnect;
using ordercloud.integrations.exchangerates;
using ordercloud.integrations.library;
using ordercloud.integrations.smartystreets;
using ordercloud.integrations.taxjar;
using ordercloud.integrations.vertex;

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
        public UI UI { get; set; }
        public ZohoSettings ZohoSettings { get; set; } = new ZohoSettings();
    }

    public class ApplicationInsightsSettings
    {
        public string InstrumentationKey { get; set; }
    }

    public enum AppEnvironment { Test, Staging, Production }

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

	public class EnvironmentSettings
	{
		public AppEnvironment Environment { get; set; }
		public string BuildNumber { get; set; } // set during deploy
		public string Commit { get; set; } // set during deploy
		public string MiddlewareBaseUrl { get; set; }
        public TaxProvider TaxProvider { get; set; } = TaxProvider.Avalara;
	}

    public enum TaxProvider { Avalara, Vertex, Taxjar }


    public class FlurlSettings
    {
        public int TimeoutInSeconds { get; set; }
    }

    public class JobSettings
    {
        public bool ShouldCaptureCreditCardPayments { get; set; }
        public bool ShouldRunZoho { get; set; }
        public string CaptureCreditCardsAfterDate { get; set; } // TODO: remove this once all orders have IsPaymentCaptured set
    }

    public class OrderCloudSettings
    {
        public string ApiUrl { get; set; }
        public string MiddlewareClientID { get; set; }
        public string MiddlewareClientSecret { get; set; }
        public string MarketplaceID { get; set; }
        public string MarketplaceName { get; set; } // used for display purposes
        public string WebhookHashKey { get; set; }
        public string IncrementorPrefix { get; set; }
    }

    public class SendgridSettings
    {
        public string ApiKey { get; set; }
        public string FromEmail { get; set; }
        public string CriticalSupportEmails { get; set; } // (Optional) Comma delimited list of emails that should be contacted when criticial failures occur that require manual intervention
        public string SupportCaseEmail { get; set; } // (Optional) Email to send support cases to
        public string BillingEmail { get; set; } // (Optional) Email to send for payment, billing, or refund queries
        public string OrderSubmitTemplateID { get; set; } // (Optional but required to send OrderSubmit emails) ID for the template to be used for OrderSubmit emails
        public string OrderApprovalTemplateID { get; set; } // (Optional but required to send OrderApproval emails) ID for template to be used for OrderApproval emails
        public string LineItemStatusChangeTemplateID { get; set; } // (Optional but required to send LineItemStatusChange emails) ID for template to be used for LineItemStatusChange emails
        public string QuoteOrderSubmitTemplateID { get; set; } // (Optional but required to send QuoteOrderSubmit emails) ID for template to be used for QuoteOrderSubmit emails
        public string NewUserTemplateID { get; set; } // (Optional but required to send NewUser emails) ID for template to be used for NewUser emails
        public string ProductInformationRequestTemplateID { get; set; } // (Optional but required to send ProductInformationRequest emails) ID for template to be used for ProductInformationRequest emails
        public string PasswordResetTemplateID { get; set; } // (Optional but required to send PasswordReset emails) ID for template to be used for PasswordReset emails
        public string CriticalSupportTemplateID { get; set; } // (Optional but required to send CriticalSupport emails) ID for template to be used for CriticalSupport emails
    }

    public class ServiceBusSettings
    {
        public string ConnectionString { get; set; }
        public string ZohoQueueName { get; set; }
    }

    public class SmartyStreetSettings
    {
        public string AuthID { get; set; }
        public string AuthToken { get; set; }
        public string RefererHost { get; set; } // The autocomplete pro endpoint requires the Referer header to be a pre-set value 
        public string WebsiteKey { get; set; }
    }

    public class UI
    {
        public string BaseAdminUrl { get; set; }
    }

    public class ZohoSettings
    {
        public string AccessToken { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string OrgID { get; set; }
        public bool PerformOrderSubmitTasks { get; set; }
    }
}
