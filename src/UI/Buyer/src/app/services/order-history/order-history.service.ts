import { Injectable } from '@angular/core'
import {
  Orders,
  LineItems,
  Me,
  Suppliers,
  SupplierAddresses,
  Tokens,
} from 'ordercloud-javascript-sdk'
import { ReorderHelperService } from '../reorder/reorder.service'
import { OrderFilterService } from './order-filter.service'
import {
  HSAddressBuyer,
  HSOrder,
  HSLineItem,
  OrderDetails,
  HSShipmentWithItems,
  HeadStartSDK,
} from '@ordercloud/headstart-sdk'
import { HttpHeaders, HttpClient } from '@angular/common/http'
import { AppConfig } from 'src/app/models/environment.types'
import { LineItemGroupSupplier } from 'src/app/models/line-item.types'
import { OrderReorderResponse } from 'src/app/models/order.types'
@Injectable({
  providedIn: 'root',
})
export class OrderHistoryService {
  activeOrderID: string // TODO - make this read-only in components

  constructor(
    public filters: OrderFilterService,
    private reorderHelper: ReorderHelperService,
    private httpClient: HttpClient,
    private appConfig: AppConfig,
  ) {}

  async getLocationsUserCanView(): Promise<HSAddressBuyer[]> {
    // add strong type when sdk is regenerated
    const accessUserGroups = await Me.ListUserGroups({
      filters: { 'xp.Role': 'ViewAllOrders' },
    })
    const locationRequests = accessUserGroups.Items.map((a) =>
      Me.GetAddress(a.xp.Location)
    )
    const locationResponses = await Promise.all(locationRequests)
    return locationResponses
  }

  async approveOrder(
    orderID: string = this.activeOrderID,
    Comments = '',
    AllowResubmit = false
  ): Promise<HSOrder> {
    const order = await Orders.Approve('Outgoing', orderID, {
      Comments,
      AllowResubmit,
    })
    return order as HSOrder
  }

  async declineOrder(
    orderID: string = this.activeOrderID,
    Comments = '',
    AllowResubmit = true
  ): Promise<HSOrder> {
    const order = await Orders.Decline('Outgoing', orderID, {
      Comments,
      AllowResubmit,
    })
    return order as HSOrder
  }

  async validateReorder(
    orderID: string = this.activeOrderID,
    lineItems: HSLineItem[]
  ): Promise<OrderReorderResponse> {
    return this.reorderHelper.validateReorder(orderID, lineItems)
  }

  async getOrderDetails(
    orderID: string = this.activeOrderID
  ): Promise<OrderDetails> {
    return await HeadStartSDK.Orders.GetOrderDetails(orderID)
  }

  async getLineItemSuppliers(
    liGroups: HSLineItem[][]
  ): Promise<LineItemGroupSupplier[]> {
    const suppliers: LineItemGroupSupplier[] = []
    for (const group of liGroups) {
      const line = group[0]
      if (line?.SupplierID) {
        const supplier = await Suppliers.Get(line.SupplierID)
        if (line.ShipFromAddressID) {
          const shipFrom = await SupplierAddresses.Get(
            line.SupplierID,
            line.ShipFromAddressID
          )
          suppliers.push({ supplier, shipFrom })
        } else {
          suppliers.push({ supplier, shipFrom: null })
        }
      }
    }
    return suppliers
  }

  async listShipments(
    orderID: string = this.activeOrderID
  ): Promise<HSShipmentWithItems[]> {
    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
      Authorization: `Bearer ${Tokens.GetAccessToken()}`,
    })
    const url = `${this.appConfig.middlewareUrl}/order/${orderID}/shipmentswithitems`
    return this.httpClient
      .get<HSShipmentWithItems[]>(url, { headers })
      .toPromise()
  }

  async submitCancelOrReturn(
    orderID: string,
    lineItemStatusChange: any
  ): Promise<void> {
    return HeadStartSDK.Orders.BuyerUpdateLineItemStatusesWithNotification(
      orderID,
      lineItemStatusChange
    )
  }
}
