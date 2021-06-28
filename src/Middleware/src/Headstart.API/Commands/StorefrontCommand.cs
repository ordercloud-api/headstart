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
            //var client = await _oc.ApiClients.CreateAsync(apiClient);

            //await _blob.ListContainersWithPrefixAsync("$web", null);
            //1. Create the $web container
            await _blob.CreateContainerAsync("$web", false);

            //2. Get files of deployed buyer app so we can upload them to our contianer (within a subfolder)
            // Store files in our storage container vs azure devops so we already have access.
            // Implement in build process first
            var files = await _blob.GetBlobFiles("buyerweb");
            var tasks = new List<Task>();
            foreach(var file in files)
            {
                var name = file.Uri.ToString().Split("buyerweb/")[1];
                tasks.Add(_blob.TransferBlobs("buyerweb", "$web", name));
            }
            await Task.WhenAll(tasks);



            var configToUpdate = files.Find(file => file.Uri.ToString() == "https://headstartdemo.blob.core.windows.net/buyerweb/assets/appConfigs/defaultbuyer-test.json");

            //await _blob.CopyBlobs();
            //await _blob.Save("test", configToUpdate.ToString());

            //Console.WriteLine("it worked?");

            //4. Upload folder with file contents for the site being deployed into this $web folder

            //5. Check if a CDN profile exists. If not, create one.
            // Dont necessarily need to do this. Can still access site without CDN.
            // They will have to do this. Make it a step in getting started.
            // How do we get CDN endpoints from the profile.

            //6. Create CDN Endpoint within the CDN Profile and set the path to reference what was uploaded in step 3.
        }
    }
}
