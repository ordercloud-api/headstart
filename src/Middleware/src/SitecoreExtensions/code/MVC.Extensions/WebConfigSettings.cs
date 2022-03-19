using System;
using Sitecore.Diagnostics;
using System.Web.Configuration;
using Sitecore.Foundation.SitecoreExtensions.Extensions;

namespace Sitecore.Foundation.SitecoreExtensions.MVC.Extensions
{
	public sealed class WebConfigSettings
	{
		private static WebConfigSettings _instance = null;
		private static readonly object _padlock = new object();
		public bool IsNonProdEnv { get; set; }
		public string AppLogFileKey { get; set; } = "CustomApiLogs";

		/// <summary>
		/// Default WebConfigSettings object constructor method
		/// </summary>
		private WebConfigSettings()
		{
			//Do nothing as this is a Singleton Class which means class can only be instantiated with-in the class itself and create only one instance of itself,
			//allowing to have thread safe object instance with thread-safe locking on use of the object (i.e. using double-check locking) 
			//See: http://csharpindepth.com/articles/general/singleton.aspx for details
		}


		/// <summary>
		/// Singleton Class which means class can only be instantiated with-in the class itself and create only one instance of itself, 
		/// allowing to have thread safe object instance with thread-safe locking on use of the object (i.e. using double-check locking)
		/// See: http://csharpindepth.com/articles/general/singleton.aspx for details
		/// </summary>
		public static WebConfigSettings Instance
		{
			get
			{
				if (_instance != null)
				{
					_instance.GetConfigSettings();
					return _instance;
				}
				lock (_padlock)
				{
					if (_instance == null)
					{
						_instance = new WebConfigSettings();
					}
					_instance.GetConfigSettings();
				}
				return _instance;
			}
		}

		/// <summary>
		/// GetConfigSettings to get the latest WebConfiguration AppSettings into the WebConfigSettings intanace for re-usability throughout the entire application provides
		/// property intellisense, reducing typos and better more centralized management of AppSettings keys.
		/// </summary>
		private void GetConfigSettings()
		{
			try
			{
				IsNonProdEnv = DataTypeExtensions.GetBoolean(WebConfigurationManager.AppSettings[@"IsNonProdEnv"].ToString().Trim());
				AppLogFileKey = string.IsNullOrEmpty(WebConfigurationManager.AppSettings[@"AppLogFileKey"].ToString().Trim())
					? AppLogFileKey : WebConfigurationManager.AppSettings[@"AppLogFileKey"].ToString().Trim();
			}
			catch (Exception ex)
			{
				LogExt.LogException(AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}
	}
}