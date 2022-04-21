using System;
using Flurl.Http;
using Newtonsoft.Json;
using OrderCloud.Catalyst;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using SitecoreExtensions = Sitecore.Foundation.SitecoreExtensions.Extensions;

namespace Headstart.Common.Exceptions
{
	public class ProcessResultException
	{
		private string Message { get; set; } = string.Empty;
		private dynamic ResponseBody { get; set; }

		public ProcessResultException(Exception ex, AppSettings settings)
		{
			Message = ex.Message;
			ResponseBody = string.Empty;
			LoggingNotifications.LogApiResponseMessages(settings.LogSettings, SitecoreExtensions.Helpers.GetMethodName(), ResponseBody,
				LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace, ex);
		}

		public ProcessResultException(CatalystBaseException ex, AppSettings settings)
		{
			Message = ex.Errors[0].Message;
			try
			{
				ResponseBody = JsonConvert.SerializeObject(ex?.Errors);
				var origEx = new Exception(ex.Message, ex);
				LoggingNotifications.LogApiResponseMessages(settings.LogSettings, SitecoreExtensions.Helpers.GetMethodName(), Message,
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ResponseBody, ex.StackTrace, origEx);
			}
			catch (Exception ex1)
			{
				ResponseBody = @"Error while trying to parse response body.";
				LoggingNotifications.LogApiResponseMessages(settings.LogSettings, SitecoreExtensions.Helpers.GetMethodName(), ResponseBody,
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex1.Message, ex1.StackTrace, ex1);
			}
		}

		public ProcessResultException(FlurlHttpException ex, AppSettings settings)
		{
			Message = ex.Message;
			try
			{
				ResponseBody = ex.GetResponseJsonAsync().Result;
				var origEx = new Exception(ex.Message, ex);
				LoggingNotifications.LogApiResponseMessages(settings.LogSettings, SitecoreExtensions.Helpers.GetMethodName(), Message,
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ResponseBody, ex?.StackTrace, origEx);
			}
			catch (Exception ex1)
			{
				ResponseBody = @"Error while trying to parse response body.";
				LoggingNotifications.LogApiResponseMessages(settings.LogSettings, SitecoreExtensions.Helpers.GetMethodName(), ResponseBody,
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex1.Message, ex1.StackTrace, ex1);
			}
		}
	}
}