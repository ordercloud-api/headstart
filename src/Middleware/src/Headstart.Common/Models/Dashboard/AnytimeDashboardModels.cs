using System;
using Newtonsoft.Json;
using Headstart.Models;
using System.Collections.Generic;

namespace Headstart.Common.Models
{
    public class AFClub
    {
        public string id { get; set; } = string.Empty;

        public string clubGuid { get; set; } = string.Empty;

        public string afNumber { get; set; } = string.Empty;

        public string billingNumber { get; set; } = string.Empty;

        public string name { get; set; } = string.Empty;

        public string phoneNumber { get; set; } = string.Empty;

        public string email { get; set; } = string.Empty;

        public string legalEntity { get; set; } = string.Empty;

        public string primaryContactName { get; set; } = string.Empty;

        public AFAddress address { get; set; } = new AFAddress();

        public Coordinates coordinates { get; set; } = new Coordinates();

        public AFLocationStatus status { get; set; } = new AFLocationStatus();

        public DateTime? openingDate { get; set; }

        public bool isDeleted { get; set; } // Not sure how/if to use this yet.
    }
    public class AFAddress
    {
        public string city { get; set; } = string.Empty;

        public string stateProvince { get; set; } = string.Empty;

        public string postCode { get; set; } = string.Empty;

        public string address { get; set; } = string.Empty;

        public string address2 { get; set; } = string.Empty;

        public string country { get; set; } = string.Empty;
    }

    public class AFLocationStatus
    {
        public string id { get; set; } = string.Empty;

        public string description { get; set; } = string.Empty;
    }
    public class AFCredentials
    {
        public string username { get; set; } = string.Empty;

        public int authId { get; set; }

        public string authGuid { get; set; } = string.Empty;

        public CredentialsUserType userType { get; set; } = new CredentialsUserType();

        public IEnumerable<CredentialsRole> roles { get; set; } = new List<CredentialsRole>();

        public IEnumerable<CredentialsClub> clubs { get; set; } = new List<CredentialsClub>();   
    }

    public class CredentialsUserType
    {
        public int userTypeId { get; set; }

        public string name { get; set; } = string.Empty;

    }

    public class CredentialsRole
    {
        public string id { get; set; } = string.Empty;

        public string name { get; set; } = string.Empty;

        public int applicationId { get; set; }

        public string category { get; set; } = string.Empty;
    }

    public class CredentialsClub
    {
        public string guid { get; set; } = string.Empty;

        public string name { get; set; } = string.Empty;

        public string afNumber { get; set; } = string.Empty;

        public string billingNumber { get; set; } = string.Empty;

        public int franchiseDbId { get; set; }
    }
    public class AFGetStaffResponse
    {
        public string id { get; set; } = string.Empty; // auto-incremented I think, e.g. 9381355

        public string firstName { get; set; } = string.Empty;

        public string lastName { get; set; } = string.Empty;

        public string type { get; set; } = string.Empty; // "Owner", "Staff", "Manager", "Trainer", "Regional Manager"

        public string language { get; set; } = string.Empty;// "en-US", 

        public string email { get; set; } = string.Empty;

        public string username { get; set; } = string.Empty;// seems to match email

        public bool isDeleted { get; set; }

        public DateTime? updated { get; set; } // Not sure if how to use this yet, but seems like it might be helpful

        public List<AFClub> clubs { get; set; } = new List<AFClub>();

        public AFClub homeClub { get; set; } = new AFClub();
    }
    public class AFStaff
    {
        public string id { get; set; } = string.Empty; // auto-incremented I think, e.g. 9381355

        public string firstName { get; set; } = string.Empty;

        public string lastName { get; set; } = string.Empty;

        public string type { get; set; } = string.Empty; // "Owner", "Staff", "Manager", "Trainer", "Regional Manager"

        public string language { get; set; } = string.Empty; // "en-US", 

        public string email { get; set; } = string.Empty;

        public string username { get; set; } = string.Empty; // seems to match email

        public bool isDeleted { get; set; }

        public DateTime? updated { get; set; } // Not sure if how to use this yet, but seems like it might be helpful
    }
    public class AFToken
    {
        public string access_token { get; set; } = string.Empty;

        public string token_type { get; set; } = string.Empty;

        public int expires_in { get; set; }

        public string refresh_token { get; set; } = string.Empty;
    }

    public class Notification
    {
        public NotificationChannel Channel { get; set; } // the type of entity that changed

        public NotificationAction Action { get; set; } // the CRUD action that was performed

        public string TimestampUtc { get; set; } = string.Empty; // the time the action was performed

        public string JsonData { get; set; } = string.Empty; // json formatted string

        public T GetData<T>()
        {
            return JsonConvert.DeserializeObject<T>(JsonData);
        }
    }

    public enum NotificationChannel { Club, Staff }

    public enum NotificationAction { Update, Delete, Create }
}