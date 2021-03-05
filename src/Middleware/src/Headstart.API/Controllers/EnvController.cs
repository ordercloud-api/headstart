using Microsoft.AspNetCore.Mvc;

namespace Headstart.Common.Controllers
{
    [Route("env")]
    public class EnvController : ControllerBase
    {
        private readonly AppSettings _settings;

        public EnvController(AppSettings settings) 
        {
            _settings = settings;
        }

        [HttpGet]
        public object Get()
        {
            return new { 
                Environment = _settings.EnvironmentSettings.Environment.ToString(),
                BuildNumber = _settings.EnvironmentSettings.BuildNumber, // set during deploy
                Commit = _settings.EnvironmentSettings.Commit, // set during deploy
                CosmosDatabase = _settings.CosmosSettings.DatabaseName
            };
        }
    }
}
