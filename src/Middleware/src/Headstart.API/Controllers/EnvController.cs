using Microsoft.AspNetCore.Mvc;

namespace Headstart.Common.Controllers
{
    [Route("env")]
    public class EnvController : ControllerBase
    {
        private readonly AppSettings _settings;

        /// <summary>
        /// The IOC based constructor method for the EnvController class object with Dependency Injection
        /// </summary>
        /// <param name="settings"></param>
        public EnvController(AppSettings settings) 
        {
            _settings = settings;
        }

        /// <summary>
        /// Gets a new object with environment settings
        /// </summary>
        /// <returns>An object with environment settings</returns>
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