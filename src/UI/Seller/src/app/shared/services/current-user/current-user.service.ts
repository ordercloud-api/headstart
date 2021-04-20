import { Injectable, Inject } from '@angular/core'
import {
  Supplier,
  OcMeService,
  OcAuthService,
  OcTokenService,
  MeUser,
  OcSupplierService,
} from '@ordercloud/angular-sdk'
import { applicationConfiguration } from '@app-seller/config/app.config'
import { AppAuthService } from '@app-seller/auth/services/app-auth.service'
import { AppStateService } from '../app-state/app-state.service'
import { HeadStartSDK, ImageAsset } from '@ordercloud/headstart-sdk'
import { Tokens } from 'ordercloud-javascript-sdk'
import { BehaviorSubject } from 'rxjs'
import { AppConfig } from '@app-seller/models/environment.types'
import { UserContext } from '@app-seller/models/user.types'

@Injectable({
  providedIn: 'root',
})
export class CurrentUserService {
  me: MeUser
  mySupplier: Supplier
  public userSubject: BehaviorSubject<MeUser<any>> = new BehaviorSubject<
    MeUser<any>
  >({})
  public profileImgSubject: BehaviorSubject<ImageAsset> = new BehaviorSubject<ImageAsset>(
    {}
  )
  constructor(
    private ocMeService: OcMeService,
    private ocAuthService: OcAuthService,
    @Inject(applicationConfiguration) private appConfig: AppConfig,
    private ocTokenService: OcTokenService,
    private appAuthService: AppAuthService,
    private appStateService: AppStateService,
    private ocSupplierService: OcSupplierService
  ) { }

  async login(username: string, password: string, rememberMe: boolean) {
    const accessToken = await this.ocAuthService
      .Login(username, password, this.appConfig.clientID, this.appConfig.scope)
      .toPromise()

    if (rememberMe && accessToken.refresh_token) {
      /**
       * set the token duration in the dashboard - https://developer.ordercloud.io/dashboard/settings
       * refresh tokens are configured per clientID and initially set to 0
       * a refresh token of 0 means no refresh token is returned in OAuth accessToken
       */
      this.ocTokenService.SetRefresh(accessToken.refresh_token)
      this.appAuthService.setRememberStatus(true)
    }
    HeadStartSDK.Tokens.SetAccessToken(accessToken.access_token)
    Tokens.SetAccessToken(accessToken.access_token)
    this.ocTokenService.SetAccess(accessToken.access_token)
    this.appStateService.isLoggedIn.next(true)
    this.me = await this.ocMeService.Get().toPromise()
    this.userSubject.next(this.me)
    if (this.me?.Supplier) {
      this.mySupplier = await HeadStartSDK.Suppliers.GetMySupplier(
        this.me?.Supplier?.ID
      )
    }
  }

  async getUser(): Promise<MeUser> {
    return this.me ? this.me : await this.refreshUser()
  }

  async patchUser(patchObj: Partial<MeUser>): Promise<MeUser> {
    const patchedUser = await this.ocMeService.Patch(patchObj).toPromise()
    this.userSubject.next(patchedUser)
    this.me = patchedUser
    return this.me
  }

  async refreshUser(): Promise<MeUser> {
    this.me = await this.ocMeService.Get().toPromise()
    this.userSubject.next(this.me)
    return this.me
  }

  async getMySupplier(): Promise<Supplier> {
    const me = await this.getUser()
    if (!me.Supplier) return
    return this.mySupplier && this.mySupplier.ID === me.Supplier.ID
      ? this.mySupplier
      : await this.refreshSupplier(me.Supplier.ID)
  }

  async refreshSupplier(supplierID): Promise<Supplier> {
    const token = await this.ocTokenService.GetAccess()
    this.mySupplier = await HeadStartSDK.Suppliers.GetMySupplier(
      supplierID,
      token
    )
    return this.mySupplier
  }

  async getUserContext(): Promise<UserContext> {
    const UserContext: UserContext = await this.constructUserContext()
    return UserContext
  }

  async constructUserContext(): Promise<UserContext> {
    const me: MeUser = await this.getUser()
    const userType = await this.appAuthService.getOrdercloudUserType()
    const userRoles = await this.appAuthService.getUserRoles()
    return {
      Me: me,
      UserType: userType,
      UserRoles: userRoles,
    }
  }

  async isSupplierUser() {
    const me = await this.getUser()
    return me.Supplier ? true : false
  }

  onChange(callback: (user: MeUser) => void): void {
    this.userSubject.subscribe(callback)
  }

  private get user(): MeUser {
    return this.userSubject.value
  }

  private set user(value: MeUser) {
    this.userSubject.next(value)
  }
}
