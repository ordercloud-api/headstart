using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;
using System.Collections.Generic;

namespace ordercloud.integrations.library
{
    public interface IOrderCloudIntegrationsBlobService
    {
        CloudBlobClient Client { get; }
        CloudBlobContainer Container { get; }
        Task<T> Get<T>(string id);
        Task<string> Get(string id);
        Task Save(string reference, string blob, string fileType = null);
        Task Save(string reference, JObject blob, string fileType = null);
        Task Save(string reference, IFormFile blob, string fileType = null);
        Task Save(string reference, Stream file, string fileType = null);
        Task Save(string reference, byte[] bytes, string fileType = null, string cacheControl = null);
        Task Save(BlobBase64Image base64Image);
        Task Delete(string id);
        Task DeleteContainer();
        Task<List<IListBlobItem>> GetBlobFiles(string containerName);
        Task<bool> CreateContainerAsync(string containerName, bool isPublic);
        Task<List<CloudBlobContainer>> ListContainers();
        Task CopyBlobs();
        Task TransferBlobs(string sourceContainer, string destinationContainer, string blobName);

    }
    public class OrderCloudIntegrationsBlobService : IOrderCloudIntegrationsBlobService
    {
        public CloudBlobClient Client { get; }
        public CloudBlobContainer Container { get; }
        private readonly BlobServiceConfig _config;

        // BlobServiceConfig must be required for this service to function properly
        public OrderCloudIntegrationsBlobService() : this(new BlobServiceConfig())
        {

        }

        public OrderCloudIntegrationsBlobService(BlobServiceConfig config)
        {
            _config = config;
            try
            {
                if (config.ConnectionString == null)
                    throw new Exception("Connection string not supplied");
                if (config.Container == null)
                    throw new Exception("Blob container not specified");

                CloudStorageAccount.TryParse(config.ConnectionString, out var storage);
                Client = storage.CreateCloudBlobClient();
                if (config.Container != null)
                    Container = Client.GetContainerReference(config.Container);
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}. The blob service must be invoked with a valid configuration");
            }
        }
        private async Task Init()
        {
            var created = await Container.CreateIfNotExistsAsync();
            if (created)
            {
                var permissions = await Container.GetPermissionsAsync();
                permissions.PublicAccess = _config.AccessType;
                await Container.SetPermissionsAsync(permissions);

                var properties = await Client.GetServicePropertiesAsync();
                properties.Cors.CorsRules.Add(new CorsRule
                {
                    AllowedHeaders = { "*" },
                    AllowedOrigins = { "*" },
                    AllowedMethods =
                        CorsHttpMethods.Options |
                        CorsHttpMethods.Get |
                        CorsHttpMethods.Put |
                        CorsHttpMethods.Post |
                        CorsHttpMethods.Head |
                        CorsHttpMethods.Delete |
                        CorsHttpMethods.Merge,
                    ExposedHeaders = { "*" },
                    MaxAgeInSeconds = (int)TimeSpan.FromHours(1).TotalSeconds
                });
                await Client.SetServicePropertiesAsync(properties);
            }
        }

        public async Task<string> Get(string id)
        {
            await this.Init();
            var value = await Container.GetBlockBlobReference(id).DownloadTextAsync();
            return value;
        }

        public async Task<bool> CreateContainerAsync(string containerName, bool isPublic)
        {
            // Create the container if it doesn't exist.
            await this.Init();
            var blobContainer = Client.GetContainerReference(containerName);
            if (isPublic)
            {
                var returnData = await blobContainer.CreateIfNotExistsAsync();
                if (returnData)
                    await blobContainer.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
                return returnData;
            }
            return await blobContainer.CreateIfNotExistsAsync();
        }

        public async Task<List<IListBlobItem>> GetBlobFiles(string containerName)
        {
            await this.Init();
            var blobContainer = Client.GetContainerReference(containerName);
            List<IListBlobItem> results = new List<IListBlobItem>();
            BlobContinuationToken continuationToken = null;
            do
            {
                //ListBlobsSegmentedAsync(string prefix, bool useFlatBlobListing, BlobListingDetails blobListingDetails, int? maxResults, BlobContinuationToken currentToken, BlobRequestOptions options, OperationContext operationContext);
                var response = await blobContainer.ListBlobsSegmentedAsync(string.Empty, true, BlobListingDetails.None, int.MaxValue, continuationToken, null, null);
                continuationToken = response.ContinuationToken;
                results.AddRange(response.Results);
            } while (continuationToken != null);

            return results;
        }

        public async Task TransferBlobs(string sourceContainer, string destinationContainer, string blobName)
        {
            string directoryName = "webfolder";
            await this.Init();
            CreateDirectory(directoryName);
            await DownloadBlob(sourceContainer, blobName);
            await UploadBlob(destinationContainer, blobName);
            DeleteDirectory(directoryName);
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
            if (!Directory.Exists(directoryName))
            {
                Directory.Delete(directoryName);
            }
        }

