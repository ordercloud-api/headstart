using System.Collections.Generic;
using OrderCloud.SDK;

namespace Headstart.Common.Models
{
    public class MessageNotification<TEventBodyType>
    {
        public string BuyerID { get; set; }

        public string UserToken { get; set; }

        public User Recipient { get; set; }

        public MessageType MessageType { get; set; }

        public string[] CCRecipient { get; set; }

        public MessageConfigData ConfigData { get; set; }

        public TEventBodyType EventBody { get; set; }
    }

    public class OrderSubmitEventBody
    {
        public HSOrder Order { get; set; }

        public List<OrderApproval> Approvals { get; set; }

        public List<HSLineItem> LineItems { get; set; }

        public List<HSProduct> Products { get; set; }
    }

    public class OrderReturnEventBody : OrderSubmitEventBody
    {
        public HSOrderReturn OrderReturn { get; set; }
    }

    public class PasswordResetEventBody
    {
        public string Username { get; set; }

        public string PasswordRenewalAccessToken { get; set; }

        public string PasswordRenewalVerificationCode { get; set; }

        public string PasswordRenewalUrl { get; set; }
    }

    public class MessageConfigData
    {
        public MessageTypeConfig[] MessageTypeConfig { get; set; }

        public string ApiKey { get; set; }
    }

    public class MessageTypeConfig
    {
        public string FromEmail { get; set; }

        public string MainContent { get; set; }

        public string MessageType { get; set; }

        public string Subject { get; set; }

        public string TemplateName { get; set; }

        public string Name { get; set; }
    }

    public class ContactSupplierBody
    {
        public HSProduct Product { get; set; }

        public BuyerRequestForInfo BuyerRequest { get; set; }
    }

    public class BuyerRequestForInfo
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string BuyerLocation { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string Comments { get; set; }
    }
}
