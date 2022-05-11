using Headstart.Models;
using Newtonsoft.Json;
using ordercloud.integrations.library;
using System;
using System.Collections.Generic;
using System.Text;

namespace Headstart.Common.Models
{
    public class AFClub
    {
        public string id { get; set; }
        public string clubGuid { get; set; }
        public string afNumber { get; set; }
        public string billingNumber { get; set; }
        public string name { get; set; }
        public string phoneNumber { get; set; }
        public string email { get; set; }
        public string legalEntity { get; set; }
        public string primaryContactName { get; set; }
        public AFAddress address { get; set; }
        public Coordinates coordinates { get; set; }
        public AFLocationStatus status { get; set; }
        public DateTime? openingDate { get; set; }
        public bool isDeleted { get; set; } // Not sure how/if to use this yet.
    }
    public class AFAddress
    {
        public string city { get; set; }
        public string stateProvince { get; set; }
        public string postCode { get; set; }
        public string address { get; set; }
        public string address2 { get; set; }
        public string country { get; set; }
    }

    public class AFLocationStatus
    {
        public string id { get; set; }
        public string description { get; set; }
    }
    public class AFCredentials
    {
        public string username { get; set; }
        public int authId { get; set; }
        public string authGuid { get; set; }
        public CredentialsUserType userType { get; set; }
        public IEnumerable<CredentialsRole> roles { get; set; }
        public IEnumerable<CredentialsClub> clubs { get; set; }
    }

    public class CredentialsUserType
    {
        public int userTypeId { get; set; }
        public string name { get; set; }
    }

    public class CredentialsRole
    {
        public string id { get; set; }
        public string name { get; set; }
        public int applicationId { get; set; }
        public string category { get; set; }
    }

    public class CredentialsClub
    {
        public string guid { get; set; }
        public string name { get; set; }
        public string afNumber { get; set; }
        public string billingNumber { get; set; }
        public int franchiseDbId { get; set; }
    }
    public class AFGetStaffResponse
    {
        public string id { get; set; } // auto-incremented I think, e.g. 9381355
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string type { get; set; } // "Owner", "Staff", "Manager", "Trainer", "Regional Manager"
        public string language { get; set; } // "en-US", 
        public string email { get; set; }
        public string username { get; set; } // seems to match email
        public bool isDeleted { get; set; }
        public DateTime? updated { get; set; } // Not sure if how to use this yet, but seems like it might be helpful
        public List<AFClub> clubs { get; set; }
        public AFClub homeClub { get; set; }
    }
    public class AFStaff
    {
        public string id { get; set; } // auto-incremented I think, e.g. 9381355
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string type { get; set; } // "Owner", "Staff", "Manager", "Trainer", "Regional Manager"
        public string language { get; set; } // "en-US", 
        public string email { get; set; }
        public string username { get; set; } // seems to match email
        public bool isDeleted { get; set; }
        public DateTime? updated { get; set; } // Not sure if how to use this yet, but seems like it might be helpful
    }
    public class AFToken
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string refresh_token { get; set; }
    }

    public class Notification
    {
        public NotificationChannel Channel { get; set; } // the type of entity that changed
        public NotificationAction Action { get; set; } // the CRUD action that was performed
        public string TimestampUtc { get; set; } // the time the action was performed
        public string JsonData { get; set; } // json formatted string

        public T GetData<T>() => JsonConvert.DeserializeObject<T>(JsonData);
    }

    public enum NotificationChannel { Club, Staff }

    public enum NotificationAction { Update, Delete, Create }
    }
