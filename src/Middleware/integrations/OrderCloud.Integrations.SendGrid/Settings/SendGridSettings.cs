namespace OrderCloud.Integrations.SendGrid
{
    public class SendGridSettings
    {
        public string ApiKey { get; set; }

        public string BillingEmail { get; set; } // (Optional) Email to send for payment, billing, or refund queries

        public string CriticalSupportEmails { get; set; } // (Optional) Comma delimited list of emails that should be contacted when criticial failures occur that require manual intervention

        public string CriticalSupportTemplateID { get; set; } // (Optional but required to send CriticalSupport emails) ID for template to be used for CriticalSupport emails

        public string FromEmail { get; set; }

        public string LineItemStatusChangeTemplateID { get; set; } // (Optional but required to send LineItemStatusChange emails) ID for template to be used for LineItemStatusChange emails

        public string NewUserTemplateID { get; set; } // (Optional but required to send NewUser emails) ID for template to be used for NewUser emails

        public string OrderApprovalTemplateID { get; set; } // (Optional but required to send OrderApproval emails) ID for template to be used for OrderApproval emails

        public string OrderSubmitTemplateID { get; set; } // (Optional but required to send OrderSubmit emails) ID for the template to be used for OrderSubmit emails

        public string PasswordResetTemplateID { get; set; } // (Optional but required to send PasswordReset emails) ID for template to be used for PasswordReset emails

        public string ProductInformationRequestTemplateID { get; set; } // (Optional but required to send ProductInformationRequest emails) ID for template to be used for ProductInformationRequest emails

        public string QuoteOrderSubmitTemplateID { get; set; } // (Optional but required to send QuoteOrderSubmit emails) ID for template to be used for QuoteOrderSubmit emails

        public string OrderReturnTemplateID { get; set; } // (Optional) but required to send order return emails

        public string SupportCaseEmail { get; set; } // (Optional) Email to send support cases to
    }
}
