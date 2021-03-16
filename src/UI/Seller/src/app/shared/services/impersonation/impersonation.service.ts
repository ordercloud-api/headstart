import { Injectable, Inject } from '@angular/core'
import { OcUserService, OcBuyerService, OcApiClientService } from '@ordercloud/angular-sdk'
import { applicationConfiguration } from '@app-seller/config/app.config'
import { AppConfig } from '@app-seller/models/environment.types'
import { listAll } from '../listAll'
import { ApiClients, ApiClientAssignment } from 'ordercloud-javascript-sdk'

@Injectable({
  providedIn: 'root',
})
export class ImpersonationService {
  constructor(
    private userService: OcUserService,
    private buyerService: OcBuyerService,
    private apiClientService: OcApiClientService,
    @Inject(applicationConfiguration) private appConfig: AppConfig
  ) {}

  async impersonateUser(networkID: string, user: any): Promise<void> {
    const buyer = await this.buyerService.Get(networkID).toPromise()
    if(!buyer?.xp?.URL || !buyer?.xp?.ClientID) {
      throw new Error("Buyer not configured for impersonation")
    }
    const userCustomRoles = this.getCustomRolesForBuyerUser(user)
    const auth = await this.userService
      .GetAccessToken(networkID, user.ID, {
        ClientID: buyer.xp.ClientID,
        Roles: this.appConfig.impersonatingBuyerScope,
        CustomRoles: userCustomRoles,
      })
      .toPromise()
    const url =
      buyer.xp.URL + 'impersonation?token=' + auth.access_token
    window.open(url, '_blank')
  }

  getCustomRolesForBuyerUser(user: any): string[] {
    return this.appConfig.impersonatingBuyerCustomRoleScope.filter((role) =>
      user.AvailableRoles.includes(role)
    )
  }
}
