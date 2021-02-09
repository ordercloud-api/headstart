import { Injectable, Inject } from '@angular/core'
import { OcUserService, OcBuyerService } from '@ordercloud/angular-sdk'
import { applicationConfiguration } from '@app-seller/config/app.config'
import { AppConfig } from '@app-seller/models/environment.types'

@Injectable({
  providedIn: 'root',
})
export class ImpersonationService {
  constructor(
    private userService: OcUserService,
    private buyerService: OcBuyerService,
    @Inject(applicationConfiguration) private appConfig: AppConfig
  ) {}

  async impersonateUser(networkID: string, user: any): Promise<void> {
    const buyer = await this.buyerService.Get(networkID).toPromise()
    const buyerConfig = this.appConfig.buyerConfigs[buyer.Name]
    const userCustomRoles = this.getCustomRolesForBuyerUser(user)
    const auth = await this.userService
      .GetAccessToken(networkID, user.ID, {
        ClientID: buyerConfig.clientID,
        Roles: this.appConfig.impersonatingBuyerScope,
        CustomRoles: userCustomRoles,
      })
      .toPromise()
    const url =
      buyerConfig.buyerUrl + 'impersonation?token=' + auth.access_token
    window.open(url, '_blank')
  }

  getCustomRolesForBuyerUser(user: any): string[] {
    return this.appConfig.impersonatingBuyerCustomRoleScope.filter((role) =>
      user.AvailableRoles.includes(role)
    )
  }
}
