using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace ordercloud.integrations.library
{
    public class SwaggerConfig
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string ContactEmail { get; set; }
        public string Host { get; set; }
        /// <summary>
        /// Default value of "/"
        /// </summary>
        public string BasePath { get; set; } = "/";
        /// <summary>
        /// Default value of "https"
        /// </summary>
        public JToken Schemes { get; set; } = new JArray("https");
    }
}
