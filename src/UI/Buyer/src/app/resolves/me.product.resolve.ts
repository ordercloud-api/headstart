import { Injectable } from '@angular/core'
import { Resolve, ActivatedRouteSnapshot } from '@angular/router'
import { SuperHSProduct, HeadStartSDK } from '@ordercloud/headstart-sdk'

@Injectable()
export class MeProductResolver implements Resolve<SuperHSProduct> {
  async resolve(route: ActivatedRouteSnapshot): Promise<SuperHSProduct> {
     return await HeadStartSDK.Mes.GetSuperProduct(route.params.productID)
  }
}
