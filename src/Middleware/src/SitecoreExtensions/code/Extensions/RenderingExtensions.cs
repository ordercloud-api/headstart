namespace Sitecore.Foundation.SitecoreExtensions.Extensions
{
    using System;
    using Sitecore.Mvc.Presentation;

    public static class RenderingExtensions
    {
        /// <summary>
        /// Common re-usable GetIntegerParameter() extension method
        /// </summary>
        /// <param name="rendering"></param>
        /// <param name="parameterName"></param>
        /// <param name="defaultValue"></param>
        /// <returns>The Rendering objects's Integer value</returns>
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
            int returnValue;
            return !int.TryParse(parameter, out returnValue) ? defaultValue : returnValue;
        }
    }
}