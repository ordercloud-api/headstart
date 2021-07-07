import { InjectionToken } from '@angular/core'
import { environment } from '../../environments/environment.local'
import { ApiRole } from '@ordercloud/angular-sdk'
import { AppConfig } from '@app-seller/models/environment.types'

export const ocAppConfig: AppConfig = {
  appname: environment.appname,
  clientID: environment.clientID,
  sellerID: environment.sellerID,
  middlewareUrl: environment.middlewareUrl,
  orderCloudApiUrl: environment.orderCloudApiUrl,
  translateBlobUrl: environment.translateBlobUrl,
  blobStorageUrl: environment.blobStorageUrl,
  // sellerName is being hard-coded until this is available to store in OrderCloud
  sellerName: environment.sellerName,
  scope: [
    // 'AdminAddressReader' is just for reading admin addresses as a seller user on product create/edti
    // Will need to be updated to 'AdminAddressAdmin' when seller address create is implemented
    'ApiClientAdmin',
    'ApiClientReader',
    'AdminAddressAdmin',
    'AdminAddressReader',
    'MeAddressAdmin',
    'AddressReader',
    'MeAdmin',
    'BuyerUserAdmin',
    'UserGroupAdmin',
    'MeCreditCardAdmin',
    'MeXpAdmin',
    'Shopper',
    'CategoryReader',
    'ProductAdmin',

    // adding this for product editing and creation on the front end
    // this logic may be moved to the backend in the future and this might not be required
    'PriceScheduleAdmin',

    'SupplierReader',
    'SupplierAddressReader',
    'BuyerAdmin',
    'OverrideUnitPrice',
    'OrderAdmin',
    'OverrideTax',
    'OverrideShipping',
    'BuyerImpersonation',
    'AddressAdmin',
    'CategoryAdmin',
    'CatalogAdmin',
    'PromotionAdmin',
    'ApprovalRuleAdmin',
    'CreditCardAdmin',
    'SupplierAdmin',
    'SupplierUserAdmin',
    'SupplierUserGroupAdmin',
    'SupplierAddressAdmin',
    'AdminUserAdmin',
    'ProductFacetAdmin',
    'ProductFacetReader',
    'ShipmentAdmin',

    // custom cms roles
    'AssetAdmin',
    'DocumentAdmin',
    'SchemaAdmin',

    // custom roles used to conditionally display ui
    'HSMeProductAdmin',
    'HSMeProductReader',
    'HSProductAdmin',
    'HSProductReader',
    'HSPromotionAdmin',
    'HSPromotionReader',
    'HSCategoryAdmin',
    'HSCategoryReader',
    'HSOrderAdmin',
    'HSOrderReader',
    'HSShipmentAdmin',
    'HSBuyerAdmin',
    'HSBuyerReader',
    'HSSellerAdmin',
    'HSReportReader',
    'HSReportAdmin',
    'HSSupplierAdmin',
    'HSMeSupplierAdmin',
    'HSMeSupplierAddressAdmin',
    'HSMeSupplierUserAdmin',
    'HSSupplierUserGroupAdmin',
    'HSStorefrontAdmin',
  ] as ApiRole[],
  impersonatingBuyerScope: [
    'MeAddressAdmin',
    'AddressAdmin', // Only for location owners
    'MeAdmin',
    'MeCreditCardAdmin',
    'MeXpAdmin',
    'UserGroupAdmin',
    'ApprovalRuleAdmin',
    'BuyerUserAdmin',
    'Shopper',
    'BuyerReader',
    'PasswordReset',
    'SupplierReader',
    'SupplierAddressReader',
  ] as ApiRole[],
  impersonatingBuyerCustomRoleScope: [
    'HSLocationOrderApprover',
    'HSLocationViewAllOrders',
  ],
}

export const applicationConfiguration = new InjectionToken<AppConfig>(
  'app.config',
  {
    providedIn: 'root',
    factory: () => ocAppConfig,
  }
)
