namespace Headstart.Common.Services.Portal.Models
{
    public class AdminCompany
    {
        public string Name { get; set; } = string.Empty;

        public string ID { get; set; } = string.Empty;

        public int OwnerDevID { get; set; }

        public object AutoForwardingUserID { get; set; }
    }
}