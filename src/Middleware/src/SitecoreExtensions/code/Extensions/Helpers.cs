using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Runtime.CompilerServices;

namespace Sitecore.Foundation.SitecoreExtensions.Extensions
{
	/// <summary>
	/// ApiAuthKey model class object
	/// </summary>
	public class ApiAuthKey
	{
		public string ApiAuthKeyId { get; set; } = string.Empty;
		public string ApiAuthKeyValue { get; set; } = string.Empty;
	}

	/// <summary>
	/// JsonExtension class object
	/// </summary>
	public static class JsonExtension
	{
		/// <summary>
		/// JsonRequestBehavior Enum
		/// </summary>
		public enum JsonRequestBehavior
		{
			/// <summary>
			/// HTTP GET requests from the client are allowed.
			/// </summary>
			AllowGet = 0,

			/// <summary>
			/// HTTP GET requests from the client are not allowed.
			/// </summary>
			DenyGet = 1
		}

		/// <summary>
		/// Returns model in Json String format with camelCasing
		/// </summary>
		/// <param name="value"></param>
		/// <returns>The JSon string value from the JSon object</returns>
		public static string ToJson(this object value)
		{
			var settings = new JsonSerializerSettings
			{
				ContractResolver = new CamelCasePropertyNamesContractResolver(),
				NullValueHandling = NullValueHandling.Ignore,
				ReferenceLoopHandling = ReferenceLoopHandling.Serialize
			};
			return JsonConvert.SerializeObject(value, settings);
		}

		/// <summary>
		/// Returns model in Json object format with camelCasing
		/// </summary>
		/// <param name="value"></param>
		/// <returns>The JSon object value</returns>
		public static object ToJsonObject(this object value)
		{
			return JsonConvert.DeserializeObject(value.ToJson());
		}
	}

	/// <summary>
	/// Helpers class object
	/// </summary>
	public static class Helpers
	{
		/// <summary>
		/// Common re-usable GetMethodName() method - used to return Current Method's name 
		/// </summary>
		/// <param name="callerMethod"></param>
		/// <returns>The Calling Method's name string value or empty</returns>
		public static string GetMethodName([CallerMemberName] string callerMethod = "")
		{
			return callerMethod;
		}

		/// <summary>
		/// Common re-usable SetMethodName() method - used to set Current Job Method's name
		/// </summary>
		/// <param name="callerMethod"></param>
		/// <returns>The callerMethod string value using the callerMethod param</returns>
		public static string SetMethodName(string callerMethod)
		{
			return callerMethod;
		}

		/// <summary>
		/// Common re-usable FileDateStamp() method - used to return Current File Date Stamp as string
		/// </summary>
		/// <param name="dateTime"></param>
		/// <returns>The File DateStamp string value</returns>
		public static string FileDateStamp(DateTime dateTime)
		{
			return $@"{dateTime:MM/dd/yyyy}";
		}
	}
}