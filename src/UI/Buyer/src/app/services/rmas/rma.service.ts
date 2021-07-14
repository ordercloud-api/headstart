import { Injectable } from '@angular/core'
import { HeadStartSDK, RMA } from '@ordercloud/headstart-sdk'
import { CosmosListPage } from '@ordercloud/headstart-sdk/dist/models/CosmosListPage'

@Injectable({
  providedIn: 'root',
})
export class RMAService {
  async listRMAsForOrder(orderID: string): Promise<CosmosListPage<RMA>> {
    return await HeadStartSDK.Orders.ListRMAsForOrder(orderID)
  }
}
