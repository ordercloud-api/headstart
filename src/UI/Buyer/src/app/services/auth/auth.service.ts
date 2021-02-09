import { Injectable } from '@angular/core'
import { Observable, of, BehaviorSubject, from } from 'rxjs'
import { tap, catchError, finalize } from 'rxjs/operators'
import { Router } from '@angular/router'

// 3rd party
import {
  Tokens,
  AccessToken,
  Me,
  Auth,
  MeUser,
  ForgottenPassword,
  AccessTokenBasic,
  TokenPasswordReset,
} from 'ordercloud-javascript-sdk'
// import { CookieService } from '@gorniv/ngx-universal';
import { CookieService } from 'ngx-cookie'
import { CurrentUserService } from '../current-user/current-user.service'
import { CurrentOrderService } from '../order/order.service'
import { HeadStartSDK } from '@ordercloud/headstart-sdk'
import { OrdersToApproveStateService } from '../order-history/order-to-approve-state.service'
import { ApplicationInsightsService } from '../application-insights/application-insights.service'
import { TokenHelperService } from '../token-helper/token-helper.service'
import { ContentManagementClient } from '@ordercloud/cms-sdk'
import { AppConfig } from 'src/app/models/environment.types'

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  fetchingRefreshToken = false
  failedRefreshAttempt = false
  refreshToken: BehaviorSubject<string> = new BehaviorSubject<string>('')
  private rememberMeCookieName = `${this.appConfig.appname
    .replace(/ /g, '_')
    .toLowerCase()}_rememberMe`
  private loggedInSubject: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(
    false
  )

  constructor(
    private cookieService: CookieService,
    private router: Router,
    private currentOrder: CurrentOrderService,
    private currentUser: CurrentUserService,
    private ordersToApproveStateService: OrdersToApproveStateService,
    private appConfig: AppConfig,
    private tokenHelper: TokenHelperService,
    private appInsightsService: ApplicationInsightsService
  ) {}

  // All this isLoggedIn stuff is only used in the header wrapper component
  // remove once its no longer needed.

  get isLoggedIn(): boolean {
    return this.loggedInSubject.value
  }

  set isLoggedIn(value: boolean) {
    this.loggedInSubject.next(value)
  }

  onIsLoggedInChange(callback: (isLoggedIn: boolean) => void): void {
    this.loggedInSubject.subscribe(callback)
  }

  // change all this unreadable observable stuff
  refresh(): Observable<void> {
    this.fetchingRefreshToken = true
    return from(this.refreshTokenLogin()).pipe(
      tap((token) => {
        this.refreshToken.next(token.access_token)
      }),
      catchError(() => {
        // ignore new refresh attempts if a refresh
        // attempt failed within the last 3 seconds
        this.failedRefreshAttempt = true
        setTimeout(() => {
          this.failedRefreshAttempt = false
        }, 3000)
        this.logout()
        return of(null)
      }),
      finalize(() => {
        this.fetchingRefreshToken = false
      })
    )
  }

  setToken(token: string): void {
    if (!token) return
    Tokens.SetAccessToken(token)
    this.isLoggedIn = true
  }

  async forgotPasssword(email: string): Promise<void> {
    await ForgottenPassword.SendVerificationCode({
      Email: email,
      ClientID: this.appConfig.clientID,
      URL: this.appConfig.baseUrl,
    })
    this.router.navigateByUrl('/login')
  }

  async register(me: MeUser<any>): Promise<AccessTokenBasic> {
    const token = await Me.Register(me)
    return token
  }

  async profiledLogin(
    userName: string,
    password: string,
    rememberMe = false
  ): Promise<AccessToken> {
    const creds = await Auth.Login(
      userName,
      password,
      this.appConfig.clientID,
      this.appConfig.scope
    )
    this.appInsightsService.setUserID(userName)
    this.loginWithTokens(
      creds.access_token,
      creds.refresh_token,
      false,
      rememberMe
    )
    this.router.navigateByUrl('/home')
    return creds
  }

  loginWithTokens(
    token: string,
    refreshToken?: string,
    isSSO = false,
    rememberMe = false
  ): void {
    this.tokenHelper.setIsSSO(isSSO)
    ContentManagementClient.Tokens.SetAccessToken(token)
    HeadStartSDK.Tokens.SetAccessToken(token)
    this.setToken(token)
    if (rememberMe && refreshToken) {
      /**
       * set the token duration in the dashboard - https://developer.ordercloud.io/dashboard/settings
       * refresh tokens are configured per clientID and initially set to 0
       * a refresh token of 0 means no refresh token is returned in OAuth response
       */
      Tokens.SetRefreshToken(refreshToken)
      this.setRememberMeStatus(true)
    }
    this.ordersToApproveStateService.alertIfOrdersToApprove()
  }

  async anonymousLogin(): Promise<AccessToken> {
    try {
      const creds = await Auth.Anonymous(
        this.appConfig.clientID,
        this.appConfig.scope
      )
      ContentManagementClient.Tokens.SetAccessToken(creds.access_token)
      HeadStartSDK.Tokens.SetAccessToken(creds.access_token)
      this.setToken(creds.access_token)
      return creds
    } catch (err) {
      this.logout()
      throw new Error(err)
    }
  }

  async logout(): Promise<void> {
    Tokens.RemoveAccessToken()
    ContentManagementClient.Tokens.RemoveAccessToken()
    HeadStartSDK.Tokens.RemoveAccessToken()
    this.isLoggedIn = false
    this.appInsightsService.clearUser()
    if (this.appConfig.anonymousShoppingEnabled) {
      this.router.navigate(['/home'])
      await this.currentUser.reset()
      this.currentOrder.reset() // TODO - can we get rid of Auth's dependency on current Order and User?
    } else {
      this.router.navigate(['/login'])
    }
  }

  async validateCurrentPasswordAndChangePassword(
    newPassword: string,
    currentPassword: string
  ): Promise<void> {
    // reset password route does not require old password, so we are handling that here through a login
    await Auth.Login(
      this.currentUser.get().Username,
      currentPassword,
      this.appConfig.clientID,
      this.appConfig.scope
    )
    await Me.ResetPasswordByToken({ NewPassword: newPassword })
  }

  async resetPassword(
    token: string,
    config: TokenPasswordReset
  ): Promise<void> {
    await Me.ResetPasswordByToken(config, { accessToken: token })
    // await ForgottenPassword.ResetPasswordByVerificationCode(code, config);
  }

  setRememberMeStatus(status: boolean): void {
    this.cookieService.putObject(this.rememberMeCookieName, { status })
  }

  getRememberStatus(): boolean {
    const rememberMe = this.cookieService.getObject(
      this.rememberMeCookieName
    ) as { status: string }
    return !!(rememberMe && rememberMe.status)
  }

  private async refreshTokenLogin(): Promise<AccessToken> {
    try {
      const refreshToken = Tokens.GetRefreshToken()
      const creds = await Auth.RefreshToken(
        refreshToken,
        this.appConfig.clientID
      )
      this.setToken(creds.access_token)
      return creds
    } catch (err) {
      if (this.appConfig.anonymousShoppingEnabled) {
        return this.anonymousLogin()
      } else {
        throw new Error(err)
      }
    }
  }
}
