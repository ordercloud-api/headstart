using System;
using System.Collections.Generic;
using System.Text;

namespace ordercloud.integrations.library.Cosmos
{
	public static class CosmosInteropID
    {
        public const string VALIDATION_REGEX = @"^[a-zA-Z0-9-_{}]+$";

        public static string New()
        {
            return Encode(Guid.NewGuid());
        }

        /// <summary>
        /// returns the passed in ID or a newly generated one if null or whitespace.
        /// </summary>
        public static string GetOrNew(string id)
        {
            return string.IsNullOrEmpty(id) ? New() : id;
        }

        // http://madskristensen.net/post/a-shorter-and-url-friendly-guid
        private static string Encode(Guid guid)
        {
            string enc = Convert.ToBase64String(guid.ToByteArray());
            enc = enc.Replace("/", "_");
            enc = enc.Replace("+", "-");
            return enc.Substring(0, 22);
        }

        // in case we ever need to decode back to a guid
        // http://madskristensen.net/post/a-shorter-and-url-friendly-guid
        private static Guid Decode(string encoded)
        {
            encoded = encoded.Replace("_", "/");
            encoded = encoded.Replace("-", "+");
            byte[] buffer = Convert.FromBase64String(encoded + "==");
            return new Guid(buffer);
        }
    }
}
