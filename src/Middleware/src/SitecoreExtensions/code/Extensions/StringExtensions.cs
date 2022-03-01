namespace Sitecore.Foundation.SitecoreExtensions.Extensions
{
    using System;
    using System.Web;
    using System.Text.RegularExpressions;

    public static class StringExtensions
    {
        /// <summary>
        /// Common re-usable Humanize() extension method.
        /// </summary>
        /// <param name="input"></param>
        /// <returns>The Humanize value from as string object, after replacing not allowed characters, retuned as a string value</returns>
        public static string Humanize(this string input)
        {
            return Regex.Replace(input, "(\\B[A-Z])", " $1");
        }

        /// <summary>
        /// Common re-usable ToCssUrlValue() extension method.
        /// </summary>
        /// <param name="url"></param>
        /// <returns>The CssUrlValue from as string object, as a string value</returns>
        public static string ToCssUrlValue(this string url)
        {
            return string.IsNullOrWhiteSpace(url) ? "none" : $@"url('{url}')";
        }

        /// <summary>
        /// Common re-usable StripHtml() extension method.
        /// </summary>
        /// <param name="html"></param>
        /// <returns>The content from a Html string object retrieved from the HtmlDocument object, as a string value</returns>
        public static string StripHtml(this string html)
        {
            if (string.IsNullOrEmpty(html))
            {
                return html;
            }

            HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.LoadHtml(html);
            return htmlDoc.DocumentNode.InnerText;
        }

        /// <summary>
        /// Common re-usable RenderValue() extension method - returns HtmlString or empty string.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>The clean Html content, as a Html string value</returns>
        public static IHtmlString RenderValue(this string value)
        {
            string renderValue = (!string.IsNullOrEmpty(value.GetCleanRitchTextContent())) ? value : string.Empty;
            return new HtmlString(renderValue);
        }

        /// <summary>
        /// Common re-usable RemomveSpecifiedChars() extension method - returns string or empty string.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="value"></param>
        /// <returns>The string value after removing an array of regex characters from the string, as a string value</returns>
        public static string RemoveSpecifiedChars(this string value, string regexPattern, bool setLowerCase = false)
        {
            Regex pattern = new Regex(regexPattern);
            return (setLowerCase) ? pattern.Replace(value.ToLower().Trim(), "") : pattern.Replace(value.Trim(), "");
        }
    }
}