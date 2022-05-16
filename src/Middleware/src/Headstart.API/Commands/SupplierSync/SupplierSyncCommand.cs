using System;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Headstart.Common;
using Headstart.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrderCloud.Catalyst;
using OrderCloud.Integrations.Library;
using OrderCloud.SDK;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using SitecoreExtensions = Sitecore.Foundation.SitecoreExtensions.Extensions;

namespace Headstart.API.Commands
{
	public interface ISupplierSyncCommand
	{
		Task<JObject> GetOrderAsync(string id, OrderType orderType, DecodedToken decodedToken);
	}

	public class SupplierSyncCommand : ISupplierSyncCommand
	{
		private readonly AppSettings settings;
		private readonly IOrderCloudClient oc;

		/// <summary>
		/// The IOC based constructor method for the SupplierSyncCommand class object with Dependency Injection
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="oc"></param>
		public SupplierSyncCommand(AppSettings settings, IOrderCloudClient oc)
		{
			try
			{
				this.settings = settings;
				this.oc = oc;
			}
			catch (Exception ex)
			{
				LoggingNotifications.LogApiResponseMessages(this.settings.LogSettings, SitecoreExtensions.Helpers.GetMethodName(), "",
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace, ex);
			}
		}

		/// <summary>
		/// Public re-usable GetOrderAsync task method
		/// </summary>
		/// <param name="id"></param>
		/// <param name="orderType"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The JObject object for the Get Order process</returns>
		public async Task<JObject> GetOrderAsync(string id, OrderType orderType, DecodedToken decodedToken)
		{
            var me = await oc.Me.GetAsync(accessToken: decodedToken.AccessToken);

            // Quote orders often won't have a hyphen in their order IDs, so allowing ID to be a fallback. This value is determined subsequently for quotes.
            var supplierID = id.Split("-").Length > 1 ? id.Split("-")[1] : id;
			if (orderType != OrderType.Quote)
			{
                Require.That(decodedToken.CommerceRole == CommerceRole.Seller || supplierID == me.Supplier.ID, new ErrorCode("Unauthorized", $"You are not authorized view this order", HttpStatusCode.Unauthorized));
			}
			try
			{
				var type = 
					Assembly.GetExecutingAssembly().GetTypeByAttribute<SupplierSyncAttribute>(attribute => attribute.SupplierID == supplierID) ??
					Assembly.GetExecutingAssembly().GetTypeByAttribute<SupplierSyncAttribute>(attribute => attribute.SupplierID == "Generic");
				if (type == null) 
				{
					throw new MissingMethodException($"Command for {supplierID} is unavailable");
				}

                var command = (ISupplierSyncCommand)Activator.CreateInstance(type, settings);
                var method = command.GetType().GetMethod($"GetOrderAsync", BindingFlags.Public | BindingFlags.Instance);
				if (method == null)
				{
					throw new MissingMethodException($"Get Order Method for {supplierID} is unavailable");
				}

                return await (Task<JObject>)method.Invoke(command, new object[] { id, orderType, decodedToken });
			}
			catch (MissingMethodException mex)
			{
				throw new Exception(JsonConvert.SerializeObject(new ApiError()
				{
                    Data = new { decodedToken, OrderID = id, OrderType = orderType },
					ErrorCode = mex.Message,
                    Message = $"Missing Method for: {supplierID ?? "Invalid Supplier"}",
				}));
			}
		}
	}
}
