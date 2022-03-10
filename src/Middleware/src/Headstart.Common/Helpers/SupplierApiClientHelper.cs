using System;
using OrderCloud.SDK;
using System.Threading.Tasks;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Foundation.SitecoreExtensions.MVC.Extensions;
using SitecoreExtensions = Sitecore.Foundation.SitecoreExtensions.Extensions;

namespace Headstart.Common.Helpers
{
	public interface ISupplierApiClientHelper
	{
		Task<ApiClient> GetSupplierApiClient(string supplierId, string token);
	}

	public class SupplierApiClientHelper : ISupplierApiClientHelper
	{
		private readonly IOrderCloudClient _oc;
		private readonly AppSettings _settings;
		private readonly WebConfigSettings _webConfigSettings = WebConfigSettings.Instance;

		public SupplierApiClientHelper(AppSettings settings, IOrderCloudClient oc)
		{
			try
			{
				_settings = settings;
				_oc = oc;
			}
			catch (Exception ex)
			{
				LoggingNotifications.LogApiResponseMessages(_webConfigSettings.AppLogFileKey, SitecoreExtensions.Helpers.GetMethodName(), "",
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex?.StackTrace);
			}
		}

		/// <summary>
		/// GET ApiClient using the xp value on supplier
		/// </summary>
		/// <param name="supplierId"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		public async Task<ApiClient> GetSupplierApiClient(string supplierId, string token)
		{
			var supplierDetails = await _oc.Suppliers.GetAsync(supplierId);
			try
			{
				ApiClient apiClient = await _oc.ApiClients.GetAsync(supplierDetails?.xp?.ApiClientID);
				// If ApiClient exists, return it
				if (apiClient?.ID == null)
				{
					// In some cases, a null xp value was returning a 'blank' api client in the get request
					return await HandleError(supplierDetails, token);
				}
				return apiClient;
			}
			catch (Exception ex)
			{
				LoggingNotifications.LogApiResponseMessages(_webConfigSettings.AppLogFileKey, SitecoreExtensions.Helpers.GetMethodName(), 
					$@"Error occurred with the params supplierId: {supplierId} and token: {token}.",
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace);
				// else create and return the new api client
				return await HandleError(supplierDetails, token);
			}
		}

		private async Task<ApiClient> HandleError(Supplier supplierDetails, string token)
		{
			var supplierClient = await _oc.ApiClients.CreateAsync(new ApiClient()
			{
				AppName = $@"Integration Client {supplierDetails.Name}",
				Active = true,
				DefaultContextUserName = $@"dev_{supplierDetails.ID}",
				ClientSecret = _settings.OrderCloudSettings.MiddlewareClientSecret,
				AccessTokenDuration = 600,
				RefreshTokenDuration = 43200,
				AllowAnyBuyer = false,
				AllowAnySupplier = false,
				AllowSeller = false,
				IsAnonBuyer = false,
			}, token);

			// Assign Supplier API Client to new supplier
			await _oc.ApiClients.SaveAssignmentAsync(new ApiClientAssignment()
			{
				ApiClientID = supplierClient.ID,
				SupplierID = supplierDetails.ID
			}, token);
			// Update supplierXp to contain the new api client value
			await _oc.Suppliers.PatchAsync(supplierDetails.ID, new PartialSupplier { xp = new { ApiClientID = supplierClient.ID } });
			return supplierClient;
		}
	}
}