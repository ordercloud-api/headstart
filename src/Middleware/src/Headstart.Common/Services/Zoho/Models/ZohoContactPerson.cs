namespace Headstart.Common.Services.Zoho.Models
{
    public class ZohoContactPerson
    {
        public string contact_person_id { get; set; } = string.Empty;

        public string salutation { get; set; } = string.Empty;

        public string first_name { get; set; } = string.Empty;

        public string last_name { get; set; } = string.Empty;

        public string email { get; set; } = string.Empty;

        public string phone { get; set; } = string.Empty;

        public string mobile { get; set; } = string.Empty;

        public string designation { get; set; } = string.Empty;

        public string department { get; set; } = string.Empty;

        public string skype { get; set; } = string.Empty;

        public bool enable_portal { get; set; }
    }
}