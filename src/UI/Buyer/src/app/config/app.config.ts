import { ApiRole } from 'ordercloud-javascript-sdk'
import { environment } from 'src/environments/environment.local'
import { AppConfig } from '../models/environment.types'

export const ocAppConfig: AppConfig = {
  appname: environment.appname,
  appID: environment.appID,
  clientID: environment.clientID,
  incrementorPrefix: environment.incrementorPrefix,
  baseUrl: environment.baseUrl,
  middlewareUrl: environment.middlewareUrl,
  cmsUrl: environment.cmsUrl,
  creditCardIframeUrl: environment.creditCardIframeUrl,
  sellerID: environment.sellerID,
  translateBlobUrl: environment.translateBlobUrl,
  orderCloudApiUrl: environment.orderCloudApiUrl,
  theme: environment.theme,
  anonymousShoppingEnabled: false,
  appInsightsInstrumentationKey: environment.appInsightsInstrumentationKey,
  scope: [
    'MeAddressAdmin',
    'AddressAdmin', // Only for location owners
    'MeAdmin',
    'MeCreditCardAdmin',
    'MeXpAdmin',
    'UserGroupAdmin',
    'ApprovalRuleAdmin',
    'Shopper',
    'BuyerUserAdmin',
    'BuyerReader',
    'PasswordReset',
    'SupplierReader',
    'SupplierAddressReader',
    'MPApprovalRuleAdmin',

    // location roles, will appear on jwt if a user
    // has this role for any location
    'MPLocationPermissionAdmin',
    'MPLocationOrderApprover',
    'MPLocationNeedsApproval',
    'MPLocationViewAllOrders',
    'MPLocationCreditCardAdmin',
    'MPLocationAddressAdmin',
    'MPLocationResaleCertAdmin',

    'DocumentReader',
  ] as ApiRole[],
}
