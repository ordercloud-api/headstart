using System;
using System.Net;
using OrderCloud.SDK;
using Newtonsoft.Json;
using Headstart.Common;
using System.Reflection;
using OrderCloud.Catalyst;
using Sitecore.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using ordercloud.integrations.library;
using Headstart.Common.Models.Headstart;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Foundation.SitecoreExtensions.MVC.Extensions;

namespace Headstart.API.Commands.SupplierSync
{
	public interface ISupplierSyncCommand
	{
		Task<JObject> GetOrderAsync(string id, OrderType orderType, DecodedToken decodedToken);
	}

	public class SupplierSyncCommand : ISupplierSyncCommand
	{
		private readonly AppSettings _settings;
		private readonly IOrderCloudClient _oc;
		private readonly ConfigSettings _configSettings = ConfigSettings.Instance;

		/// <summary>
		/// The IOC based constructor method for the SupplierSyncCommand class object with Dependency Injection
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="oc"></param>
		public SupplierSyncCommand(AppSettings settings, IOrderCloudClient oc)
		{
			try
			{
				_settings = settings;
				_oc = oc;
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Public re-usable GetOrderAsync task method
		/// </summary>
		/// <param name="id"></param>
		/// <param name="orderType"></param>
		/// <param name="decodedToken"></param>
		/// <returns>The JObject response object for the Get Order process</returns>
		public async Task<JObject> GetOrderAsync(string id, OrderType orderType, DecodedToken decodedToken)
		{
			var resp = new JObject();
			try
			{
				var me = await _oc.Me.GetAsync(accessToken: decodedToken.AccessToken);
				// quote orders often won't have a hyphen in their order ids, so allowing ID to be a fallback. This value is determined subsequently for quotes.
				var supplieId = id.Split("-").Length > 1 ? id.Split("-")[1] : id;
				if (orderType != OrderType.Quote)
				{
					Require.That(decodedToken.CommerceRole == CommerceRole.Seller || supplieId == me.Supplier.ID, new ErrorCode(@"Unauthorized", @"You are not authorized view this order.", HttpStatusCode.Unauthorized));
				}
				try
				{
					var type = Assembly.GetExecutingAssembly().GetTypeByAttribute<SupplierSyncAttribute>(attribute => attribute.SupplierId == supplieId) ??
					           Assembly.GetExecutingAssembly().GetTypeByAttribute<SupplierSyncAttribute>(attribute => attribute.SupplierId.Trim().Equals(@"Generic", StringComparison.OrdinalIgnoreCase));
					if (type == null) 
					{
						var ex = new MissingMethodException(@"Command for {supplierID} is unavailable.");
						LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
						throw ex;
					}


					var command = (ISupplierSyncCommand)Activator.CreateInstance(type, _settings);
					var method = command.GetType().GetMethod(@"GetOrderAsync", BindingFlags.Public | BindingFlags.Instance);
					if (method == null)
					{
						var ex = new MissingMethodException($@"Get Order Method for {supplieId} is unavailable.");
						LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
						throw ex;
					}
					resp = await (Task<JObject>)method.Invoke(command, new object[]
					{
						id, 
						orderType, 
						decodedToken
					});
				}
				catch (MissingMethodException mex)
				{
					var ex = new Exception(JsonConvert.SerializeObject(new ApiError()
					{
						Data = new
						{
							decodedToken, OrderID = id, 
							OrderType = orderType
						},
						ErrorCode = mex.Message,
						Message = $@"Missing Method for: {supplieId ?? "Invalid Supplier"}. OrderID: {id}, OrderType : {orderType}."
					}));
					LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", $@"{mex.Message}. {ex.Message}", mex.StackTrace, this, true);
					throw ex;
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}
	}
}