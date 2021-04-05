import cookies from '../utils/CookieService'
import parseJwt from '../utils/ParseJwt'

const isNode = new Function(
  'try {return this===global;}catch(e){return false;}'
)
export default class Tokens {
  private accessTokenCookieName = `ordercloud.access-token`
  private impersonationTokenCookieName = 'ordercloud.impersonation-token'
  private refreshTokenCookieName = 'ordercloud.refresh-token'

  private accessToken?: string = null
  private impersonationToken?: string = null
  private refreshToken?: string = null

  /**
   * @ignore
   * not part of public api, don't include in generated docs
   */
  constructor() {
    this.GetAccessToken = this.GetAccessToken.bind(this)
    this.GetImpersonationToken = this.GetImpersonationToken.bind(this)
    this.GetRefreshToken = this.GetRefreshToken.bind(this)
    this.RemoveAccessToken = this.RemoveAccessToken.bind(this)
    this.RemoveImpersonationToken = this.RemoveImpersonationToken.bind(this)
    this.SetAccessToken = this.SetAccessToken.bind(this)
    this.RemoveRefreshToken = this.RemoveRefreshToken.bind(this)
    this.SetImpersonationToken = this.SetImpersonationToken.bind(this)
    this.SetRefreshToken = this.SetRefreshToken.bind(this)
  }

  /**
   * Manage Access Tokens
   */

  public GetAccessToken(): string {
    return isNode() ? this.accessToken : cookies.get(this.accessTokenCookieName)
  }

  public SetAccessToken(token: string): void {
    parseJwt(token) // check if token is valid
    isNode()
      ? (this.accessToken = token)
      : cookies.set(this.accessTokenCookieName, token)
  }

  public RemoveAccessToken(): void {
    isNode()
      ? (this.accessToken = '')
      : cookies.remove(this.accessTokenCookieName)
  }

  /**
   * Manage Impersonation Tokens
   */

  public GetImpersonationToken(): string {
    return isNode()
      ? this.impersonationToken
      : cookies.get(this.impersonationTokenCookieName)
  }

  public SetImpersonationToken(token: string): void {
    parseJwt(token) // check if token is valid
    isNode()
      ? (this.impersonationToken = token)
      : cookies.set(this.impersonationTokenCookieName, token)
  }

  public RemoveImpersonationToken(): void {
    isNode()
      ? (this.impersonationToken = null)
      : cookies.remove(this.impersonationTokenCookieName)
  }

  /**
   * Manage Refresh Tokens
   */

  public GetRefreshToken(): string {
    return isNode()
      ? this.refreshToken
      : cookies.get(this.refreshTokenCookieName)
  }

  public SetRefreshToken(token: string): void {
    isNode()
      ? (this.refreshToken = token)
      : cookies.set(this.refreshTokenCookieName, token)
  }

  public RemoveRefreshToken(): void {
    isNode()
      ? (this.refreshToken = null)
      : cookies.remove(this.refreshTokenCookieName)
  }
}
