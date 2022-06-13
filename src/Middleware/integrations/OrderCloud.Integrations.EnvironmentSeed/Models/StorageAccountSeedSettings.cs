using System.ComponentModel.DataAnnotations;

namespace OrderCloud.Integrations.EnvironmentSeed.Models
{
    public class StorageAccountSeedSettings
    {
        [Required]
        public string ConnectionString { get; set; }

        public string ContainerNameTranslations { get; set; } = "ngx-translate";

        public string ContainerNameDownloads { get; set; } = "downloads";
    }
}
