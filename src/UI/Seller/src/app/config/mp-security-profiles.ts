/*
 * Headstart has distinct roles which are sometimes a combination of OrderCloud roles or sometimes a single OrderCloud role
 * We have choosen to represent these MP roles with security profiles and identifying custom roles for example: MPProductAdmin (OrderCloud roles: ProductAdmin, FacetAdmin, SpecAdmin)
 *
 */

import { MPRole } from '@app-seller/models/user.types'

export const MPRoles = {
  MPMeProductAdmin: 'MPMeProductAdmin',
  MPMeProductReader: 'MPMeProductReader',
  MPProductAdmin: 'MPProductAdmin',
  MPProductReader: 'MPProductReader',
  MPPromotionAdmin: 'MPPromotionAdmin',
  MPPromotionReader: 'MPPromotionReader',
  MPCategoryAdmin: 'MPCategoryAdmin',
  MPCategoryReader: 'MPCategoryReader',
  MPOrderAdmin: 'MPOrderAdmin',
  MPOrderReader: 'MPOrderReader',
  MPShipmentAdmin: 'MPShipmentAdmin',
  MPBuyerAdmin: 'MPBuyerAdmin',
  MPBuyerReader: 'MPBuyerReader',
  MPSellerAdmin: 'MPSellerAdmin',
  MPSupplierAdmin: 'MPSupplierAdmin',
  MPMeSupplierAdmin: 'MPMeSupplierAdmin',
  MPMeSupplierAddressAdmin: 'MPMeSupplierAddressAdmin',
  MPMeSupplierUserAdmin: 'MPMeSupplierUserAdmin',
  MPReportReader: 'MPReportReader',
  MPReportAdmin: 'MPReportAdmin',
  MPStorefrontAdmin: 'MPStorefrontAdmin',
}

const OrderCloudRoles = {
  AddressAdmin: 'AddressAdmin',
  AddressReader: 'AddressReader',
  AdminAddressAdmin: 'AdminAddressAdmin',
  AdminAddressReader: 'AdminAddressReader',
  AdminUserAdmin: 'AdminUserAdmin',
  AdminUserGroupAdmin: 'AdminUserGroupAdmin',
  AdminUserGroupReader: 'AdminUserGroupReader',
  AdminUserReader: 'AdminUserReader',
  ApiClientAdmin: 'ApiClientAdmin',
  ApiClientReader: 'ApiClientReader',
  ApprovalRuleAdmin: 'ApprovalRuleAdmin',
  ApprovalRuleReader: 'ApprovalRuleReader',
  BuyerAdmin: 'BuyerAdmin',
  BuyerImpersonation: 'BuyerImpersonation',
  BuyerReader: 'BuyerReader',
  BuyerUserAdmin: 'BuyerUserAdmin',
  BuyerUserReader: 'BuyerUserReader',
  CatalogAdmin: 'CatalogAdmin',
  CatalogReader: 'CatalogReader',
  CategoryAdmin: 'CategoryAdmin',
  CategoryReader: 'CategoryReader',
  CostCenterAdmin: 'CostCenterAdmin',
  CostCenterReader: 'CostCenterReader',
  CreditCardAdmin: 'CreditCardAdmin',
  CreditCardReader: 'CreditCardReader',
  FullAccess: 'FullAccess',
  GrantForAnyRole: 'GrantForAnyRole',
  IncrementorAdmin: 'IncrementorAdmin',
  IncrementorReader: 'IncrementorReader',
  IntegrationEventAdmin: 'IntegrationEventAdmin',
  IntegrationEventReader: 'IntegrationEventReader',
  InventoryAdmin: 'InventoryAdmin',
  MeAddressAdmin: 'MeAddressAdmin',
  MeAdmin: 'MeAdmin',
  MeCreditCardAdmin: 'MeCreditCardAdmin',
  MessageConfigAssignmentAdmin: 'MessageConfigAssignmentAdmin',
  MessageSenderAdmin: 'MessageSenderAdmin',
  MessageSenderReader: 'MessageSenderReader',
  MeXpAdmin: 'MeXpAdmin',
  OrderAdmin: 'OrderAdmin',
  OrderReader: 'OrderReader',
  OverrideShipping: 'OverrideShipping',
  OverrideTax: 'OverrideTax',
  OverrideUnitPrice: 'OverrideUnitPrice',
  PasswordReset: 'PasswordReset',
  PriceScheduleAdmin: 'PriceScheduleAdmin',
  PriceScheduleReader: 'PriceScheduleReader',
  ProductAdmin: 'ProductAdmin',
  ProductAssignmentAdmin: 'ProductAssignmentAdmin',
  ProductFacetAdmin: 'ProductFacetAdmin',
  ProductFacetReader: 'ProductFacetReader',
  ProductReader: 'ProductReader',
  PromotionAdmin: 'PromotionAdmin',
  PromotionReader: 'PromotionReader',
  SecurityProfileAdmin: 'SecurityProfileAdmin',
  SecurityProfileReader: 'SecurityProfileReader',
  SetSecurityProfile: 'SetSecurityProfile',
  ShipmentAdmin: 'ShipmentAdmin',
  ShipmentReader: 'ShipmentReader',
  Shopper: 'Shopper',
  SpendingAccountAdmin: 'SpendingAccountAdmin',
  SpendingAccountReader: 'SpendingAccountReader',
  SupplierAddressAdmin: 'SupplierAddressAdmin',
  SupplierAddressReader: 'SupplierAddressReader',
  SupplierAdmin: 'SupplierAdmin',
  SupplierReader: 'SupplierReader',
  SupplierUserAdmin: 'SupplierUserAdmin',
  SupplierUserGroupAdmin: 'SupplierUserGroupAdmin',
  SupplierUserGroupReader: 'SupplierUserGroupReader',
  SupplierUserReader: 'SupplierUserReader',
  UnsubmittedOrderReader: 'UnsubmittedOrderReader',
  UserGroupAdmin: 'UserGroupAdmin',
  UserGroupReader: 'UserGroupReader',
  WebhookAdmin: 'WebhookAdmin',
  WebhookReader: 'WebhookReader',
  XpIndexAdmin: 'XpIndexAdmin',
}

