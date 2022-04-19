namespace Headstart.Common.Services.CMS.Models
{
	public class AssetAssignment
	{
		public string AssetId { get; set; } = string.Empty;

		public string ResourceId { get; set; } = string.Empty;

		public ResourceType? ResourceType { get; set; }

		public string ParentResourceId { get; set; } = string.Empty;

		public ParentResourceType? ParentResourceType { get; set; }
	}
}
