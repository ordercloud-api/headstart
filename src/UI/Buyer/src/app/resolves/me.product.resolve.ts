import { Injectable } from '@angular/core'
import { Resolve, ActivatedRouteSnapshot } from '@angular/router'
import {} from 'ordercloud-javascript-sdk'
import {
  SuperHSProduct,
  HeadStartSDK,
} from '@ordercloud/headstart-sdk'
import { TempSdk } from '../services/temp-sdk/temp-sdk.service'

@Injectable()
export class MeProductResolver implements Resolve<SuperHSProduct> {
  constructor(private tempSdk: TempSdk) {}

  async resolve(
    route: ActivatedRouteSnapshot
  ): Promise<SuperHSProduct> {
    // TODO: strongly type this once headstart sdk includes ProductType 'Kit'
    const superProduct = (await HeadStartSDK.Mes.GetSuperProduct(
      route.params.productID
    )) as any
    if (superProduct.Product.xp.ProductType === 'Kit') {
      const kitProduct = await this.tempSdk.getMeKitProduct(
        superProduct.Product.ID
      )
      return kitProduct
    }
    return superProduct
  }
}
