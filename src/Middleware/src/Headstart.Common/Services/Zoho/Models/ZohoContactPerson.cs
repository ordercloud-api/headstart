using System;
using System.Collections.Generic;
using System.Text;

namespace Headstart.Common.Services.Zoho.Models
{
    public class ZohoContactPerson
    {
        public string contact_person_id { get; set; }
        public string salutation { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string mobile { get; set; }
        public string designation { get; set; }
        public string department { get; set; }
        public string skype { get; set; }
        //public bool is_primary_contact { get; set; }
        public bool enable_portal { get; set; }
    }
}
