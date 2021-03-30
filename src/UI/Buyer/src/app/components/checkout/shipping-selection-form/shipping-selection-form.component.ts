import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core'
import {
  ShipEstimate,
  ShipMethod,
  ShipMethodSelection,
} from 'ordercloud-javascript-sdk'
import { FormGroup, FormControl } from '@angular/forms'
import {
  faExclamationTriangle,
  faQuestionCircle,
} from '@fortawesome/free-solid-svg-icons'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'

@Component({
  templateUrl: './shipping-selection-form.component.html',
  styleUrls: ['./shipping-selection-form.component.scss'],
})
export class OCMShippingSelectionForm implements OnInit {
  @Input() set shipEstimate(value: ShipEstimate) {
    this._shipEstimate = value
    this.setSelectedRate(value.SelectedShipMethodID)
  }
  @Input() shipFromAddressID: string
  @Input() supplierID: string
  @Output() selectionChanged = new EventEmitter<ShipMethodSelection>()

  faQuestionCircle = faQuestionCircle
  faExclamationTriangle = faExclamationTriangle
  _shipEstimate: ShipEstimate
  _orderCurrency: string
  form: FormGroup

  constructor(private context: ShopperContextService) {}

  ngOnInit(): void {
    this.form = new FormGroup({ rateID: new FormControl(null) })
    this.form = new FormGroup({ ShipMethodID: new FormControl(null) })
    this._orderCurrency = this.context.currentUser.get().Currency
  }

  setSelectedRate(selectedShipMethodID: string): void {
    this.form.setValue({ ShipMethodID: selectedShipMethodID })
  }

  onFormChanges(): void {
    const selectedShipMethodID = this.form.value.ShipMethodID
    this.selectionChanged.emit({
      ShipMethodID: selectedShipMethodID,
      ShipEstimateID: this._shipEstimate.ID,
    })
  }

  detectPlural(value: number): string {
    return value === 1 ? '' : 's'
  }

  getEstTransitDays(method: ShipMethod): number {
    // If current day is Friday, Standard Overnight will return 3 days
    // as the estimated delivery days since there is only delivery on
    // business days.  We want to override this so it is always 1 day.
    // Documentation: https://www.easypost.com/fedex-guide#levels
    if (method.Name === 'STANDARD_OVERNIGHT') {
      return 1
    }
    return method.EstimatedTransitDays
  }
}
