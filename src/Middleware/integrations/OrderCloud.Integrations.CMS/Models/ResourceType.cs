using System;
using Headstart.Common.Extensions;

namespace Headstart.Integrations.CMS.Models
{
    public enum ParentResourceType
    {
        Catalogs,
        Buyers,
        Suppliers,
    }

    public enum ResourceType
    {
        Catalogs,
        [Parent(ParentResourceType.Catalogs)]
        Categories,
        Products,
        PriceSchedules,
        ProductFacets,
        Specs,

        SecurityProfiles,
        PasswordResets,
        OpenIdConnects,
        ImpersonationConfigs,

        Buyers,
        [Parent(ParentResourceType.Buyers)]
        Users,
        [Parent(ParentResourceType.Buyers)]
        UserGroups,
        [Parent(ParentResourceType.Buyers)]
        Addresses,
        [Parent(ParentResourceType.Buyers)]
        CostCenters,
        [Parent(ParentResourceType.Buyers)]
        CreditCards,
        [Parent(ParentResourceType.Buyers)]
        SpendingAccounts,
        [Parent(ParentResourceType.Buyers)]
        ApprovalRules,

        Suppliers,
        [Parent(ParentResourceType.Suppliers)]
        SupplierUsers,
        [Parent(ParentResourceType.Suppliers)]
        SupplierUserGroups,
        [Parent(ParentResourceType.Suppliers)]
        SupplierAddresses,

        // Param "Direction" breaks these for now.
        // Orders,
        // [ParentResource(Orders)] LineItems,
        // [ParentResource(Orders)] Payments,
        // [ParentResource(Orders)]Shipments,
        Promotions,

        AdminUsers,
        AdminAddresses,
        AdminUserGroups,
        MessageSenders,
        Webhooks,
        ApiClients,
        Incrementors,
        IntegrationEvents,
        XpIndices,
    }

    public static class ResourceTypeExtesions
    {
        public static ParentResourceType? GetParentType(this ResourceType type)
        {
            return typeof(ResourceType).GetField(type.ToString()).GetAttribute<ParentAttribute>()?.ParentType;
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ParentAttribute : Attribute
    {
        public ParentAttribute(ParentResourceType type)
        {
            ParentType = type;
        }

        public ParentResourceType ParentType { get; set; }
    }
}
