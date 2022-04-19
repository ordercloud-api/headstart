using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using Headstart.Common.Models.Base;
using Headstart.Common.Models.Headstart;

namespace Headstart.Common.Models.Dashboard
{
	public class AfClub : HsBaseObject
	{
		public string ClubGuid { get; set; } = string.Empty;

		public string AfNumber { get; set; } = string.Empty;

		public string BillingNumber { get; set; } = string.Empty;

		public string Name { get; set; } = string.Empty;

		public string PhoneNumber { get; set; } = string.Empty;

		public string Email { get; set; } = string.Empty;

		public string LegalEntity { get; set; } = string.Empty;

		public string PrimaryContactName { get; set; } = string.Empty;

		public AfAddress Address { get; set; } = new AfAddress();

		public Coordinates Coordinates { get; set; } = new Coordinates();

		public AfLocationStatus Status { get; set; } = new AfLocationStatus();

		public DateTime? OpeningDate { get; set; }

		public bool IsDeleted { get; set; } // Not sure how/if to use this yet.
	}
	public class AfAddress
	{
		public string City { get; set; } = string.Empty;

		public string StateProvince { get; set; } = string.Empty;

		public string PostCode { get; set; } = string.Empty;

		public string Address { get; set; } = string.Empty;

		public string Address2 { get; set; } = string.Empty;

		public string Country { get; set; } = string.Empty;
	}

	public class AfLocationStatus
	{
		public string Id { get; set; } = string.Empty;

		public string Description { get; set; } = string.Empty;
	}

	public class AfCredentials
	{
		public string Username { get; set; } = string.Empty;

		public int AuthId { get; set; }

		public string AuthGuid { get; set; } = string.Empty;

		public CredentialsUserType UserType { get; set; } = new CredentialsUserType();

		public IEnumerable<CredentialsRole> Roles { get; set; } = new List<CredentialsRole>();

		public IEnumerable<CredentialsClub> Clubs { get; set; } = new List<CredentialsClub>();   
	}

	public class CredentialsUserType
	{
		public int UserTypeId { get; set; }

		public string Name { get; set; } = string.Empty;
	}

	public class CredentialsRole
	{
		public string Id { get; set; } = string.Empty;

		public string Name { get; set; } = string.Empty;

		public int ApplicationId { get; set; }

		public string Category { get; set; } = string.Empty;
	}

	public class CredentialsClub
	{
		public string Quid { get; set; } = string.Empty;

		public string Name { get; set; } = string.Empty;

		public string AfNumber { get; set; } = string.Empty;

		public string BillingNumber { get; set; } = string.Empty;

		public int FranchiseDbId { get; set; }
	}
	public class AfGetStaffResponse
	{
		public string Id { get; set; } = string.Empty; // auto-incremented I think, e.g. 9381355

		public string FirstName { get; set; } = string.Empty;

		public string LastName { get; set; } = string.Empty;

		public string Type { get; set; } = string.Empty; // "Owner", "Staff", "Manager", "Trainer", "Regional Manager"

		public string Language { get; set; } = string.Empty; // "en-US", 

		public string Email { get; set; } = string.Empty;

		public string Username { get; set; } = string.Empty; // Seems to match email

		public bool IsDeleted { get; set; }

		public DateTime? Updated { get; set; } // Not sure if how to use this yet, but seems like it might be helpful

		public List<AfClub> Clubs { get; set; } = new List<AfClub>();

		public AfClub HomeClub { get; set; } = new AfClub();
	}
	public class AfStaff
	{
		public string Id { get; set; } = string.Empty; // auto-incremented I think, e.g. 9381355

		public string FirstName { get; set; } = string.Empty;

		public string LastName { get; set; } = string.Empty;

		public string Type { get; set; } = string.Empty; // "Owner", "Staff", "Manager", "Trainer", "Regional Manager"

		public string Language { get; set; } = string.Empty; // "en-US", 

		public string Email { get; set; } = string.Empty;

		public string Username { get; set; } = string.Empty; // seems to match email

		public bool IsDeleted { get; set; }

		public DateTime? Updated { get; set; } // Not sure if how to use this yet, but seems like it might be helpful
	}

	public class AfToken
	{
		public string AccessToken { get; set; } = string.Empty;

		public string TokenType { get; set; } = string.Empty;

		public int ExpiresIn { get; set; }

		public string RefreshToken { get; set; } = string.Empty;
	}

	public class Notification
	{
		public NotificationChannel Channel { get; set; } // the type of entity that changed

		public NotificationAction Action { get; set; } // the CRUD action that was performed

		public string TimestampUtc { get; set; } = string.Empty; // the time the action was performed

		private string JsonData { get; set; } = string.Empty; // json formatted string

		public T GetData<T>()
		{
			return JsonConvert.DeserializeObject<T>(JsonData);
		}
	}

	public enum NotificationChannel
	{
		Club, 
		Staff
	}

	public enum NotificationAction
	{
		Update, 
		Delete, 
		Create
	}
}