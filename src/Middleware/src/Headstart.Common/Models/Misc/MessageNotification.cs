using OrderCloud.SDK;
using Headstart.Models.Headstart;
using System.Collections.Generic;

namespace Headstart.Models.Misc
{
    public class MessageNotification<EventBodyType>
    {
        public string BuyerID { get; set; } = string.Empty;

        public string UserToken { get; set; } = string.Empty;

        public User Recipient { get; set; } = new User();

        public MessageType MessageType { get; set; }

        public string[] CCRecipient { get; set; }

        public MessageConfigData ConfigData { get; set; } = new MessageConfigData();

        public EventBodyType EventBody { get; set; }
    }

    public class OrderSubmitEventBody
    {
        public HSOrder Order { get; set; } = new HSOrder();

        public List<OrderApproval> Approvals { get; set; } = new List<OrderApproval>();

        public List<HSLineItem> LineItems { get; set; } = new List<HSLineItem>();

        public List<HSProduct> Products { get; set; } = new List<HSProduct>();
    }

    public class PasswordResetEventBody
    {
        public string Username { get; set; } = string.Empty;

        public string PasswordRenewalAccessToken { get; set; } = string.Empty;

        public string PasswordRenewalVerificationCode { get; set; } = string.Empty;

        public string PasswordRenewalUrl { get; set; } = string.Empty;
    }

    public class MessageConfigData
    {
        public MessageTypeConfig[] MessageTypeConfig { get; set; }

        public string ApiKey { get; set; } = string.Empty;
    }

    public class MessageTypeConfig
    {
        public string FromEmail { get; set; } = string.Empty;

        public string MainContent { get; set; } = string.Empty;

        public string MessageType { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;

        public string TemplateName { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
    }

    public class ContactSupplierBody
    {
        public HSProduct Product { get; set; } = new HSProduct();

        public BuyerRequestForInfo BuyerRequest { get; set; } = new BuyerRequestForInfo();
    }

    public class BuyerRequestForInfo
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string BuyerLocation { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Comments { get; set; } = string.Empty;
    }
}