import { Component, Input, Inject, Output, EventEmitter } from '@angular/core'
import { groupBy as _groupBy } from 'lodash'
import { applicationConfiguration } from '@app-seller/config/app.config'
import { HSLineItem, HSOrder } from '@ordercloud/headstart-sdk'
import {
  LineItem,
  LineItemSpec,
  MeUser,
  Orders,
} from 'ordercloud-javascript-sdk'
import { AppConfig, RegexService } from '@app-seller/shared'
import { getPrimaryLineItemImage } from '@app-seller/shared/services/assets/asset.helper'
import { LineItemStatusPipe } from '@app-seller/shared/pipes/lineitem-status.pipe'
import { TranslateService } from '@ngx-translate/core'

@Component({
  selector: 'app-line-item-table',
  templateUrl: './line-item-table.component.html',
  styleUrls: ['./line-item-table.component.scss'],
})
export class LineItemTableComponent {
  _lineItems: HSLineItem[] = []
  _order: HSOrder
  _liGroupedByShipFrom: HSLineItem[][]
  _supplierOrders: HSOrder[] = []
  _user: MeUser
  @Input()
  set order(value: HSOrder) {
    this._order = value
    void this.setSupplierOrders(value)
  }
  @Input() orderDirection: 'Incoming' | 'Outgoing'
  @Output() orderChange = new EventEmitter()
  isSaving = false

  @Input()
  set lineItems(value: HSLineItem[]) {
    this._lineItems = value
    if (value?.length) {
      this.setLineItemGroups(value)
    }
  }

  constructor(
    private regexService: RegexService,
    private lineitemStatusPipe: LineItemStatusPipe,
    private translateService: TranslateService,
    @Inject(applicationConfiguration) private appConfig: AppConfig
  ) {}

  getShipMethodString(lineItem: HSLineItem): string {
    const salesOrderID = this._order.ID.split('-')[0]
    const isPurchaseOrder = salesOrderID !== this._order.ID
    const supplierOrder = this._supplierOrders?.find(
      (order) => order.ID === `${salesOrderID}-${lineItem.SupplierID}`
    )
    const shipFromID = lineItem.ShipFromAddressID
    const shipMethod = (
      lineItem.SupplierID === null
        ? this._order?.xp?.SelectedShipMethodsSupplierView || []
        : supplierOrder?.xp?.SelectedShipMethodsSupplierView || []
    ).find((sm) => sm.ShipFromAddressID === shipFromID)
    if (shipMethod == null) return 'No Data'
    const name = shipMethod.Name.replace(/_/g, ' ')
    if (isPurchaseOrder) {
      return `${name}, Estimated ${shipMethod.EstimatedTransitDays} Day Delivery`
    }
    const delivery = new Date(this._order.DateSubmitted)
    delivery.setDate(delivery.getDate() + shipMethod.EstimatedTransitDays)
    return `${name},  ${delivery.toLocaleDateString('en-US')} Delivery`
  }

  setLineItemGroups(lineItems: HSLineItem[]): void {
    const liGroups = _groupBy(lineItems, (li) => li.ShipFromAddressID)
    this._liGroupedByShipFrom = Object.values(liGroups)
  }

  async setSupplierOrders(order: HSOrder): Promise<void> {
    const salesOrderID = order?.ID?.split('-')[0]
    if (order?.ID && salesOrderID && order.ID === salesOrderID) {
      const supplierOrderFilterString = order?.xp?.SupplierIDs?.map(
        (id) => `${order.ID}-${id}`
      ).join('|')
      const supplierOrders = await Orders.List(
        this.orderDirection === 'Incoming' ? 'Outgoing' : 'Incoming',
        {
          filters: { ID: supplierOrderFilterString },
        }
      )
      this._supplierOrders = supplierOrders.Items
    } else {
      this._supplierOrders = [order]
    }
  }

  getVariableTextSpecs = (li: LineItem): LineItemSpec[] =>
    li?.Specs?.filter((s) => s.OptionID === null)

  getLineItemStatusDisplay(lineItem: HSLineItem): string {
    if (!lineItem?.xp?.StatusByQuantity) {
      // If StatusByQuantity is missing this generally means that something failed during post submit (that's where those values are initialized)
      return 'N/A'
    }
    return Object.entries(lineItem.xp.StatusByQuantity)
      .filter(([, quantity]) => quantity)
      .map(([status, quantity]) => {
        const readableStatus = this.translateService.instant(
          this.lineitemStatusPipe.transform(status)
        ) as string
        return `${quantity} ${readableStatus}`
      })
      .join(', ')
  }

  getReturnOrCancelReason(reason: string): string {
    return this.regexService.getStatusSplitByCapitalLetter(reason)
  }

  getImageUrl(lineItemID: string): string {
    return getPrimaryLineItemImage(lineItemID, this._lineItems)
  }
}
