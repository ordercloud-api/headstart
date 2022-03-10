using System;
using OrderCloud.SDK;
using Cosmonaut.Attributes;
using Newtonsoft.Json.Linq;
using Microsoft.Azure.WebJobs;
using Headstart.Common.Exceptions;
using ordercloud.integrations.library;
using ordercloud.integrations.library.helpers;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Foundation.SitecoreExtensions.MVC.Extensions;
using SitecoreExtensions = Sitecore.Foundation.SitecoreExtensions.Extensions;
using Newtonsoft.Json;

namespace Headstart.Common.Models
{
	[CosmosCollection("orchestrationlogs")]
	public class OrchestrationLog : CosmosObject
	{
		private readonly WebConfigSettings _webConfigSettings = WebConfigSettings.Instance;

		[Sortable]
		public OrchestrationErrorType? ErrorType { get; set; }

		public string Message { get; set; } = string.Empty;

		[Sortable]
		public LogLevel Level { get; set; }

		[Sortable]
		public string ResourceId { get; set; } = string.Empty;

		[Sortable]
		public string RecordId { get; set; } = string.Empty;

		[Sortable]
		public RecordType? RecordType { get; set; }

		[Sortable]
		public Action? Action { get; set; }

		[DocIgnore]
		public JObject Current { get; set; } = new JObject();

		[DocIgnore]
		public JObject Cache { get; set; } = new JObject();

		[DocIgnore]
		public JObject Diff { get; set; } = new JObject();

		public ApiError[] OrderCloudErrors { get; set; }

		public OrchestrationLog()
		{
		}

		public OrchestrationLog(WorkItem wi)
		{
			Action = wi.Action;
			Current = wi.Current;
			Cache = wi.Cache;
			RecordId = wi.RecordId;
			ResourceId = wi.ResourceId;
			RecordType = wi.RecordType;
			Level = LogLevel.Warn;

			var ResponseBody = JsonConvert.SerializeObject(this.ToJsonObject());
			LoggingNotifications.LogApiResponseMessages(_webConfigSettings.AppLogFileKey, SitecoreExtensions.Helpers.GetMethodName(), ResponseBody,
				LoggingNotifications.GetApiResponseMessagePrefixKey(), false);
		}

		public OrchestrationLog(OrderCloudException ex)
		{
			Level = LogLevel.Error;
			OrderCloudErrors = ex.Errors;
			var ResponseBody = JsonConvert.SerializeObject(this.ToJsonObject());
			LoggingNotifications.LogApiResponseMessages(_webConfigSettings.AppLogFileKey, SitecoreExtensions.Helpers.GetMethodName(), ResponseBody,
				LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace);
		}

		public OrchestrationLog(OrchestrationException ex)
		{
			LoggingNotifications.LogApiResponseMessages(_webConfigSettings.AppLogFileKey, SitecoreExtensions.Helpers.GetMethodName(), "",
				LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace);
		}

		public OrchestrationLog(FunctionFailedException ex)
		{
			LoggingNotifications.LogApiResponseMessages(_webConfigSettings.AppLogFileKey, SitecoreExtensions.Helpers.GetMethodName(), "",
				LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace);
		}

		public OrchestrationLog(Exception ex)
		{
			LoggingNotifications.LogApiResponseMessages(_webConfigSettings.AppLogFileKey, SitecoreExtensions.Helpers.GetMethodName(), "",
				LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace);
		}
	}
}