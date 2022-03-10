using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Headstart.Common.Models.Base;

namespace Headstart.Common.Services.CMS.Models
{
	public enum AssetType
	{
		Image, 
		Text, 
		Audio, 
		Video, 
		Presentation, 
		SpreadSheet, 
		PDF, 
		Compressed, 
		Code, 
		JSON, 
		Markup, 
		Unknown
	}

	public class Asset : HsBaseObject
	{
		public string Title { get; set; } = string.Empty;

		public bool Active { get; set; }

		public string Url { get; set; } = string.Empty; // Generated if not set.

		public AssetType Type { get; set; }

		public List<string> Tags { get; set; } = new List<string>();

		public string FileName { get; set; } = string.Empty; // Defaults to the file name in the upload. Or should be required?

		public AssetMetadata Metadata { get; set; } = new AssetMetadata();

		public History History { get; set; } = new History();
	}

	public class AssetMetadata
	{
		public bool IsUrlOverridden { get; set; }

		public string ContentType { get; set; } = string.Empty;

		public int? SizeBytes { get; set; }

		public int? ImageHeight { get; set; } = null; // Null if asset not image

		public int? ImageWidth { get; set; } = null; // Null if asset not image

		public decimal? ImageVerticalResolution { get; set; } // Pixels per inch

		public decimal? ImageHorizontalResolution { get; set; } // Pixels per inch
	}

	public class AssetUpload : HsBaseObject
	{
		public string Title { get; set; } = string.Empty;

		public bool Active { get; set; }

		public IFormFile File { get; set; }

		public string Url { get; set; } = string.Empty;

		public string Tags { get; set; } = string.Empty;

		public string Filename { get; set; } = string.Empty;
	}
}