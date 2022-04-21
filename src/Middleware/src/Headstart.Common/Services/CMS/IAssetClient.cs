using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using Headstart.Common.Extensions;
using ordercloud.integrations.library;
using Headstart.Common.Models.Headstart;
using Headstart.Common.Services.CMS.Models;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using SitecoreExtensions = Sitecore.Foundation.SitecoreExtensions.Extensions;

namespace Headstart.Common.Services.CMS
{
	public interface IAssetClient
	{
		Task<ImageAsset> CreateImage(AssetUpload asset);
		Task DeleteAsset(string id);
		Task<DocumentAsset> CreateDocument(AssetUpload asset);
		Task DeleteAssetByUrl(string assetUrl);
	}

	public class AssetClient : IAssetClient
	{
		private readonly IOrderCloudIntegrationsBlobService _blob;
		private readonly AppSettings _settings;

		public AssetClient(IOrderCloudIntegrationsBlobService blob, AppSettings settings)
		{
			try
			{
				_blob = blob;
				_settings = settings;
			}
			catch (Exception ex)
			{
				LoggingNotifications.LogApiResponseMessages(_settings, SitecoreExtensions.Helpers.GetMethodName(), "",
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace, ex);
			}
		}

		public async Task<ImageAsset> CreateImage(AssetUpload asset)
		{
			var container = _blob.Container.Name;
			var assetGuid = Guid.NewGuid().ToString();
			try
			{
				using (Image image = Image.FromStream(asset.File.OpenReadStream()))
				{
					var small = image.ResizeSmallerDimensionToTarget(100);
					var medium = image.ResizeSmallerDimensionToTarget(300);
					await Task.WhenAll(new[] {
						_blob.Save(assetGuid, medium.ToBytes(ImageFormat.Png), $@"image/png"),
						_blob.Save($@"{assetGuid}-s", small.ToBytes(ImageFormat.Png), $@"image/png")
					});
				}
			}
			catch (Exception ex)
			{
				LoggingNotifications.LogApiResponseMessages(_settings, SitecoreExtensions.Helpers.GetMethodName(), "",
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace, ex);
			}
			return new ImageAsset
			{
				Url = $@"{GetBaseUrl()}{container}/{assetGuid}",
				ThumbnailUrl = $@"{GetBaseUrl()}{container}/{assetGuid}-s"
			};
		}

		public async Task<DocumentAsset> CreateDocument(AssetUpload asset)
		{
			var container = _blob.Container.Name;
			var assetGuid = Guid.NewGuid().ToString();
			try
			{
				await _blob.Save(assetGuid, asset.File, @"application/pdf");
			}
			catch (Exception ex)
			{
				LoggingNotifications.LogApiResponseMessages(_settings, SitecoreExtensions.Helpers.GetMethodName(), "",
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace, ex);
			}
			return new DocumentAsset()
			{
				FileName = asset.Filename,
				Url = $@"{GetBaseUrl()}{container}/{assetGuid}"
			};
		}

		public async Task DeleteAsset(string id)
		{
			await _blob.Delete(id);
			try
			{
				await _blob.Delete($@"{id}-s");
			}
			catch { }
		}

		public async Task DeleteAssetByUrl(string assetUrl)
		{
			var id = GetAssetIDFromUrl(assetUrl);
			await _blob.Delete(id);
			try
			{
				await _blob.Delete($@"{id}-s");
			}
			catch (Exception ex)
			{
				LoggingNotifications.LogApiResponseMessages(_settings, SitecoreExtensions.Helpers.GetMethodName(), "",
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace, ex);
			}
		}

		private string GetAssetIDFromUrl(string url)
		{
			var parts = url.Split(@"/");
			return parts[parts.Length - 1];
		}

		private string GetBaseUrl()
		{
			return _settings.StorageAccountSettings.BlobPrimaryEndpoint.EndsWith("/") ? _settings.StorageAccountSettings.BlobPrimaryEndpoint : _settings.StorageAccountSettings.BlobPrimaryEndpoint + "/";
		}
	}
}