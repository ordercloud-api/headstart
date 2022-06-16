using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace OrderCloud.Integrations.EnvironmentSeed.Models
{
    public class UploadTranslationsRequest
    {
        /// <summary>
        /// The translations file containing localisation strings.
        /// </summary>
        [Required]
        public IFormFile TranslationsFile { get; set; }

        /// <summary>
        /// The language code of the translation file being uploaded.
        /// </summary>
        [Required]
        public string LanguageCode { get; set; }
    }
}
