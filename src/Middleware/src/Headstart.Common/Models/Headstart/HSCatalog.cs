using OrderCloud.SDK;

namespace Headstart.Models
{
    public class HSCatalog : UserGroup<UserGroupCatalogXp>, IHSObject
    {
    }

    public class UserGroupCatalogXp
    {
        /// <summary>
        /// Provides context type to the user group to drive functionality. User groups created with this xp will always be of type "Catalog".
        /// </summary>
        public string Type { get; set; } = "Catalog";
    }
}
