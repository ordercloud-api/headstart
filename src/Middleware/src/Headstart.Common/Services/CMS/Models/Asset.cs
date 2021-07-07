using Microsoft.AspNetCore.Http;
using ordercloud.integrations.library;
using System;
using System.Collections.Generic;
using System.Text;

namespace Headstart.Common.Services.CMS.Models
{
	public enum AssetType { Image, Text, Audio, Video, Presentation, SpreadSheet, PDF, Compressed, Code, JSON, Markup, Unknown }

	
	public class Asset
	{
		public string ID { get; set; }
		public string Title { get; set; }
		public bool Active { get; set; } = false;
		public string Url { get; set; } // Generated if not set. 
		public AssetType Type { get; set; }
		public List<string> Tags { get; set; }
		public string FileName { get; set; } // Defaults to the file name in the upload. Or should be required?
		public AssetMetadata Metadata { get; set; }
		public History History { get; set; }
	}

	
	public class AssetMetadata
	{
		public bool IsUrlOverridden { get; set; } = false;
		public string ContentType { get; set; }
		public int? SizeBytes { get; set; }
		public int? ImageHeight { get; set; } = null; // null if asset not image
		public int? ImageWidth { get; set; } = null; // null if asset not image
		public decimal? ImageVerticalResolution { get; set; } = null; // pixels per inch
		public decimal? ImageHorizontalResolution { get; set; } = null; // pixels per inch
	}

    public class AssetUpload
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public bool Active { get; set; }
        public IFormFile File { get; set; }
		//public FormFile File { get; set; }
        public string Url { get; set; }
        public string Tags { get; set; }
        public string Filename { get; set; }
    }
}
