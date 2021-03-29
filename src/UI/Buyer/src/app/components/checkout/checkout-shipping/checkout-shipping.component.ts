import { Component, OnInit, Output, EventEmitter, Input } from '@angular/core'
import {
  HSOrder,
  HSLineItem,
} from '@ordercloud/headstart-sdk'
import {
  ShipEstimate,
  ListPage,
  ShipMethodSelection,
} from 'ordercloud-javascript-sdk'
import { faExclamationCircle } from '@fortawesome/free-solid-svg-icons'
import { CheckoutService } from 'src/app/services/order/checkout.service'
import { LineItemGroupSupplier } from 'src/app/models/line-item.types'

function notEmpty<TValue>(value: TValue | null | undefined): value is TValue {
  return value !== null && value !== undefined
}

@Component({
  templateUrl: './checkout-shipping.component.html',
  styleUrls: ['./checkout-shipping.component.scss'],
})
export class OCMCheckoutShipping implements OnInit {
  @Input() order?: HSOrder
  @Input() set lineItems(value: ListPage<HSLineItem>) {
    this._lineItems = value;
    this.getSupplierData()
  } 
  @Input() set shipEstimates(value: ShipEstimate[]) {
    this._shipEstimates = value
    this._areAllShippingSelectionsMade = this.areAllShippingSelectionsMade(
      value
    )
    this._lineItemsByShipEstimate = value
      .map((shipEstimate) => {
        return this.getLineItemsForShipEstimate(shipEstimate)
      })
  }
  @Output() selectShipRate = new EventEmitter<ShipMethodSelection>()
  @Output() continue = new EventEmitter()
  @Output() backToAddress = new EventEmitter()
  _shipEstimates?: ShipEstimate[]
  _areAllShippingSelectionsMade = false
  _lineItemsByShipEstimate?: HSLineItem[][]
  faExclamationCircle = faExclamationCircle
  _lineItems: ListPage<HSLineItem>
  _supplierData: LineItemGroupSupplier[]

  constructor(private checkoutService: CheckoutService) {
    this.order = undefined
    this.lineItems = undefined
  }

  ngOnInit(): void {}

  async getSupplierData(): Promise<void> {
    if(this._lineItems) {
      this._supplierData = await this.checkoutService.buildSupplierData(this._lineItems.Items)
    }
  }

  getLineItemsForShipEstimate(
    shipEstimate: ShipEstimate
  ): HSLineItem[] {
    if (!shipEstimate.ShipEstimateItems) {
      return []
    }
    if (!this._lineItems || !this._lineItems?.Items) {
      return []
    }
    const lineItemsByShipFromAddressID = this._lineItems?.Items?.filter(
      (li) => li.ShipFromAddressID === shipEstimate?.xp?.ShipFromAddressID
    )
    return lineItemsByShipFromAddressID.filter(notEmpty)
  }

  areAllShippingSelectionsMade(shipEstimates: ShipEstimate[]): boolean {
    return shipEstimates.every(
      (shipEstimate) => shipEstimate.SelectedShipMethodID
    )
  }

  getSupplierID(shipEstimate: ShipEstimate): string | undefined {
    if (!this.order || !this.order.xp) return
    const line = this.getFirstLineItem(shipEstimate)
    return line?.SupplierID
  }

  onChangeAddressClicked(): void {
    this.backToAddress.emit()
  }

  getShipFromAddressID(shipEstimate: ShipEstimate): string | undefined {
    if (!this.order || !this.order.xp) return
    const line = this.getFirstLineItem(shipEstimate)
    return line?.ShipFromAddressID
  }

  getFirstLineItem(
    shipEstimate: ShipEstimate
  ): HSLineItem | undefined {
    if (
      !shipEstimate ||
      !shipEstimate.ShipEstimateItems ||
      !shipEstimate.ShipEstimateItems.length
    ) {
      return
    }
    if (!this.lineItems || !this.lineItems.Items) {
      return
    }
    const firstLineItemID = shipEstimate.ShipEstimateItems[0].LineItemID
    return this._lineItems.Items.find(
      (lineItem) => lineItem.ID === firstLineItemID
    )
  }

  selectRate(selection: ShipMethodSelection): void {
    this.selectShipRate.emit(selection)
  }

  onContinueClicked(): void {
    this.continue.emit()
  }
}
