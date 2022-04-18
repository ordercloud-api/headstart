using System;
using Flurl.Http;
using Newtonsoft.Json;
using OrderCloud.Catalyst;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Foundation.SitecoreExtensions.MVC.Extensions;
using SitecoreExtensions = Sitecore.Foundation.SitecoreExtensions.Extensions;

namespace Headstart.Common.Exceptions
{
	public class ProcessResultException
	{
		private string Message { get; set; } = string.Empty;
		private dynamic ResponseBody { get; set; }
		private readonly ConfigSettings _configSettings = ConfigSettings.Instance;

		public ProcessResultException(Exception ex)
		{
			Message = ex.Message;
			ResponseBody = string.Empty;
		}

		public ProcessResultException(CatalystBaseException ex)
		{
			Message = ex.Errors[0].Message;
			try
			{
				ResponseBody = JsonConvert.SerializeObject(ex?.Errors);
				var origEx = new Exception(ex.Message, ex);
				LoggingNotifications.LogApiResponseMessages(_configSettings.AppLogFileKey, SitecoreExtensions.Helpers.GetMethodName(), Message,
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ResponseBody, ex.StackTrace, origEx);
			}
			catch (Exception ex1)
			{
				ResponseBody = @"Error while trying to parse response body.";
				LoggingNotifications.LogApiResponseMessages(_configSettings.AppLogFileKey, SitecoreExtensions.Helpers.GetMethodName(), ResponseBody,
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex1.Message, ex1.StackTrace, ex1);
			}
		}

		public ProcessResultException(FlurlHttpException ex)
		{
			Message = ex.Message;
			try
			{
				ResponseBody = ex.GetResponseJsonAsync().Result;
				var origEx = new Exception(ex.Message, ex);
				LoggingNotifications.LogApiResponseMessages(_configSettings.AppLogFileKey, SitecoreExtensions.Helpers.GetMethodName(), Message,
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ResponseBody, ex?.StackTrace, origEx);
			}
			catch (Exception ex1)
			{
				ResponseBody = @"Error while trying to parse response body.";
				LoggingNotifications.LogApiResponseMessages(_configSettings.AppLogFileKey, SitecoreExtensions.Helpers.GetMethodName(), ResponseBody,
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex1.Message, ex1.StackTrace, ex1);
			}
		}
	}
}