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
using Microsoft.TeamFoundation.SourceControl.WebApi;
namespace Headstart.API.Commands
{
    public interface IStorefrontCommand
    {
        Task DeployBuyerSite(string storeFrontName);
    }

    public class StorefrontCommand : IStorefrontCommand
    {
        private readonly AppSettings _settings;
        private readonly IOrderCloudIntegrationsBlobService _blob;


        public StorefrontCommand(IOrderCloudIntegrationsBlobService blob, AppSettings settings)
        {
            _settings = settings;
            _blob = blob;
        }

        public async Task DeployBuyerSite(string storeFrontName)
        {
            //await _blob.ListContainersWithPrefixAsync("$web", null);
            //1. Create the $web container
            await _blob.CreateContainerAsync("$web", false);



            //2. Get files of deployed buyer app so we can upload them to our contianer (within a subfolder)
            // Store files in our storage container vs azure devops so we already have access.
            // Implement in build process first

            // Need to authenticate
            var creds = new VssBasicCredential("username", "password");
            VssConnection connection = new VssConnection(new Uri(collectionUri), creds);

            using var client = connection.GetClient<GitHttpClient>();
            // https://stackoverflow.com/questions/54391120/how-to-download-the-latest-build-artifacts-from-azure-devops-programmatically
            var organization = "OrderCloud";
            var project = "Headstart";
            var definition = "172";
            var branchName = "development";

            var url = $"https://dev.azure.com/{organization}/{project}/_apis/build/latest/{definition}?branchName={branchName}&api-version=5.0-preview.1";

            var files = await url.GetAsync();

            Console.WriteLine(files);

            //4. Upload folder with file contents for the site being deployed into this $web folder

            //5. Check if a CDN profile exists. If not, create one.
            // Dont necessarily need to do this. Can still access site without CDN.
            // They will have to do this. Make it a step in getting started.
            // How do we get CDN endpoints from the profile.

            //6. Create CDN Endpoint within the CDN Profile and set the path to reference what was uploaded in step 3.
        }

        public static async void GetLatestBuild()
        {
            try
            {
                var personalaccesstoken = "PAT_FROM_WEBSITE";

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(
                        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                        Convert.ToBase64String(
                            System.Text.ASCIIEncoding.ASCII.GetBytes(
                                string.Format("{0}:{1}", "", personalaccesstoken))));

                    using (HttpResponseMessage response = client.GetAsync(
                                "https://dev.azure.com/{organization}/_apis/projects").Result)
                    {
                        response.EnsureSuccessStatusCode();
                        string responseBody = await response.Content.ReadAsStringAsync();
                        Console.WriteLine(responseBody);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
