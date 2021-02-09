using NPOI.OpenXmlFormats.Dml;
using ordercloud.integrations.library;

namespace Headstart.Models
{
    [SwaggerModel]
    public class MonitoredProductFieldModifiedNotification
    {
        public NotificationUser Supplier { get; set; }
        public NotificationProduct Product { get; set; }
        public string Status { get; set; }
        public NotificationHistory History { get; set; }
    }

    public class NotificationUser
    {
        public string ID { get; set; }
        public string Name { get; set; }
    }
    public class NotificationProduct
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string FieldModified { get; set; }
        public dynamic PreviousValue { get; set; }
        public dynamic CurrentValue { get; set; }
    }
    public class NotificationHistory
    {
        public NotificationUser ModifiedBy { get; set; }
        public NotificationUser ReviewedBy { get; set; }
        public string DateModified { get; set; }
        public string DateReviewed { get; set; }
    }
    public static class NotificationStatus
    {
        public const string SUBMITTED = "SUBMITTED";
        public const string ACCEPTED = "ACCEPTED";
        public const string DISMISSED = "DISMISSED";
        public const string REJECTED = "REJECTED";
    }
}
