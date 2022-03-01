namespace Sitecore.Foundation.SitecoreExtensions.Extensions
{
    using System;
    using System.Web;
    using System.Web.Mvc;
    using Newtonsoft.Json;

    public class JsonNetResult : JsonResult
    {
        public JsonRequestBehavior Behavior { get; set; }
		
		/// <summary>
        /// JsonNetResult() contructor method for the JsonNetResult class.
        /// </summary>
        public JsonNetResult() { }

		/// <summary>
        /// JsonNetResult() contructor method with params for the JsonNetResult class.
        /// </summary>
        /// <param name="data"></param>
        public JsonNetResult(object data)
        {
            Data = data;
        }

		/// <summary>
        /// JsonNetResult() contructor method with params for the JsonNetResult class.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="behavior"></param>
        public JsonNetResult(object data, JsonRequestBehavior behavior)
        {
            Data = data;
            Behavior = behavior;
            MaxJsonLength = Int32.MaxValue;
        }

		/// <summary>
        /// Common re-usable ExecuteResult() override methof with params.
        /// </summary>
        /// <param name="context"></param>
        public override void ExecuteResult(ControllerContext context)
        {
            HttpResponseBase response = context.HttpContext.Response;
            response.ContentType = "application/json";
            if (ContentEncoding != null)
            {
                response.ContentEncoding = ContentEncoding;
            }
            if (Data == null) return;

            JsonTextWriter writer = new JsonTextWriter(response.Output) { Formatting = Formatting.Indented };
            JsonSerializer serializer = JsonSerializer.Create(new JsonSerializerSettings());
            serializer.Serialize(writer, Data);
            writer.Flush();
        }
    }
}