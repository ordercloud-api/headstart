namespace Headstart.Common.Services.Portal.Models
{
    public class Organization
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public PortalUser Owner { get; set; }
        public string Environment { get; set; }
    }
}
