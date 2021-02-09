import { ApiRole } from 'ordercloud-javascript-sdk'

export enum Brand {
  'DEFAULT_BUYER' = 'DEFAULT_BUYER',
}

export enum Environment {
  /**
   * for internal QA, ordercloud data is from a sandbox org
   */
  'TEST' = 'TEST',

  /**
   * An environment that is used for validation by client or demos
   * ordercloud data is restored from production weekly so mirrors production data fairly closely
   */
  'UAT' = 'UAT',
  'PRODUCTION' = 'PRODUCTION',
}

export interface Theme {
  logoSrc: string
}

export interface EnvironmentConfig {
  hostedApp: boolean

  /**
   * A short name for your app. It will be used as as general display throughout the app.
   */
  appname: string

  /**
   * A unique identifier for this app configuration
   */
  appID: string

  /**
   * The identifier for the seller, buyer network or buyer application that
   * will be used for authentication. You can view client ids for apps
   * you own or are a contributor to on the [dashboard](https://developer.ordercloud.io/dashboard)
   */
  clientID: string

  /**
   * OrderCloud allows us to increment any ID - specifically we
   * increment order IDs so that they are numbered sequentially
   * this string is prefixed to the orderID
   */
  incrementorPrefix: string
  baseUrl: string
  middlewareUrl: string
  cmsUrl: string
  creditCardIframeUrl: string
  sellerID: string
  translateBlobUrl: string
  orderCloudApiUrl: string
  theme?: Theme
  appInsightsInstrumentationKey?: string
}

export class AppConfig {
  /**
   * A short name for your app. It will be used as general display throughout the app.
   */
  appname: string

  /**
   * Unique ID for your app configuration
   */
  appID: string

  /**
   * The identifier for the seller, buyer network or buyer application that
   * will be used for authentication. You can view client ids for apps
   * you own or are a contributor to on the [dashboard](https://developer.ordercloud.io/dashboard)
   */
  clientID: string

  /**
   * OrderCloud allows us to increment any ID - specifically we
   * increment order IDs so that they are numbered sequentially
   * this string is prefixed to the orderID
   */
  incrementorPrefix: string

  /**
   * If set to true users can browse and submit orders without profiling themselves. This requires
   * additional set up in the dashboard. Click here to
   * [learn more](https://developer.ordercloud.io/documentation/platform-guides/authentication/anonymous-shopping)
   */
  anonymousShoppingEnabled: boolean
  baseUrl: string

  /**
   * link to where the translation files for the application are hosted
   */
  translateBlobUrl: string
  orderCloudApiUrl: string
  cmsUrl: string
  middlewareUrl: string
  creditCardIframeUrl: string

  /**
   *  The ID of the seller organization.
   */
  sellerID: string

  /**
   * An array of security roles that will be requested upon login.
   * These roles allow access to specific endpoints in the OrderCloud.io API.
   * To learn more about these roles and the security profiles that comprise them
   * read [here](https://developer.ordercloud.io/documentation/platform-guides/authentication/security-profiles)
   */
  scope: ApiRole[]
  theme: Theme

  /**
   * Microsoft Azure Application Insights instrumentation key
   */
  appInsightsInstrumentationKey: string
}
