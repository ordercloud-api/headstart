using System;
using OrderCloud.SDK;
using Newtonsoft.Json;
using Headstart.Common;
using Headstart.Models;
using System.Reflection;
using OrderCloud.Catalyst;
using Sitecore.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using ordercloud.integrations.library;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Foundation.SitecoreExtensions.MVC.Extenstions;

namespace Headstart.API.Commands
{
    public interface ISupplierSyncCommand
    {
        Task<JObject> GetOrderAsync(string ID, OrderType orderType, DecodedToken decodedToken);
    }

    public class SupplierSyncCommand : ISupplierSyncCommand
    {
        private readonly AppSettings _settings;
        private readonly IOrderCloudClient _oc;
        private WebConfigSettings _webConfigSettings = WebConfigSettings.Instance;

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
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
        }

        /// <summary>
        /// Public re-usable GetOrderAsync task method
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="orderType"></param>
        /// <param name="decodedToken"></param>
        /// <returns>The JObject response object for the Get Order process</returns>
        public async Task<JObject> GetOrderAsync(string ID, OrderType orderType, DecodedToken decodedToken)
        {
            var resp = new JObject();
            try
            {
                var me = await _oc.Me.GetAsync(accessToken: decodedToken.AccessToken);
                // quote orders often won't have a hyphen in their order IDs, so allowing ID to be a fallback. This value is determined subsequently for quotes.
                var supplierID = ID.Split("-").Length > 1 ? ID.Split("-")[1] : ID;
                if (orderType != OrderType.Quote)
                {
                    Require.That(decodedToken.CommerceRole == CommerceRole.Seller || supplierID == me.Supplier.ID, new ErrorCode($@"Unauthorized", $@"You are not authorized view this order.", 401));
                }
                try
                {
                    var type = Assembly.GetExecutingAssembly().GetTypeByAttribute<SupplierSyncAttribute>(attribute => attribute.SupplierID == supplierID) ??
                        Assembly.GetExecutingAssembly().GetTypeByAttribute<SupplierSyncAttribute>(attribute => attribute.SupplierID.Trim().Equals($@"Generic", StringComparison.OrdinalIgnoreCase));
                    if (type == null) 
                    {
                        var ex = new MissingMethodException($@"Command for {supplierID} is unavailable.");
                        LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
                        throw ex;
                    }


                    var command = (ISupplierSyncCommand)Activator.CreateInstance(type, _settings);
                    var method = command.GetType().GetMethod($@"GetOrderAsync", BindingFlags.Public | BindingFlags.Instance);
                    if (method == null)
                    {
                        var ex = new MissingMethodException($@"Get Order Method for {supplierID} is unavailable.");
                        LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
                        throw ex;
                    }
                    resp = await (Task<JObject>)method.Invoke(command, new object[] { ID, orderType, decodedToken });
                }
                catch (MissingMethodException mex)
                {
                    var ex = new Exception(JsonConvert.SerializeObject(new ApiError()
                    {
                        Data = new { decodedToken, OrderID = ID, OrderType = orderType },
                        ErrorCode = mex.Message,
                        Message = $@"Missing Method for: {supplierID ?? "Invalid Supplier"}. OrderID: {ID}, OrderType : {orderType}."
                    }));
                    LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", $@"{mex.Message}. {ex.Message}", mex.StackTrace, this, true);
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
            return resp;
        }
    }
}