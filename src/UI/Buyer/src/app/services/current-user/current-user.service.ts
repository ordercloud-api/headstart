import { Injectable } from '@angular/core'
import { BehaviorSubject } from 'rxjs'
import { MeUser, Me, User, UserGroup, Tokens } from 'ordercloud-javascript-sdk'
import { TokenHelperService } from '../token-helper/token-helper.service'
import { CreditCardService } from './credit-card.service'
import { HttpClient, HttpHeaders } from '@angular/common/http'
import { CurrentUser } from 'src/app/models/profile.types'
import { AppConfig } from 'src/app/models/environment.types'
import { ContactSupplierBody } from 'src/app/models/buyer.types'
import { SitecoreSendTrackingService } from '../sitecore-send/sitecore-send-tracking.service'

@Injectable({
  providedIn: 'root',
})
export class CurrentUserService {
  private readonly MaxFavorites: number = 40
  private readonly favOrdersXP = 'FavoriteOrders'
  private readonly favProductsXP = 'FavoriteProducts'
  public isAnonSubject: BehaviorSubject<boolean>
  private userSubject: BehaviorSubject<CurrentUser> =
    new BehaviorSubject<CurrentUser>(null)

  // users for determining location management permissions for a user
  private userGroups: BehaviorSubject<UserGroup[]> = new BehaviorSubject<
    UserGroup[]
  >([])

  constructor(
    private tokenHelper: TokenHelperService,
    public cards: CreditCardService,
    public http: HttpClient,
    private appConfig: AppConfig,
    private send: SitecoreSendTrackingService
  ) {
    this.isAnonSubject = new BehaviorSubject(true)
  }

  get(): CurrentUser {
    return this.user
  }

  getUniqueReportingID(): string {
    if (this.isAnonymous()) {
      return `anon-${this.tokenHelper.getDecodedOCToken().orderid}`
    } else {
      return `${this.user.Buyer.ID}-${this.user.ID}`
    }
  }

  async reset(): Promise<void> {
    const requests: Promise<any>[] = [
      Me.Get(),
      Me.ListUserGroups({ pageSize: 100 }),
    ]
    const [user, userGroups] = await Promise.all(requests)
    this.user = await this.MapToCurrentUser(user)
    this.send.identify(this.user.Email)
    this.userGroups.next(userGroups.Items)
  }

  async patch(user: MeUser): Promise<CurrentUser> {
    const patched = await Me.Patch(user)
    this.user = await this.MapToCurrentUser(patched)
    return this.user
  }

  isAnonymous(): boolean {
    return this.tokenHelper.isTokenAnonymous()
  }

  isSSO(): boolean {
    return this.tokenHelper.getIsSSO()
  }

  onChange(callback: (user: CurrentUser) => void): void {
    this.userSubject.subscribe(callback)
  }

  setIsFavoriteProduct(isFav: boolean, productID: string): void {
    this.setFavoriteValue(this.favProductsXP, isFav, productID)
  }

  setIsFavoriteOrder(isFav: boolean, orderID: string): void {
    this.setFavoriteValue(this.favOrdersXP, isFav, orderID)
  }

  hasRoles(...roles: string[]): boolean {
    return roles.every((role) => this.user.AvailableRoles.includes(role))
  }

  hasLocationAccess(locationID: string, permissionType: string): boolean {
    const userGroupIDNeeded = `${locationID}-${permissionType}`
    return this.userGroups.value.some((u) => u.ID === userGroupIDNeeded)
  }

  async submitContactSupplierForm(
    contactRequest: ContactSupplierBody
  ): Promise<void> {
    const headers = new HttpHeaders({
      Authorization: `Bearer ${Tokens.GetAccessToken()}`,
    })
    const url = `${this.appConfig.middlewareUrl}/me/products/requestinfo`
    await this.http.post<void>(url, contactRequest, { headers }).toPromise()
  }

  private async MapToCurrentUser(user: MeUser): Promise<CurrentUser> {
    const currentUser = user as CurrentUser
    const myUserGroups = await Me.ListUserGroups()
    currentUser.UserGroups = myUserGroups.Items
    currentUser.FavoriteOrderIDs = this.getFavorites(user, this.favOrdersXP)
    currentUser.FavoriteProductIDs = this.getFavorites(user, this.favProductsXP)
    // Using `|| "USD"` for fallback right now in case there's bad data without the xp value.
    currentUser.Currency =
      myUserGroups.Items.filter((ug) => ug.xp.Type === 'BuyerLocation')[0]?.xp
        ?.Currency || 'USD'
    return currentUser
  }

  private get user(): CurrentUser {
    return this.userSubject.value
  }

  private set user(value: CurrentUser) {
    this.userSubject.next(value)
  }

  private getFavorites(user: MeUser, XpFieldName: string): string[] {
    return user && user.xp && user.xp[XpFieldName] instanceof Array
      ? user.xp[XpFieldName]
      : []
  }

  private async setFavoriteValue(
    XpFieldName: string,
    isFav: boolean,
    ID: string
  ): Promise<void> {
    if (!this.user.xp || !this.user.xp[XpFieldName]) {
      await this.patch({ xp: { [XpFieldName]: [] } })
    }
    let favorites = this.user.xp[XpFieldName] || []
    if (isFav && favorites.length >= this.MaxFavorites) {
      throw Error(`You have reached your limit of ${XpFieldName}`)
    }
    if (isFav) {
      favorites = [...favorites, ID]
    } else {
      favorites = favorites.filter((x) => x !== ID)
    }
    await this.patch({ xp: { [XpFieldName]: favorites } })
  }
}
