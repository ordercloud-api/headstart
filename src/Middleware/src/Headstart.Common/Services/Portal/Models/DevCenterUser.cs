namespace Headstart.Common.Services.Portal.Models
{
    public class PortalUser
    {
        public string Email { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public bool CanCreateProductionOrgs { get; set; }
    }

    public class PortalAuthResponse
    {
        public string[] coreuser_roles { get; set; }

        public string[] portaluser_roles { get; set; }

        public string[] granted_roles { get; set; }

        public string access_token { get; set; } = string.Empty;

        public string refresh_token { get; set; } = string.Empty;

        public string token_type { get; set; } = string.Empty;

        public int expires_in { get; set; }
    }
}