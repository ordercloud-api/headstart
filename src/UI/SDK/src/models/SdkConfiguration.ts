export interface SdkConfiguration {
  /**
   * the path that will be used to talk to the ordercloud api.
   * It may be useful to change this to interact with different
   * environments or different versions of the api. At the time of writing
   * there is only one version of the api.
   */
  baseApiUrl?: string

  /**
   * the path that will be used to authenticate into ordercloud api.
   * It may be useful to change this to interact with different
   * environments or different versions of the api. At the time of writing
   * there is only one version of the api.
   */
  baseAuthUrl?: string

  /**
   * when set is used to call the refresh token endpoint to obtain a new access
   * token when exired (provided a refresh token is set in the sdk)
   * this functionality is only intended for users that interact
   * with at most one client per sdk instance
   */
  clientID?: string

  /**
   * specifies the number of milliseconds before the request times out.
   * If the request takes longer than `timeoutInMilliseconds`, the request will be aborted.
   * Default timeout is 10,000 milliseconds or 10 seconds
   */
  timeoutInMilliseconds?: number

  cookieOptions?: CookieOptions
}

export interface CookieOptions {
  /**
   * The cookie will be available only for this domain and its sub-domains
   */
  domain?: string

  /**
   * If true, then the cookie will only be available through a
   * secured connection (generally https)
   */
  secure?: boolean

  /**
   * Defines protocol for how cookies should be sent
   * in first or third party contexts https://adzerk.com/blog/chrome-samesite/
   */
  samesite?: 'none' | 'lax' | 'strict'
}
