/*
 * Headstart has distinct roles which are sometimes a combination of OrderCloud roles or sometimes a single OrderCloud role
 * We have choosen to represent these HS roles with security profiles and identifying custom roles for example: HSProductAdmin (OrderCloud roles: ProductAdmin, FacetAdmin, SpecAdmin)
 *
 */

import { HSRole } from '@app-seller/models/user.types'

export const HSRoles = {
  HSMeProductAdmin: 'HSMeProductAdmin',
  HSMeProductReader: 'HSMeProductReader',
  HSProductAdmin: 'HSProductAdmin',
  HSProductReader: 'HSProductReader',
  HSPromotionAdmin: 'HSPromotionAdmin',
  HSPromotionReader: 'HSPromotionReader',
  HSCategoryAdmin: 'HSCategoryAdmin',
  HSCategoryReader: 'HSCategoryReader',
  HSOrderAdmin: 'HSOrderAdmin',
  HSOrderReader: 'HSOrderReader',
  HSShipmentAdmin: 'HSShipmentAdmin',
  HSBuyerAdmin: 'HSBuyerAdmin',
  HSBuyerReader: 'HSBuyerReader',
  HSSellerAdmin: 'HSSellerAdmin',
  HSSupplierAdmin: 'HSSupplierAdmin',
  HSMeSupplierAdmin: 'HSMeSupplierAdmin',
  HSMeSupplierAddressAdmin: 'HSMeSupplierAddressAdmin',
  HSMeSupplierUserAdmin: 'HSMeSupplierUserAdmin',
  HSReportReader: 'HSReportReader',
  HSReportAdmin: 'HSReportAdmin',
  HSStorefrontAdmin: 'HSStorefrontAdmin',
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

const HSMeProductAdmin: HSRole = {
  // Assigned to user types who want to manage own products in OC
  RoleName: HSRoles.HSMeProductAdmin,
  OrderCloudRoles: [
    OrderCloudRoles.ProductAdmin,
    OrderCloudRoles.PriceScheduleAdmin,
    OrderCloudRoles.InventoryAdmin,
  ],
}
const HSMeProductReader: HSRole = {
  // Assigned to user types who want to view own products in OC
  RoleName: HSRoles.HSMeProductReader,
  OrderCloudRoles: [
    OrderCloudRoles.ProductReader,
    OrderCloudRoles.PriceScheduleReader,
  ],
}
const HSProductAdmin: HSRole = {
  // Assigned to user types who want to manager the display to buyers of others products in OC
  RoleName: HSRoles.HSProductAdmin,
  OrderCloudRoles: [
    OrderCloudRoles.ProductReader,
    OrderCloudRoles.CatalogAdmin,
    OrderCloudRoles.ProductAssignmentAdmin,
    OrderCloudRoles.ProductFacetAdmin,
  ],
}
const HSProductReader: HSRole = {
  // Assigned to user types who want to view the display to buyers of others products in OC but cannot manager (might not be needed for SEB)
  RoleName: HSRoles.HSProductReader,
  OrderCloudRoles: [
    OrderCloudRoles.ProductReader,
    OrderCloudRoles.CatalogReader,
    OrderCloudRoles.ProductFacetReader,
  ],
}
const HSPromotionAdmin: HSRole = {
  // Assigned to user types who want to administer promotions
  RoleName: HSRoles.HSPromotionAdmin,
  OrderCloudRoles: [OrderCloudRoles.PromotionAdmin],
}
const HSPromotionReader: HSRole = {
  // Assigned to user types who want to view promotions
  RoleName: HSRoles.HSPromotionReader,
  OrderCloudRoles: [OrderCloudRoles.PromotionReader],
}
const HSCategoryAdmin: HSRole = {
  // Assigned to user types who want to administer categorys and assignments
  RoleName: HSRoles.HSCategoryAdmin,
  OrderCloudRoles: [OrderCloudRoles.CategoryAdmin],
}
const HSCategoryReader: HSRole = {
  // Assigned to user types who want to view categorys
  RoleName: HSRoles.HSCategoryReader,
  OrderCloudRoles: [OrderCloudRoles.CategoryReader],
}
const HSOrderAdmin: HSRole = {
  // Assigned to user types who want to edit orders, line items, and shipments. Would likely by a supplier who needs to make manual updates to an order
  RoleName: HSRoles.HSOrderAdmin,
  OrderCloudRoles: [
    OrderCloudRoles.OrderAdmin,
    OrderCloudRoles.ShipmentReader,
    OrderCloudRoles.AddressReader,
  ],
}
const HSOrderReader: HSRole = {
  // Assigned to a user type who wants to view orders. Would likely be a seller user who shouldn't edit orders but wants to view
  RoleName: HSRoles.HSOrderReader,
  OrderCloudRoles: [
    OrderCloudRoles.OrderReader,
    OrderCloudRoles.ShipmentReader,
    OrderCloudRoles.AddressReader,
  ],
}
const HSShipmentAdmin: HSRole = {
  // Assigned to a user type who wants to administer shipping for a supplier
  RoleName: HSRoles.HSShipmentAdmin,
  OrderCloudRoles: [
    OrderCloudRoles.OrderReader,
    OrderCloudRoles.ShipmentAdmin,
    OrderCloudRoles.AddressReader,
  ],
}
// unclear if we need a MeBuyerAdmin
// will need to be some disucssion about the breakout of these for SEB
const HSBuyerAdmin: HSRole = {
  // Assigned to a user type who wants to administer buyers and related subresources
  RoleName: HSRoles.HSBuyerAdmin,
  OrderCloudRoles: [
    OrderCloudRoles.BuyerAdmin,
    OrderCloudRoles.BuyerUserAdmin,
    OrderCloudRoles.UserGroupAdmin,
    OrderCloudRoles.AddressAdmin,
    OrderCloudRoles.CreditCardAdmin,
    OrderCloudRoles.ApprovalRuleAdmin,
  ],
}
const HSBuyerReader: HSRole = {
  // Assigned to a user type who wants to view buyers and related subresources
  RoleName: HSRoles.HSBuyerReader,
  OrderCloudRoles: [
    OrderCloudRoles.BuyerReader,
    OrderCloudRoles.BuyerUserReader,
    OrderCloudRoles.UserGroupReader,
    OrderCloudRoles.AddressReader,
    OrderCloudRoles.CreditCardReader,
    OrderCloudRoles.ApprovalRuleReader,
  ],
}
const HSSellerAdmin: HSRole = {
  // Assigned to a user type who wants to view buyers and related subresources
  RoleName: HSRoles.HSSellerAdmin,
  OrderCloudRoles: [OrderCloudRoles.AdminUserAdmin],
}
const HSSupplierAdmin: HSRole = {
  // Assigned to a user type who wants to view buyers and related subresources
  RoleName: HSRoles.HSSupplierAdmin,
  OrderCloudRoles: [
    OrderCloudRoles.SupplierAdmin,
    OrderCloudRoles.SupplierUserAdmin,
    OrderCloudRoles.SupplierAddressAdmin,
  ],
}
const HSMeSupplierAdmin: HSRole = {
  RoleName: HSRoles.HSMeSupplierAdmin,
  OrderCloudRoles: [
    OrderCloudRoles.SupplierAdmin,
    OrderCloudRoles.SupplierAddressReader,
    OrderCloudRoles.SupplierUserReader,
  ],
}
const HSMeSupplierAddressAdmin: HSRole = {
  RoleName: HSRoles.HSMeSupplierAddressAdmin,
  OrderCloudRoles: [
    OrderCloudRoles.SupplierReader,
    OrderCloudRoles.SupplierAddressAdmin,
  ],
}
const HSMeSupplierUserAdmin: HSRole = {
  RoleName: HSRoles.HSMeSupplierUserAdmin,
  OrderCloudRoles: [
    OrderCloudRoles.SupplierReader,
    OrderCloudRoles.SupplierUserAdmin,
  ],
}
const HSReportReader: HSRole = {
  RoleName: HSRoles.HSReportReader,
  OrderCloudRoles: [
    OrderCloudRoles.SupplierReader,
    OrderCloudRoles.SupplierAdmin,
  ],
}
const HSReportAdmin: HSRole = {
  RoleName: HSRoles.HSReportAdmin,
  OrderCloudRoles: [OrderCloudRoles.AdminUserAdmin],
}
const HSStorefrontAdmin: HSRole = {
  RoleName: HSRoles.HSStorefrontAdmin,
  OrderCloudRoles: [
    OrderCloudRoles.ProductFacetAdmin,
    OrderCloudRoles.ProductFacetReader,
  ],
}

const HSManager = {
  Name: 'HSManager',
  HSRoles: [
    HSProductReader,
    HSPromotionReader,
    HSCategoryReader,
    HSOrderReader,
    HSBuyerReader,
    HSSellerAdmin,
    HSSupplierAdmin,
    HSMeSupplierAdmin,
    HSStorefrontAdmin,
    HSReportReader,
    HSReportAdmin,
  ],
}

// SEB Specific Roles
// Ultimately these will not be hardcoded in the app but live outside and by dynamic

const SupplierManager = {
  Name: 'SupplierManager',
  HSRoles: [
    HSMeProductAdmin,
    HSCategoryReader,
    HSOrderAdmin,
    HSShipmentAdmin,
    HSMeSupplierAdmin,
    HSMeSupplierAddressAdmin,
    HSMeSupplierUserAdmin,
    HSReportReader,
  ],
}
const SupplierTeamMember = {
  Name: 'SupplierTeamMember',
  HSRoles: [
    HSMeProductAdmin,
    HSOrderAdmin,
    HSShipmentAdmin,
    HSMeSupplierAdmin,
    HSMeSupplierAddressAdmin,
    HSReportReader,
  ],
}
const SEBUserTypes = [SupplierManager, SupplierTeamMember, HSManager]
