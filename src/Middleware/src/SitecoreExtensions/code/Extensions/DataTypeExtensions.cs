using System.Text.RegularExpressions;

namespace System
{
	public static class DataTypeExtensions
	{
		/// <summary>
		/// Common re-usable GetCleanRitchTextContent() method to return guid string value or empty guid value as a GUID object type, from String object.
		/// </summary>
		/// <param name="value"></param>
		/// <returns>The clean text content from a Ritch-Text Html content, as a string with all Html stripped out</returns>
		public static string GetCleanRitchTextContent(this string value)
		{
			return Regex.Replace(value.Trim(), @"\r\n?|\n", string.Empty).Replace("&nbsp;", string.Empty).Replace(@"<br/>", string.Empty).Replace(@"<br />", string.Empty).Trim();
		}

		/// <summary>
		/// Common re-usable GetStringGuid() method to return guid string value or empty guid value as a GUID object type, from String object.
		/// </summary>
		/// <param name="value"></param>
		/// <returns>The Guid value or Guid.Empty value from a string object, as a Guid object value</returns>
		public static Guid GetStringGuid(this string value)
		{
			var guidId = Guid.Empty;
			Guid.TryParse(value, out guidId);
			return guidId;
		}

		/// <summary>
		/// Common re-usable GetGuidString() method to return guid string value or empty guid value as a String object type, from String object
		/// </summary>
		/// <param name="value"></param>
		/// <returns>The Guid value or Guid.Empty value from a string object, as a string value</returns>
		public static string GetGuidString(this string value)
		{
			var guidId = Guid.Empty;
			Guid.TryParse(value, out guidId);
			return guidId.ToString().Trim();
		}

		/// <summary>
		/// Common re-usable GetStringToInt() method to return string value or default (0) value as a Int object type, from String object.
		/// </summary>
		/// <param name="value"></param>
		/// <returns>The Integer value from a string object (defaults to empty int value), as an int value</returns>
		public static int GetStringToInt(this string value)
		{
			int.TryParse(value, out var intId);
			return intId;
		}

		/// <summary>
		/// Common re-usable GetOperationalStartDatetime() method to return DateTime Start value or default (DateTime.Now) value, from string 
		/// object as a DateTime object type (i.e. for auto-sync or batch jobs).
		/// </summary>
		/// <param name="value"></param>
		/// <returns>The DateTime value from a string object, as a DateTime value (default value returns as DateTime.Now value)</returns>
		public static DateTime GetOperationalStartDatetime(this string value)
		{
			var now = DateTime.Now;
			DateTime dateTime = new DateTime(now.Year, now.Month, now.Day, 8, 0, 0);
			DateTime.TryParse(value, out dateTime);
			return dateTime;
		}

		/// <summary>
		/// Common re-usable GetOperationalEndDatetime() method to return DateTime End value or default (DateTime.Now) value, from string 
		/// object as a DateTime object type (i.e. for auto-sync or batch jobs).
		/// </summary>
		/// <param name="value"></param>
		/// <returns>The DateTime value value from a string object, as a DateTime value (default value returns as DateTime.Now value)</returns>
		public static DateTime GetOperationalEndDatetime(this string value)
		{
			DateTime now = DateTime.Now;
			DateTime dateTime = new DateTime(now.Year, now.Month, now.Day, 20, 0, 0);
			DateTime.TryParse(value, out dateTime);
			return dateTime;
		}

		/// <summary>
		/// Common re-usable GetDateTime() method to return DateTime value or default (DateTime.Now) value from string value as a DateTime object type.
		/// </summary>
		/// <param name="value"></param>
		/// <returns>The DateTime value from a string param value, as a DateTime value (default value returns as DateTime.Now value)</returns>
		public static DateTime GetDateTime(string value)
		{
			value = (string.IsNullOrEmpty(value)) ? string.Empty : value;
			var dtVal = DateTime.Now;
			DateTime.TryParse(value, out dtVal);
			return dtVal;
		}

		/// <summary>
		/// Common re-usable GetInt() method to return int value or default (0) value as a Int object type, from passed in string value
		/// </summary>
		/// <param name="value"></param>
		/// <returns>The Integer value from a string param value, as an int value (default value returns as 0)</returns>
		public static int GetInt(string value)
		{
			value = (string.IsNullOrEmpty(value)) ? string.Empty : value;
			int.TryParse(value, out var intVal);
			return intVal;
		}

		/// <summary>
		/// Common re-usable GetDouble() method to return double value or default (0.00) value as a Double object type, from passed in string value.
		/// </summary>
		/// <param name="value"></param>
		/// <returns>The Double value from a string param value, as a double value (default value returns as 0.00)</returns>
		public static double GetDouble(string value)
		{
			value = (string.IsNullOrEmpty(value)) ? string.Empty : value;
			double.TryParse(value, out var dblVal);
			return dblVal;
		}

		/// <summary>
		/// Common re-usable GetBoolean() method to return boolean value or default (false) value as a Boolean object type, from passed in string value.
		/// </summary>
		/// <param name="value"></param>
		/// <returns>The Boolean value from a string param value, as an int value (default value returns as false)</returns>
		public static bool GetBoolean(string value)
		{
			value = (string.IsNullOrEmpty(value)) ? string.Empty : value;
			bool.TryParse(value, out var blVal);
			return blVal;
		}

		/// <summary>
		/// IsInRange used by the common 'ExternalMessageHandler.ValidateMessage()' method.
		/// </summary>
		/// <param name="dateTime"></param>
		/// <param name="baseDate"></param>
		/// <param name="rangeSeconds"></param>
		/// <returns>The Boolean value if the DateTime object is between startDate and endDate variables</returns>
		public static bool IsInRange(this DateTime dateTime, DateTime baseDate, int rangeSeconds)
		{
			dateTime = dateTime.ToUniversalTime();
			baseDate = baseDate.ToUniversalTime();

			var startDate = baseDate.AddSeconds(-rangeSeconds);
			var endDate = baseDate.AddSeconds(rangeSeconds);
			return ((startDate <= dateTime) && (dateTime <= endDate));
		}

		/// <summary>
		/// IsInRange used by the common 'ExternalMessageHandler.ValidateMessage()' method.
		/// </summary>
		/// <param name="dateTime"></param>
		/// <param name="startDate"></param>
		/// <param name="endDate"></param>
		/// <returns></returns>
		public static bool IsInRange(this DateTime dateTime, DateTime startDate, DateTime endDate)
		{
			dateTime = dateTime.ToUniversalTime();
			startDate = startDate.ToUniversalTime();
			endDate = endDate.ToUniversalTime();
			if (endDate >= startDate) // swap start/end if they're out of order
			{
				return ((startDate <= dateTime) && (dateTime <= endDate));
			}

			var tempStart = startDate;
			startDate = endDate;
			endDate = tempStart;
			return ((startDate <= dateTime) && (dateTime <= endDate));
		}
	}

	/// <summary>
	/// Common re-usable ExcludeFromHashAttribute Attribute - used to apply a ExcludeFromHashAttribute attribute to model properties.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class ExcludeFromHashAttribute : Attribute
	{
	}
}