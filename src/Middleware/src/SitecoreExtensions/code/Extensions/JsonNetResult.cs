using System.Web.Mvc;
using Newtonsoft.Json;

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

        /// <summary>
        /// Common re-usable ExecuteResult() override methof with params.
        /// </summary>
        /// <param name="context"></param>
        public override void ExecuteResult(ControllerContext context)
        {
            var response = context.HttpContext.Response;
            response.ContentType = "application/json";
            if (ContentEncoding != null)
            {
                response.ContentEncoding = ContentEncoding;
            }
            if (Data == null) return;

            var writer = new JsonTextWriter(response.Output) { Formatting = Formatting.Indented };
            var serializer = JsonSerializer.Create(new JsonSerializerSettings());
            serializer.Serialize(writer, Data);
            writer.Flush();
        }
    }
}