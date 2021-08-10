namespace Headstart.Common.Services.Portal.Models
{
    public class Marketplace
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public PortalUser Owner { get; set; }
        public string Environment { get; set; }
    }
}
