import cookies from '../utils/CookieService'
import parseJwt from '../utils/ParseJwt'
import Auth from './Auth'
import Configuration from '../configuration'

/**
 * @ignore
 * not part of public api, don't include in generated docs
 */
const isNode = new Function(
  'try {return this===global;}catch(e){return false;}'
)
class Tokens {
  private accessTokenCookieName = `.access-token`
  private impersonationTokenCookieName = '.impersonation-token'
  private refreshTokenCookieName = '.refresh-token'

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
    this._isTokenExpired = this._isTokenExpired.bind(this)
    this._tryRefreshToken = this._tryRefreshToken.bind(this)
  }

  /**
   * Manage Access Tokens
   */

  public GetAccessToken(): string | undefined {
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

  public GetImpersonationToken(): string | undefined {
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

  public GetRefreshToken(): string | undefined {
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

  /**
   * If no token is provided will attempt to get and validate token
   * stored in sdk. If token is invalid or missing it will also attempt
   * to refresh the token if possible
   */
  public async GetValidToken(tokenToValidate?: string): Promise<string> {
    let token = tokenToValidate || this.GetAccessToken()
    if (this._isTokenExpired(token)) {
      token = await this._tryRefreshToken(token)
    }
    return Promise.resolve(token || '')
  }

  private _isTokenExpired(token: string): boolean {
    if (!token) {
      return true
    }
    const decodedToken = parseJwt(token)
    const currentSeconds = Date.now() / 1000
    const currentSecondsWithBuffer = currentSeconds - 10
    return decodedToken.exp < currentSecondsWithBuffer
  }

  private async _tryRefreshToken(accessToken: string): Promise<string> {
    const refreshToken = this.GetRefreshToken()
    if (!refreshToken) {
      return accessToken || ''
    }
    const sdkConfig = Configuration.Get()
    if (!accessToken && !sdkConfig.clientID) {
      return accessToken || ''
    }

    // try to get clientid so we can make refresh request
    let clientID
    if (accessToken) {
      const decodedToken = parseJwt(accessToken)
      clientID = decodedToken.cid
    }
    if (sdkConfig.clientID) {
      clientID = sdkConfig.clientID
    }

    if (!clientID) {
      return ''
    }

    if (clientID) {
      try {
        const refreshRequest = await Auth.RefreshToken(refreshToken, clientID)
        const accessToken = refreshRequest.access_token
        this.SetAccessToken(accessToken)
        return accessToken
      } catch (e) {
        return ''
      }
    }
  }
}
export default new Tokens()
