using System;
using System.Collections.Generic;

namespace ordercloud.integrations.library
{
    public class BlobBase64Image
    {
        public byte[] Bytes { get; }
        public string Reference { get; }
        public string ContentType { get; }
        public Dictionary<string, string> MetaData { get; set; } = new Dictionary<string, string>();

        public BlobBase64Image(string filename, string base64)
        {
            this.Bytes = Convert.FromBase64String(base64.Substring(base64.IndexOf(",", StringComparison.Ordinal) + 1));
            var tags = base64.Split(";");
            this.ContentType = tags[0].Split(":")[1];
            this.Reference = $"{filename.Replace($".{ContentType.Split("/")[1]}", "")}.{ContentType.Split("/")[1]}";
        }
    }
}
