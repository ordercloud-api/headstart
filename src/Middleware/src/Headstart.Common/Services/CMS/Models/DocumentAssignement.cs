namespace Headstart.Common.Services.CMS.Models
{
    public class DocumentAssignment
    {
        public string DocumentID { get; set; } = string.Empty;

        public string ResourceID { get; set; } = string.Empty;

        public ResourceType? ResourceType { get; set; }

        public string ParentResourceID { get; set; } = string.Empty;

        public ParentResourceType? ParentResourceType { get; set; }
    }
}