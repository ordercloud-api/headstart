namespace Headstart.Common.Services.Portal.Models
{
    public class Marketplace
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public PortalUser Owner { get; set; } = new PortalUser();

        public string Environment { get; set; } = string.Empty;
    }
}