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

        public async Task DeployBuyerSite(ApiClient apiClient = null)
        {
            //First create the api client
            var client = await _oc.ApiClients.CreateAsync(apiClient);

            //await _blob.ListContainersWithPrefixAsync("$web", null);
            //1. Create the $web container
            await _blob.CreateContainerAsync("$web", false);

            //2. Get files of deployed buyer app so we can upload them to our contianer (within a subfolder)
            // Store files in our storage container vs azure devops so we already have access.
            // Implement in build process first
            var files = await _blob.GetBlobFiles("buyerweb");
            var tasks = new List<Task>();
            var directoryName = "webfolder"; // this is the folder where we save the downloaded files from blob storage.

            foreach (var file in files)
            {
                var name = file.Uri.ToString().Split("buyerweb/")[1];
                tasks.Add(_blob.TransferBlobs("buyerweb", "$web", name, directoryName));
            }
            CreateDirectory(directoryName);
            await Task.WhenAll(tasks);
            await UpdateAppConfig(apiClient);
            DeleteDirectory(directoryName);
        }

        private async Task UpdateAppConfig(ApiClient apiClient = null)
        {
            var config = await _blob.Get<BuyerAppConfiguration>("assets/appConfigs/headstartdemo-test.json");
            config.clientID = apiClient.ID;
            config.appname = apiClient?.xp?.appname;
            config.appID = apiClient?.xp?.appID;
            config.incrementorPrefix = apiClient?.xp?.incrementorPrefix;
            config.sellerID = apiClient?.xp?.sellerID;
            config.sellerName = "A whole new seller"; 
            config.theme = apiClient?.xp?.theme;
            await _blob.Save("assets/appConfigs/headstartdemo-test.json", JsonConvert.SerializeObject(config));

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
                    Directory.Delete(directoryName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

            }
        }
    }
}
