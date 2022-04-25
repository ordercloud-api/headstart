using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;
using Sitecore.Foundation.SitecoreExtensions.MVC.Extensions;

namespace Sitecore.Foundation.SitecoreExtensions.Extensions
{
	public interface ISitecoreIntegrationsBlobService
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
		Task Delete(string id);
		Task DeleteContainer();
	}

	public class SitecoreIntegrationsBlobService : ISitecoreIntegrationsBlobService
	{
		private bool IsInitialized = false;
		private readonly BlobServiceConfig _config;
		public CloudBlobClient Client { get; }
		public CloudBlobContainer Container { get; }

		/// <summary>
		/// The IOC based constructor method for the SitecoreIntegrationsBlobService class object with Dependency Injection
		/// </summary>
		/// <param name="config"></param>
		/// <exception cref="Exception"></exception>
		public SitecoreIntegrationsBlobService(BlobServiceConfig config)
		{
			_config = config;
			try
			{
				if (config.ConnectionString == null)
				{
					throw new Exception(@"The Connection string not supplied.");
				}
				if (config.ContainerName == null)
				{
					throw new Exception(@"The Blob container not specified.");
				}

				CloudStorageAccount.TryParse(config.ConnectionString, out var storage);
				Client = storage.CreateCloudBlobClient();
				if (config.ContainerName != null)
				{
					Container = Client.GetContainerReference(config.ContainerName);
				}
			}
			catch (Exception ex)
			{
				throw new Exception($@"{ex.Message}. The blob service must be invoked with a valid configuration.");
			}
		}

		/// <summary>
		/// Private Initialize task method for initializing the custom wrapper for the OrderCloudIntegrationsBlobService
		/// </summary>
		/// <returns></returns>
		private async Task Init()
		{
			if (IsInitialized)
			{
				return;
			}

			var created = await Container.CreateIfNotExistsAsync();
			if (created)
			{
				var permissions = await Container.GetPermissionsAsync();
				permissions.PublicAccess = _config.AccessType;
				await Container.SetPermissionsAsync(permissions);

				var properties = await Client.GetServicePropertiesAsync();
				properties.Cors.CorsRules.Add(new CorsRule
				{
					AllowedHeaders =
					{
						"*"
					},
					AllowedOrigins =
					{
						"*"
					},
					AllowedMethods =
						CorsHttpMethods.Options |
						CorsHttpMethods.Get |
						CorsHttpMethods.Put |
						CorsHttpMethods.Post |
						CorsHttpMethods.Head |
						CorsHttpMethods.Delete |
						CorsHttpMethods.Merge,
					ExposedHeaders =
					{
						"*"
					},
					MaxAgeInSeconds = (int)TimeSpan.FromHours(1).TotalSeconds
				});
				await Client.SetServicePropertiesAsync(properties);
			}
			IsInitialized = true;
		}

		/// <summary>
		/// Public re-usable Get task method
		/// </summary>
		/// <param name="id"></param>
		/// <returns>The string response value from the OrderCloudIntegrationsBlobService.Get() process</returns>
		public async Task<string> Get(string id)
		{
			await this.Init();
			var value = await Container.GetBlockBlobReference(id).DownloadTextAsync();
			return value;
		}

		/// <summary>
		/// Public re-usable Get task method
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="id"></param>
		/// <returns>The T object response value from the OrderCloudIntegrationsBlobService.Get() process</returns>
		public virtual async Task<T> Get<T>(string id)
		{
			await this.Init();
			var obj = await Container.GetBlockBlobReference(id).DownloadTextAsync();
			return JsonConvert.DeserializeObject<T>(obj);
		}

		/// <summary>
		/// Public re-usable GetAppendBlobReference task method
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns>The CloudAppendBlob object value from the OrderCloudIntegrationsBlobService.GetAppendBlobReference() process</returns>
		public async Task<CloudAppendBlob> GetAppendBlobReference(string fileName)
		{
			await this.Init();
			return Container.GetAppendBlobReference(fileName);
		}

		/// <summary>
		/// Public re-usable GetBlobReference task method
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns>The CloudBlob object value from the OrderCloudIntegrationsBlobService.GetBlobReference() process</returns>
		public async Task<CloudBlob> GetBlobReference(string fileName)
		{
			await this.Init();
			return Container.GetBlobReference(fileName);
		}

		/// <summary>
		/// Public re-usable Save task method
		/// </summary>
		/// <param name="reference"></param>
		/// <param name="blob"></param>
		/// <param name="fileType"></param>
		/// <returns></returns>
		public async Task Save(string reference, JObject blob, string fileType = null)
		{
			await this.Init();
			var block = Container.GetBlockBlobReference(reference);
			if (fileType != null)
			{
				block.Properties.ContentType = fileType;
			}
			await block.UploadTextAsync(JsonConvert.SerializeObject(blob), Encoding.UTF8, AccessCondition.GenerateEmptyCondition(),
				new BlobRequestOptions() { SingleBlobUploadThresholdInBytes = 4 * 1024 * 1024 }, new OperationContext());
		}

		/// <summary>
		/// Public re-usable Save task method
		/// </summary>
		/// <param name="reference"></param>
		/// <param name="bytes"></param>
		/// <param name="fileType"></param>
		/// <param name="cacheControl"></param>
		/// <returns></returns>
		public async Task Save(string reference, byte[] bytes, string fileType = null, string cacheControl = null)
		{
			await this.Init();
			var block = Container.GetBlockBlobReference(reference);
			if (fileType != null)
			{
				block.Properties.ContentType = fileType;
			}
			await block.UploadFromByteArrayAsync(bytes, 0, bytes.Length);
		}


		/// <summary>
		/// Public re-usable Save task method
		/// </summary>
		/// <param name="reference"></param>
		/// <param name="blob"></param>
		/// <param name="fileType"></param>
		/// <returns></returns>
		public async Task Save(string reference, string blob, string fileType = null)
		{
			await this.Init();
			var block = Container.GetBlockBlobReference(reference);
			if (fileType != null)
			{
				block.Properties.ContentType = fileType;
			}
			await block.UploadTextAsync(blob, Encoding.UTF8, AccessCondition.GenerateEmptyCondition(), new BlobRequestOptions() { SingleBlobUploadThresholdInBytes = 4 * 1024 * 1024 }, new OperationContext());
		}

		/// <summary>
		/// Public re-usable Save task method
		/// </summary>
		/// <param name="reference"></param>
		/// <param name="blob"></param>
		/// <param name="fileType"></param>
		/// <returns></returns>
		public async Task Save(string reference, IFormFile blob, string fileType = null)
		{
			await this.Init();
			var block = Container.GetBlockBlobReference(reference);
			block.Properties.ContentType = fileType ?? blob.ContentType;
			var stream = blob.OpenReadStream();
			await block.UploadFromStreamAsync(stream);
		}

		/// <summary>
		/// Public re-usable Save task method
		/// </summary>
		/// <param name="reference"></param>
		/// <param name="file"></param>
		/// <param name="fileType"></param>
		/// <returns></returns>
		public async Task Save(string reference, Stream file, string fileType = null)
		{
			await this.Init();
			var block = Container.GetBlockBlobReference(reference);
			block.Properties.ContentType = fileType;
			await block.UploadFromStreamAsync(file);
		}

		/// <summary>
		/// Public re-usable Delete task method
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public async Task Delete(string id)
		{
			await this.Init();
			await Container.GetBlockBlobReference(id).DeleteIfExistsAsync();
		}

		/// <summary>
		/// Public re-usable DeleteContainer task method
		/// </summary>
		/// <returns></returns>
		public async Task DeleteContainer()
		{
			await Container.DeleteAsync();
		}
	}
}