const MPMeProductAdmin: MPRole = {
  // Assigned to user types who want to manage own products in OC
  RoleName: MPRoles.MPMeProductAdmin,
  OrderCloudRoles: [
    OrderCloudRoles.ProductAdmin,
    OrderCloudRoles.PriceScheduleAdmin,
    OrderCloudRoles.InventoryAdmin,
  ],
}
const MPMeProductReader: MPRole = {
  // Assigned to user types who want to view own products in OC
  RoleName: MPRoles.MPMeProductReader,
  OrderCloudRoles: [
    OrderCloudRoles.ProductReader,
    OrderCloudRoles.PriceScheduleReader,
  ],
}
const MPProductAdmin: MPRole = {
  // Assigned to user types who want to manager the display to buyers of others products in OC
  RoleName: MPRoles.MPProductAdmin,
  OrderCloudRoles: [
    OrderCloudRoles.ProductReader,
    OrderCloudRoles.CatalogAdmin,
    OrderCloudRoles.ProductAssignmentAdmin,
    OrderCloudRoles.ProductFacetAdmin,
  ],
}
const MPProductReader: MPRole = {
  // Assigned to user types who want to view the display to buyers of others products in OC but cannot manager (might not be needed for SEB)
  RoleName: MPRoles.MPProductReader,
  OrderCloudRoles: [
    OrderCloudRoles.ProductReader,
    OrderCloudRoles.CatalogReader,
    OrderCloudRoles.ProductFacetReader,
  ],
}
const MPPromotionAdmin: MPRole = {
  // Assigned to user types who want to administer promotions
  RoleName: MPRoles.MPPromotionAdmin,
  OrderCloudRoles: [OrderCloudRoles.PromotionAdmin],
}
const MPPromotionReader: MPRole = {
  // Assigned to user types who want to view promotions
  RoleName: MPRoles.MPPromotionReader,
  OrderCloudRoles: [OrderCloudRoles.PromotionReader],
}
const MPCategoryAdmin: MPRole = {
  // Assigned to user types who want to administer categorys and assignments
  RoleName: MPRoles.MPCategoryAdmin,
  OrderCloudRoles: [OrderCloudRoles.CategoryAdmin],
}
const MPCategoryReader: MPRole = {
  // Assigned to user types who want to view categorys
  RoleName: MPRoles.MPCategoryReader,
  OrderCloudRoles: [OrderCloudRoles.CategoryReader],
}
const MPOrderAdmin: MPRole = {
  // Assigned to user types who want to edit orders, line items, and shipments. Would likely by a supplier who needs to make manual updates to an order
  RoleName: MPRoles.MPOrderAdmin,
  OrderCloudRoles: [OrderCloudRoles.OrderAdmin, OrderCloudRoles.ShipmentReader, OrderCloudRoles.AddressReader],
}
const MPOrderReader: MPRole = {
  // Assigned to a user type who wants to view orders. Would likely be a seller user who shouldn't edit orders but wants to view
  RoleName: MPRoles.MPOrderReader,
  OrderCloudRoles: [
    OrderCloudRoles.OrderReader,
    OrderCloudRoles.ShipmentReader,
    OrderCloudRoles.AddressReader
  ],
}
const MPShipmentAdmin: MPRole = {
  // Assigned to a user type who wants to administer shipping for a supplier
  RoleName: MPRoles.MPShipmentAdmin,
  OrderCloudRoles: [OrderCloudRoles.OrderReader, OrderCloudRoles.ShipmentAdmin, OrderCloudRoles.AddressReader],
}
// unclear if we need a MeBuyerAdmin
// will need to be some disucssion about the breakout of these for SEB
const MPBuyerAdmin: MPRole = {
  // Assigned to a user type who wants to administer buyers and related subresources
  RoleName: MPRoles.MPBuyerAdmin,
  OrderCloudRoles: [
    OrderCloudRoles.BuyerAdmin,
    OrderCloudRoles.BuyerUserAdmin,
    OrderCloudRoles.UserGroupAdmin,
    OrderCloudRoles.AddressAdmin,
    OrderCloudRoles.CreditCardAdmin,
    OrderCloudRoles.ApprovalRuleAdmin,
  ],
}
const MPBuyerReader: MPRole = {
  // Assigned to a user type who wants to view buyers and related subresources
  RoleName: MPRoles.MPBuyerReader,
  OrderCloudRoles: [
    OrderCloudRoles.BuyerReader,
    OrderCloudRoles.BuyerUserReader,
    OrderCloudRoles.UserGroupReader,
    OrderCloudRoles.AddressReader,
    OrderCloudRoles.CreditCardReader,
    OrderCloudRoles.ApprovalRuleReader,
  ],
}
const MPSellerAdmin: MPRole = {
  // Assigned to a user type who wants to view buyers and related subresources
  RoleName: MPRoles.MPSellerAdmin,
  OrderCloudRoles: [OrderCloudRoles.AdminUserAdmin],
}
const MPSupplierAdmin: MPRole = {
  // Assigned to a user type who wants to view buyers and related subresources
  RoleName: MPRoles.MPSupplierAdmin,
  OrderCloudRoles: [
    OrderCloudRoles.SupplierAdmin,
    OrderCloudRoles.SupplierUserAdmin,
    OrderCloudRoles.SupplierAddressAdmin,
  ],
}
const MPMeSupplierAdmin: MPRole = {
  RoleName: MPRoles.MPMeSupplierAdmin,
  OrderCloudRoles: [
    OrderCloudRoles.SupplierAdmin,
    OrderCloudRoles.SupplierAddressReader,
    OrderCloudRoles.SupplierUserReader,
  ],
}
const MPMeSupplierAddressAdmin: MPRole = {
  RoleName: MPRoles.MPMeSupplierAddressAdmin,
  OrderCloudRoles: [
    OrderCloudRoles.SupplierReader,
    OrderCloudRoles.SupplierAddressAdmin,
  ],
}
const MPMeSupplierUserAdmin: MPRole = {
  RoleName: MPRoles.MPMeSupplierUserAdmin,
  OrderCloudRoles: [
    OrderCloudRoles.SupplierReader,
    OrderCloudRoles.SupplierUserAdmin,
  ],
}
const MPReportReader: MPRole = {
  RoleName: MPRoles.MPReportReader,
  OrderCloudRoles: [
    OrderCloudRoles.SupplierReader,
    OrderCloudRoles.SupplierAdmin,
  ],
}
const MPReportAdmin: MPRole = {
  RoleName: MPRoles.MPReportAdmin,
  OrderCloudRoles: [OrderCloudRoles.AdminUserAdmin],
}
const MPStorefrontAdmin: MPRole = {
  RoleName: MPRoles.MPStorefrontAdmin,
  OrderCloudRoles: [
    OrderCloudRoles.ProductFacetAdmin,
    OrderCloudRoles.ProductFacetReader,
  ],
}

const HSManager = {
  Name: 'HSManager',
  MPRoles: [
    MPProductReader,
    MPPromotionReader,
    MPCategoryReader,
    MPOrderReader,
    MPBuyerReader,
    MPSellerAdmin,
    MPSupplierAdmin,
    MPMeSupplierAdmin,
    MPStorefrontAdmin,
    MPReportReader,
    MPReportAdmin,
  ],
}

// SEB Specific Roles
// Ultimately these will not be hardcoded in the app but live outside and by dynamic

const SupplierManager = {
  Name: 'SupplierManager',
  MPRoles: [
    MPMeProductAdmin,
    MPCategoryReader,
    MPOrderAdmin,
    MPShipmentAdmin,
    MPMeSupplierAdmin,
    MPMeSupplierAddressAdmin,
    MPMeSupplierUserAdmin,
    MPReportReader,
  ],
}
const SupplierTeamMember = {
  Name: 'SupplierTeamMember',
  MPRoles: [
    MPMeProductAdmin,
    MPOrderAdmin,
    MPShipmentAdmin,
    MPMeSupplierAdmin,
    MPMeSupplierAddressAdmin,
    MPReportReader,
  ],
}
const SEBUserTypes = [SupplierManager, SupplierTeamMember, HSManager]
