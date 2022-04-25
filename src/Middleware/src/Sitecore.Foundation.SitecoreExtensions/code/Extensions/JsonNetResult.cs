using System.Web.Mvc;

namespace Sitecore.Foundation.SitecoreExtensions.Extensions
{
	public class JsonNetResult : JsonResult
	{
		/// <summary>
		/// JsonNetResult() constructor method with params for the JsonNetResult class.
		/// </summary>
		/// <param name="data"></param>
		public JsonNetResult(object data)
		{
			Data = data;
		}

		/// <summary>
		/// JsonNetResult() constructor method with params for the JsonNetResult class.
		/// </summary>
		/// <param name="data"></param>
		/// <param name="behavior"></param>
		public JsonNetResult(object data, JsonRequestBehavior behavior)
		{
			Data = data;
			MaxJsonLength = int.MaxValue;
		}
	}
}