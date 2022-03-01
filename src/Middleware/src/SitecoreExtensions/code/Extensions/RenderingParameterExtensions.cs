namespace Sitecore.Foundation.SitecoreExtensions.Extensions
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Sitecore.Mvc.Presentation;
    using System.Collections.Generic;

    public static class RenderingParameterExtensions
    {
        /// <summary>
        /// Common re-usable ToJson() extension method
        /// </summary>
        /// <param name="renderingParameters"></param>
        /// <returns>The Rendering objects's JSon string value</returns>
        public static string ToJson(this RenderingParameters renderingParameters)
        {
            var keyValues = renderingParameters as IEnumerable<KeyValuePair<string, string>>;
            if (keyValues != null)
            {
                var renderingParams = new JObject();
                foreach (var keyValue in keyValues)
                {
                    renderingParams.Add(keyValue.Key, keyValue.Value);
                }

                return JsonConvert.SerializeObject(renderingParams);
            }
            return string.Empty;
        }
    }
}