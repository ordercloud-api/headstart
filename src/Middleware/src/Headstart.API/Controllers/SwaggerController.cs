﻿using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using ordercloud.integrations.library;
using OrderCloud.Catalyst;
using ErrorCodes = Headstart.Models.ErrorCodes;

namespace Headstart.Common.Controllers
{
    [Route("swagger")]
    public class SwaggerController : BaseController
    {
        private readonly AppSettings _settings;

        public SwaggerController(AppSettings settings)
        {
            _settings = settings;
        }

        [HttpGet]
        public async Task<JObject> Get()
        {
            var reference = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var g = new OpenApiGenerator()
                .CollectMetaData(Path.Combine(reference, "reference.md"), ErrorCodes.All)
                .DefineSpec(new SwaggerConfig()
                {
                    Name = "Headstart",
                    ContactEmail = "oheywood@four51.com",
                    Description = "Headstart API",
                    Host = _settings.EnvironmentSettings.MiddlewareBaseUrl,
                    Title = "Headstart API",
                    Url = "https://ordercloud.io",
                    Version = "1.0"
                });
            return await Task.FromResult(g.Specification());
        }
    }
}
