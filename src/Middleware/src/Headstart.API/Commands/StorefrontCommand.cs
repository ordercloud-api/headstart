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

            //await _blob.ListContainersWithPrefixAsync("$web", null);
            //1. Create the $web container
            await _blob.CreateContainerAsync("$web", false);

            //2. Get files of deployed buyer app so we can upload them to our contianer (within a subfolder)
            // Store files in our storage container vs azure devops so we already have access.
            // Implement in build process first
            var files = await _blob.GetBlobFiles("buyerweb");
            var tasks = new List<Task>();
            var directoryName = "webfolder"; // this is the folder where we save the downloaded files from blob storage.

            var storefrontName = "storefront1";
            var mainFileName = "";
            foreach (var file in files)
            {
                var fileName = file.Uri.ToString().Split("buyerweb/")[1];
                if(isMainFile(fileName))
                {
                    mainFileName = fileName;
                }
                tasks.Add(_blob.TransferBlobs("buyerweb", "$web", fileName, storefrontName, directoryName));
            }
            CreateDirectory(directoryName);
            await Task.WhenAll(tasks);
            await UpdateAppConfig(apiClient, mainFileName, storefrontName);
            await UpdateIndex(storefrontName);
            DeleteDirectory(directoryName);
        }

        private async Task UpdateIndex(string storefrontName)
        {
            var path = $"{storefrontName}/index.html";
            var index = (await _blob.Get(path))
                .Replace("<base href=\"/\">", $"<base href='/{storefrontName}'/>")
                .Replace(storefrontPlaceholder, storefrontName);

            try
            {
                await _blob.Save(path, index, "text/html");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private async Task UpdateAppConfig(ApiClient apiClient, string mainFileName, string storefrontName = "")
        {
            var config = await _blob.Get<BuyerAppConfiguration>($"{storefrontName}/assets/appConfigs/headstartdemo-test.json");
            //config.clientID = apiClient.ID;
            config.appname = "this is an app";
            //config.appID = apiClient?.xp?.appID;
            //config.incrementorPrefix = apiClient?.xp?.incrementorPrefix;
            //config.sellerID = apiClient?.xp?.sellerID;
            config.sellerName = "Brand new seller";
            //config.theme = apiClient?.xp?.theme;
            config.storefrontName = storefrontName;
            var originalMain = (await _blob.Get($"{storefrontName}/{originalMainFileName}"));
            var hasPlaceHolder = originalMain.Contains(envPlaceholder);
            Console.WriteLine(hasPlaceHolder);
            var updatedMain = originalMain
                .Replace(envPlaceholder, JsonConvert.SerializeObject(config));
            try
            {
                await Task.WhenAll(
                _blob.Save($"{storefrontName}/assets/appConfigs/headstartdemo-test.json", JsonConvert.SerializeObject(config), "application/json"),
                _blob.Save($"{storefrontName}/{mainFileName}", updatedMain, "application/x-javascript")
            );
            } catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
            
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
                try
                {
                    Directory.Delete(directoryName, true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

            }
        }
    }
}