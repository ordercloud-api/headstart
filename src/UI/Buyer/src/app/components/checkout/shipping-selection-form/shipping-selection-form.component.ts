import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core'
import { ShipEstimate, ShipMethodSelection } from 'ordercloud-javascript-sdk'
import { UntypedFormGroup, UntypedFormControl } from '@angular/forms'
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
  form: UntypedFormGroup

  constructor(private context: ShopperContextService) {}

  ngOnInit(): void {
    this.form = new UntypedFormGroup({ rateID: new UntypedFormControl(null) })
    this.form = new UntypedFormGroup({ ShipMethodID: new UntypedFormControl(null) })
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
}
