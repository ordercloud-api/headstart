import { DOCUMENT } from '@angular/common'
import { Inject, Injectable } from '@angular/core'
import { CanActivate, CanActivateChild, Router } from '@angular/router'
import { Tokens } from 'ordercloud-javascript-sdk'
import { AppConfig } from 'src/app/models/environment.types'
import { ApplicationInsightsService } from 'src/app/services/application-insights/application-insights.service'
import { AuthService } from 'src/app/services/auth/auth.service'
import { TokenHelperService } from 'src/app/services/token-helper/token-helper.service'

@Injectable({
  providedIn: 'root',
})
export class HasTokenGuard implements CanActivate, CanActivateChild {
  constructor(
    private appInsightsService: ApplicationInsightsService,
    private router: Router,
    private auth: AuthService,
    private tokenHelper: TokenHelperService,
    @Inject(DOCUMENT) private document: any,
    private appConfig: AppConfig
  ) {}

  /**
   * very simple test to make sure a token exists,
   * can be parsed and has a valid expiration time.
   *
   * Shouldn't be depended on for actual token validation.
   * The API will block invalid tokens
   * and the client-side refresh-token interceptor will handle it correctly
   */
  async canActivate(): Promise<boolean> {
    // check for impersonation superseeds existing tokens to allow impersonating buyers sequentially.
    if (this.isImpersonating()) {
      const token = this.getQueryParamToken()
      this.auth.loginWithTokens(token)
      return true
    } else if (this.isSingleSignOn()) {
      const token = this.getQueryParamSSOToken()
      this.auth.loginWithTokens(token, null, true)
      return true
    }

    const isAccessTokenValid = this.isTokenValid()
    const refreshTokenExists =
      Tokens.GetRefreshToken() && this.auth.getRememberStatus()
    if (!isAccessTokenValid && refreshTokenExists) {
      await this.auth.refresh().toPromise()
      return true
    }

    // send profiled users to login to get new token
    if (!isAccessTokenValid && !this.appConfig.anonymousShoppingEnabled) {
      this.router.navigate(['/login'])
      return false
    }
    // get new anonymous token and then let them continue
    if (!isAccessTokenValid && this.appConfig.anonymousShoppingEnabled) {
      await this.auth.anonymousLogin()
      return true
    }
    this.auth.isLoggedIn = true
    return isAccessTokenValid
  }

  async canActivateChild(): Promise<boolean> {
    return this.canActivate()
  }

  private isImpersonating(): boolean {
    return this.document.location.pathname === '/impersonation'
  }

  private isSingleSignOn(): boolean {
    const url = new URL(window.location.href)
    return !!url.searchParams.get('ssoToken')
  }

  private getQueryParamToken(): string {
    const match = /token=([^&]*)/.exec(this.document.location.search)
    if (!match) throw Error(`Missing url query param 'token'`)
    return match[1]
  }

  private getQueryParamSSOToken(): string {
    const match = /ssoToken=([^&]*)/.exec(this.document.location.search)
    if (!match) throw Error(`Missing url query param 'ssoToken'`)
    return match[1]
  }

  private isTokenValid(): boolean {
    const decodedToken = this.tokenHelper.getDecodedOCToken()

    if (!decodedToken) {
      this.appInsightsService.trackAuthErrorEvents(null, {message: 'HasTokenGuard: no token'})
      return false
    }

    const expiresIn = decodedToken.exp * 1000
    const isValid = Date.now() < expiresIn
    if (!isValid) {
      this.appInsightsService.trackAuthErrorEvents(decodedToken, {message: 'HasTokenGuard: token expired'})
    }

    return isValid
  }
}
