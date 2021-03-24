import { Injectable, Inject } from '@angular/core'
import { OcUserService, OcBuyerService, OcApiClientService } from '@ordercloud/angular-sdk'
import { applicationConfiguration } from '@app-seller/config/app.config'
import { AppConfig } from '@app-seller/models/environment.types'
import { BuyerTempService } from '../middleware-api/buyer-temp.service'

@Injectable({
  providedIn: 'root',
})
export class ImpersonationService {
  constructor(
    private userService: OcUserService,
    private buyerService: OcBuyerService,
    private apiClientService: OcApiClientService,
    private buyerTempService: BuyerTempService,
    @Inject(applicationConfiguration) private appConfig: AppConfig
  ) {}

  async impersonateUser(networkID: string, user: any): Promise<void> {
    const superBuyer = await this.buyerTempService.get(networkID)
    const {Buyer, ImpersonationConfig} = superBuyer;
    if(!Buyer?.xp?.URL || Buyer?.xp?.URL === null || 
      !ImpersonationConfig?.ClientID || ImpersonationConfig?.ClientID === null) {
      throw new Error(`${Buyer?.Name} is not configured for impersonation`)
    }
    const userCustomRoles = this.getCustomRolesForBuyerUser(user)
    const auth = await this.userService
      .GetAccessToken(networkID, user.ID, {
        ClientID: ImpersonationConfig.ClientID,
        Roles: this.appConfig.impersonatingBuyerScope,
        CustomRoles: userCustomRoles,
      })
      .toPromise()
    const url = Buyer.xp.URL[Buyer.xp.URL.length - 1] === '/' ?
      Buyer.xp.URL + 'impersonation?token=' + auth.access_token :
      Buyer.xp.URL + '/impersonation?token=' + auth.access_token 
    window.open(url, '_blank')
  }

  getCustomRolesForBuyerUser(user: any): string[] {
    return this.appConfig.impersonatingBuyerCustomRoleScope.filter((role) =>
      user.AvailableRoles.includes(role)
    )
  }
}
