using Headstart.Common;
using ordercloud.integrations.library;
using System;
using System.Collections.Generic;
using System.Linq;
using Flurl.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.Common;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.WindowsAzure.Storage.Blob;
using OrderCloud.SDK;
using System.IO;
using static Headstart.Common.Models.Headstart.AppConfigurations;
using Newtonsoft.Json;

namespace Headstart.API.Commands
{
    public interface IStorefrontCommand
    {
        Task DeployBuyerSite(ApiClient apiClient = null);
    }

    public class StorefrontCommand : IStorefrontCommand
    {
        private readonly AppSettings _settings;
        private readonly IOrderCloudIntegrationsBlobService _blob;
        private readonly IOrderCloudClient _oc;


        public StorefrontCommand(IOrderCloudIntegrationsBlobService blob, AppSettings settings, IOrderCloudClient oc)
        {
            _settings = settings;
            _blob = blob;
            _oc = oc;
        }

        private readonly string storefrontPlaceholder = "<PLACEHOLDER>";
        private readonly string envPlaceholder = "\"#{environmentConfig}\"";
        private readonly string originalMainFileName = "main-original.js";
        private bool isMainFile(string fileName) => fileName.StartsWith("main.") && fileName.EndsWith(".js");
        

        public async Task DeployBuyerSite(ApiClient apiClient = null)
        {
            //First create the api client
            //var client = await _oc.ApiClients.CreateAsync(apiClient);


            //1. Create the $web container (not sure if this is necessary. If they enable static website hosting this should already be there)
            await _blob.CreateContainerAsync("$web", false);

            //2. Get files of deployed buyer app so we can upload them to our contianer (within a subfolder)
            var files = await _blob.GetBlobFiles("buyerweb");
            var tasks = new List<Task>();

            var storefrontName = apiClient?.xp?.StorefrontName;
            var incrementorPrefix = apiClient?.xp?.IncrementorPrefix;
            var appName = apiClient.AppName;
            var mainFileName = "";

            foreach (var file in files)
            {
                var fileName = file.Uri.ToString().Split("buyerweb/")[1];
                if(isMainFile(fileName))
                {
                    mainFileName = fileName; // need to save the mainFileName to a variable so we know which file to replace with the new app configs
                }
                tasks.Add(_blob.TransferBlobs("buyerweb", "$web", fileName, storefrontName));
            }

            await Task.WhenAll(tasks);
            await UpdateAppConfig(apiClient, mainFileName, storefrontName, incrementorPrefix, appName);
            await UpdateIndex(storefrontName);
        }

        private async Task UpdateIndex(string storefrontName)
        {
            var path = $"{storefrontName}/index.html";
            var index = (await _blob.Get(path))
                .Replace("<base href=\"/\">", $"<base href='/{storefrontName}'/>")
                .Replace(storefrontPlaceholder, storefrontName);

            await _blob.Save(path, index, "text/html");
        }

        private async Task UpdateAppConfig(ApiClient apiClient, string mainFileName, string storefrontName, string incrementorPrefix, string appName)
        {
            var config = await _blob.Get<BuyerAppConfiguration>($"{storefrontName}/assets/appConfigs/headstartdemo-test.json");

            // Update our config file with values from the apiClient
            config.incrementorPrefix = !String.IsNullOrEmpty(incrementorPrefix) ? incrementorPrefix : config.incrementorPrefix;
            config.appname = !String.IsNullOrEmpty(appName) ? appName : config.appname;
            config.storefrontName = storefrontName;

            //update our main file with new app configs
            var originalMain = (await _blob.Get($"{storefrontName}/{originalMainFileName}"));
            var updatedMain = originalMain
                .Replace(envPlaceholder, JsonConvert.SerializeObject(config));

            await Task.WhenAll(
            _blob.Save($"{storefrontName}/assets/appConfigs/headstartdemo-test.json", JsonConvert.SerializeObject(config), "application/json"),
            _blob.Save($"{storefrontName}/{mainFileName}", updatedMain, "application/x-javascript")
            );
        }

        private void CreateDirectory(string directoryName)
        {
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
        }

        private void DeleteDirectory(string directoryName)
        {
            if (Directory.Exists(directoryName))
            {
                Directory.Delete(directoryName, true);

            }
        }
    }
}