        private async Task DownloadBlob(string sourceContainer, string blobName)
        {
            try
            {
                CloudBlobContainer sourceBlobContainer = Client.GetContainerReference(sourceContainer);
                ICloudBlob sourceBlob = await sourceBlobContainer.GetBlobReferenceFromServerAsync(blobName);
                if(!Directory.Exists(_webfolder))
                {
                    Directory.CreateDirectory(_webfolder);
                }

                await sourceBlob.DownloadToFileAsync(_webfolder + "/" + blobName.Replace("/", "_"), System.IO.FileMode.Create);

                //modifying the app configs.
                var configTest = "{'hostedApp': true, 'appname': 'headstartDemo' }";
                //File.WriteAllText("assets_appConfigs_defaultbuyer-test.json", configTest);
            } catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw ex;
            } 
            
        }

        private async Task UploadBlob(string destinationContainer, string blobName)
        {
            string filepath = "https://headstartdemo.blob.core.windows.net/buyerweb";
            CloudBlobContainer destBlobContainer = Client.GetContainerReference(destinationContainer);
            var path = blobName.Replace("_", "/");
            CloudBlockBlob destBlob = destBlobContainer.GetBlockBlobReference(path);
            var contentType = GetContentType(blobName);
            if(contentType != null)
            {
                destBlob.Properties.ContentType = contentType;
            }
            await destBlob.UploadFromFileAsync(_webfolder + "/" + blobName.Replace("/", "_"));
        }

        private string GetContentType(string fileName)
        {
            if (fileName.EndsWith(".txt")) return "text/plain";
            else if (fileName.EndsWith(".html")) return "text/html";
            else if (fileName.EndsWith(".js")) return "application/x-javascript";
            else if (fileName.EndsWith(".css")) return "text/css";
            else if (fileName.EndsWith(".json")) return "application/json";
            else if (fileName.EndsWith(".ico")) return "image/x-icon";
            else if (fileName.EndsWith(".jpg")) return "image/jpeg";
            else if (fileName.EndsWith(".png")) return "image/png";
            else if (fileName.EndsWith(".svg")) return "image/svg+xml";
            else if (fileName.EndsWith(".md")) return "application/octet-stream";
            else if (fileName.EndsWith(".config")) return "application/xml";
            else return null;
        }

        public async Task CopyBlobs()
        {
            var source = "buyerWeb";
            var dest = "$web";
            var sourceContainer = Client.GetContainerReference(source);
            var destContainer = Client.GetContainerReference(dest);

            CloudBlockBlob destBlob = destContainer.GetBlockBlobReference("index.html");
            await destBlob.StartCopyAsync(new Uri(GetSharedAccessUri("index.html", sourceContainer)));
        }

        // Create a SAS token for the source blob, to enable it to be read by the StartCopyAsync method
        private static string GetSharedAccessUri(string blobName, CloudBlobContainer container)
        {
            DateTime toDateTime = DateTime.Now.AddMinutes(60);

            SharedAccessBlobPolicy policy = new SharedAccessBlobPolicy
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessStartTime = null,
                SharedAccessExpiryTime = new DateTimeOffset(toDateTime)
            };

            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);
            string sas = blob.GetSharedAccessSignature(policy);

            return blob.Uri.AbsoluteUri + sas;
        }

        public async Task<List<CloudBlobContainer>> ListContainers()
        {
            await this.Init();
            BlobContinuationToken continuationToken = null;
            List<CloudBlobContainer> results = new List<CloudBlobContainer>();
            do
            {
                var response = await Client.ListContainersSegmentedAsync(continuationToken);
                continuationToken = response.ContinuationToken;
                results.AddRange(response.Results);
            } while (continuationToken != null);
            return results;
        }

        public virtual async Task<T> Get<T>(string id)
        {
            await this.Init();
            var obj = await Container.GetBlockBlobReference(id).DownloadTextAsync();
            return JsonConvert.DeserializeObject<T>(obj);
        }

        public async Task Save(string reference, JObject blob, string fileType = null)
        {
            await this.Init();
            var block = Container.GetBlockBlobReference(reference);
            if (fileType != null)
                block.Properties.ContentType = fileType;
            await block.UploadTextAsync(JsonConvert.SerializeObject(blob), Encoding.UTF8, AccessCondition.GenerateEmptyCondition(),
                new BlobRequestOptions() { SingleBlobUploadThresholdInBytes = 4 * 1024 * 1024 }, new OperationContext());
        }
        public async Task Save(string reference, byte[] bytes, string fileType = null, string cacheControl = null)
        {
            await this.Init();
            var block = Container.GetBlockBlobReference(reference);
            if (fileType != null)
                block.Properties.ContentType = fileType;
            await block.UploadFromByteArrayAsync(bytes, 0, bytes.Length);
        }

        public async Task Save(string reference, string blob, string fileType = null)
        {
            await this.Init();
            var block = Container.GetBlockBlobReference(reference);
            if (fileType != null)
                block.Properties.ContentType = fileType;
            await block.UploadTextAsync(blob, Encoding.UTF8, AccessCondition.GenerateEmptyCondition(), new BlobRequestOptions() { SingleBlobUploadThresholdInBytes = 4 * 1024 * 1024 }, new OperationContext());
        }

        public async Task Save(string reference, IFormFile blob, string fileType = null)
        {
            await this.Init();
            var block = Container.GetBlockBlobReference(reference);
            block.Properties.ContentType = fileType ?? blob.ContentType;
            await using var stream = blob.OpenReadStream();
            await block.UploadFromStreamAsync(stream);
        }
        public async Task Save(string reference, Stream file, string fileType = null)
        {
            await this.Init();
            var block = Container.GetBlockBlobReference(reference);
            block.Properties.ContentType = fileType;
            await block.UploadFromStreamAsync(file);
        }

        public async Task Save(BlobBase64Image base64Image)
        {
            await this.Init();
            var block = Container.GetBlockBlobReference(base64Image.Reference);
            block.Properties.ContentType = base64Image.ContentType;
            await block.UploadFromByteArrayAsync(base64Image.Bytes, 0, base64Image.Bytes.Length);
        }

        public async Task Delete(string id)
        {
            await this.Init();
            await Container.GetBlockBlobReference(id).DeleteIfExistsAsync();
        }

        public async Task DeleteContainer()
        {
            await Container.DeleteAsync();
        }
    }
}
