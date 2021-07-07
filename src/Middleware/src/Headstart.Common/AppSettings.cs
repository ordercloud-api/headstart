using ordercloud.integrations.cardconnect;
using ordercloud.integrations.exchangerates;
using ordercloud.integrations.library;
using ordercloud.integrations.smartystreets;

namespace Headstart.Common
{
    public class AppSettings
    {
        public UI UI { get; set; }
        public EnvironmentSettings EnvironmentSettings { get; set; } = new EnvironmentSettings();
        public ApplicationInsightsSettings ApplicationInsightsSettings { get; set; } = new ApplicationInsightsSettings();
        public AvalaraSettings AvalaraSettings { get; set; } = new AvalaraSettings();
        public BlobSettings BlobSettings { get; set; } = new BlobSettings();
        public CosmosSettings CosmosSettings { get; set; } = new CosmosSettings();
        public OrderCloudSettings OrderCloudSettings { get; set; } = new OrderCloudSettings();
        // not needed for fastsigns
        public OrderCloudIntegrationsCardConnectConfig CardConnectSettings { get; set; } = new OrderCloudIntegrationsCardConnectConfig();
        // not needed for fastsigns
        public ZohoSettings ZohoSettings { get; set; } = new ZohoSettings();
        // not needed for fastsigns
        public SmartyStreetsConfig SmartyStreetSettings { get; set; } = new SmartyStreetsConfig();
        // not needed for fastsigns
        public EasyPostSettings EasyPostSettings { get; set; } = new EasyPostSettings();
        public SendgridSettings SendgridSettings { get; set; } = new SendgridSettings();
        public FlurlSettings FlurlSettings { get; set; } = new FlurlSettings();
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
        public string MiddlewareBaseUrl { get; set; }
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
        public string IncrementorPrefix { get; set; }
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

    public class FlurlSettings
    {
        public int TimeoutInSeconds { get; set; }
    }
}
