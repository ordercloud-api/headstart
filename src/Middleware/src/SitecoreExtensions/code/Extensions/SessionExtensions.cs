using System.Web;
using Sitecore.Diagnostics;

namespace Sitecore.Foundation.SitecoreExtensions.Extensions
{
	public static class SessionExtensions
	{
		/// <summary>
		/// Common re-usable GetAndRemove() extension method
		/// </summary>
		/// <param name="session"></param>
		/// <param name="key"></param>
		/// <returns>The HttpSessionStateBase object after clearing it's session value</returns>
		public static object GetAndRemove(this HttpSessionStateBase session, string key)
		{
			Assert.ArgumentNotNull(session, nameof(session));
			Assert.ArgumentNotNullOrEmpty(key, nameof(key));

			var sessionItem = session[key];
			session.Remove(key);
			return sessionItem;
		}
	}
}