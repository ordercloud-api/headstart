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
using Flurl.Http;

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
        Task TransferBlobs(string sourceContainer, string destinationContainer, string blobName, string storefrontName);

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

        public async Task TransferBlobs(string sourceContainer, string destinationContainer, string blobName, string storefrontName)
        {
            // Download all files from blob storage to this folder
            // Upload all these files in this folder to $web in blob storage
            await this.Init();
            byte[] file = await CopyBlob(sourceContainer, blobName);
            await UploadBlob(destinationContainer, blobName, storefrontName, file);
        }


        private async Task<byte[]> CopyBlob(string sourceContainer, string blobName)
        {
            CloudBlobContainer sourceBlobContainer = Client.GetContainerReference(sourceContainer);
            ICloudBlob sourceBlob = await sourceBlobContainer.GetBlobReferenceFromServerAsync(blobName);
            var sasToken = sourceBlob.GetSharedAccessSignature(new SharedAccessBlobPolicy()
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(1)//Assuming you want the link to expire after 1 hour
            });
            var blobUrl = string.Format("{0}{1}", sourceBlob.Uri.AbsoluteUri, sasToken);
            var file = await blobUrl.GetBytesAsync();
            return file;
        }

        private async Task UploadBlob(string destinationContainer, string blobName, string storefrontName, byte[] file)
        {
            var blobLocation = storefrontName + "/" + blobName.Replace("_", "/");

            CloudBlobContainer destBlobContainer = Client.GetContainerReference(destinationContainer);
            CloudBlockBlob destBlob = destBlobContainer.GetBlockBlobReference(blobLocation);
            var contentType = GetContentType(blobName);
            if(contentType != null)
            {
                destBlob.Properties.ContentType = contentType;
            }
            await destBlob.UploadFromByteArrayAsync(file, 0, file.Length);
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
