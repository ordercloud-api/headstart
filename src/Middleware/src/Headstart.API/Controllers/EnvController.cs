using Microsoft.AspNetCore.Mvc;

namespace Headstart.Common.Controllers
{
    [Route("env")]
    public class EnvController : ControllerBase
    {
        private readonly AppSettings settings;

        public EnvController(AppSettings settings)
        {
            this.settings = settings;
        }

        [HttpGet]
        public object Get()
        {
            return new
            {
                Environment = settings.EnvironmentSettings.Environment.ToString(),
                BuildNumber = settings.EnvironmentSettings.BuildNumber, // set during deploy
                Commit = settings.EnvironmentSettings.Commit, // set during deploy
                CosmosDatabase = settings.CosmosSettings.DatabaseName,
            };
        }
    }
}
