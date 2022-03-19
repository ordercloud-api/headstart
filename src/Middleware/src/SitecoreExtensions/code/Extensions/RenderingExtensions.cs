using System;
using Sitecore.Mvc.Presentation;

namespace Sitecore.Foundation.SitecoreExtensions.Extensions
{
	public static class RenderingExtensions
	{
		/// <summary>
		/// Common re-usable GetIntegerParameter() extension method
		/// </summary>
		/// <param name="rendering"></param>
		/// <param name="parameterName"></param>
		/// <param name="defaultValue"></param>
		/// <returns>The Rendering object's Integer value</returns>
		public static int GetIntegerParameter(this Rendering rendering, string parameterName, int defaultValue = 0)
		{
			if (rendering == null)
			{
				throw new ArgumentNullException(nameof(rendering));
			}

			var parameter = rendering.Parameters[parameterName];
			if (string.IsNullOrEmpty(parameter))
			{
				return defaultValue;
			}
			return int.TryParse(parameter, result: out int returnValue) ? returnValue : defaultValue;
		}
	}
}