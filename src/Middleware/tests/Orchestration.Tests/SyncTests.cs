using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Cosmonaut;
using Headstart.Common;
using Headstart.Common.Models;
using Headstart.Common.Queries;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSubstitute;
using ordercloud.integrations.library;
using OrderCloud.SDK;
using Action = Headstart.Common.Models.Action;
using Headstart.Common.Services.CMS;
using Headstart.Orchestration;

namespace Orchestration.Tests
{
    public class SyncTests
    {
        private AppSettings _settings;
        private SyncCommand _command;

        [SetUp]
        public void Setup()
        {
            _settings = new AppSettings()
            {
                OrderCloudSettings = new OrderCloudSettings()
                {
                    ApiUrl = "api"
                }
            };
            _command = new SyncCommand(_settings, 
                new OrderCloudClient(), 
                Substitute.For<ICMSClient>(Substitute.For<ICMSClient>()), 
                Substitute.For<LogQuery>(Substitute.For<ICosmosStore<OrchestrationLog>>()));
        }

        [Test]
        [Ignore("Can not instantiate proxy of class: ordercloud.integrations.cms.AssetQuery")]
        public async Task sync_invoke_test()
        {
            using var currentFile = File.OpenText($"JObjectTests/hydrated.json");
            using var currentReader = new JsonTextReader(currentFile);
            var current = (JObject)JToken.ReadFrom(currentReader);

            var wi = new WorkItem("fourover/guid/hydratedproduct/guid")
            {
                Action = Action.Create, 
                ClientId = "clientid", 
                RecordType = RecordType.HydratedProduct, 
                Current = current["Model"] as JObject,
                Token = current["Token"].ToString()
            };
            try
            {
                var test = await _command.Dispatch(wi);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
    }
}
