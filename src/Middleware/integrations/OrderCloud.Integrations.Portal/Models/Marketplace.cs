using Headstart.Common.Models;

namespace OrderCloud.Integrations.Portal.Models
{
    public class Marketplace
    {
        public string Environment { get; set; }

        public string Id { get; set; }

        public string Name { get; set; }

        public Region Region { get; set; }
    }
}
