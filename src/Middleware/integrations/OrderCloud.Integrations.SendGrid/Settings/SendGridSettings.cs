namespace OrderCloud.Integrations.SendGrid
{
    public class SendGridSettings
    {
        /// <summary>
        /// Api Key for SendGrid Account
        /// Required if EnvironmentSettings:EmailServiceProvider is set to "SendGrid".
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Email to send for payment, billing, or refund queries (Optional)
        /// </summary>
        public string BillingEmail { get; set; }

        /// <summary>
        /// Comma delimited list of emails that should be contacted when critical failures occur that require manual intervention (Optional).
        /// </summary>
        public string CriticalSupportEmails { get; set; }

        /// <summary>
        /// ID for template to be used for CriticalSupport emails - Optional but required to send CriticalSupport emails.
        /// </summary>
        public string CriticalSupportTemplateID { get; set; } // (Optional but required to send CriticalSupport emails) ID for template to be used for CriticalSupport emails

        /// <summary>
        /// The email address that should be used as the sending email address
        /// Required if EnvironmentSettings:EmailServiceProvider is set to "SendGrid"
        /// </summary>
        public string FromEmail { get; set; }

        /// <summary>
        /// ID for template to be used for LineItemStatusChange emails
        /// Optional but without it, no line item status change emails will be sent
        /// </summary>
        public string LineItemStatusChangeTemplateID { get; set; }

        /// <summary>
        /// ID for template to be used for NewUser emails
        /// Optional but required to send NewUser emails
        /// </summary>
        public string NewUserTemplateID { get; set; }

        /// <summary>
        /// ID for template to be used for OrderApproval emails
        /// Optional but without it, no order approval emails will be sent
        /// </summary>
        public string OrderApprovalTemplateID { get; set; }

        /// <summary>
        /// ID for the template to be used for OrderSubmit emails
        /// Optional but without it, no order submit emails will be sent.
        /// </summary>
        public string OrderSubmitTemplateID { get; set; }

        /// <summary>
        /// ID for template to be used for PasswordReset emails
        /// Optional but required to send PasswordReset emails.
        /// </summary>
        public string PasswordResetTemplateID { get; set; }

        /// <summary>
        /// ID for template to be used for ProductInformationRequest emails
        /// Optional but required to send ProductInformationRequest emails.
        /// </summary>
        public string ProductInformationRequestTemplateID { get; set; }

        /// <summary>
        /// ID for template to be used for QuoteOrderSubmit emails
        /// Optional but required to send QuoteOrderSubmit emails.
        /// </summary>
        public string QuoteOrderSubmitTemplateID { get; set; }

        /// <summary>
        /// ID for template to be used for order return emails
        /// Optional but required to send order return emails.
        /// </summary>
        public string OrderReturnTemplateID { get; set; }

        /// <summary>
        /// Email to send support cases to (Optional).
        /// </summary>
        public string SupportCaseEmail { get; set; }
    }
}
