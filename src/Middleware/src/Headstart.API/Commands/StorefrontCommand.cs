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
            var directoryName = "webfolder"; // this is the local directory where we will save the downloaded files from blob storage.

            var storefrontName = "storefront1";   // this value should come from the apiClient.Name. I hardcoded a value here for testing locally
            var mainFileName = "";
            foreach (var file in files)
            {
                var fileName = file.Uri.ToString().Split("buyerweb/")[1];
                if(isMainFile(fileName))
                {
                    mainFileName = fileName; // need to save the mainFileName to a variable so we know which file to replace with the new app configs
                }
                tasks.Add(_blob.TransferBlobs("buyerweb", "$web", fileName, storefrontName, directoryName));
            }
            try
            {
                CreateDirectory(directoryName);
                await Task.WhenAll(tasks);
                await UpdateAppConfig(apiClient, mainFileName, storefrontName);
                await UpdateIndex(storefrontName);
            } finally
            {
                DeleteDirectory(directoryName);
            }
        }

        private async Task UpdateIndex(string storefrontName)
        {
            var path = $"{storefrontName}/index.html";
            var index = (await _blob.Get(path))
                .Replace("<base href=\"/\">", $"<base href='/{storefrontName}'/>")
                .Replace(storefrontPlaceholder, storefrontName);

            await _blob.Save(path, index, "text/html");
        }

        private async Task UpdateAppConfig(ApiClient apiClient, string mainFileName, string storefrontName = "")
        {
            var config = await _blob.Get<BuyerAppConfiguration>($"{storefrontName}/assets/appConfigs/headstartdemo-test.json");

            // Update our config file with values from the client ID
            //config.clientID = apiClient.ID;
            //config.appID = apiClient?.xp?.appID;
            //config.incrementorPrefix = apiClient?.xp?.incrementorPrefix;
            //config.sellerID = apiClient?.xp?.sellerID;
            //config.theme = apiClient?.xp?.theme;

            // for now testing with just these values
            config.appname = "this is an app";
            config.sellerName = "Brand new seller";
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
