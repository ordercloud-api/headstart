import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core'
import { UntypedFormGroup } from '@angular/forms'
import { SuperHSProduct, TaxCategorization } from '@ordercloud/headstart-sdk'
import {
  faTimesCircle,
  faCheckCircle,
  faAsterisk,
} from '@fortawesome/free-solid-svg-icons'

@Component({
  selector: 'product-tax-code-select-component',
  templateUrl: './product-tax-code-select.component.html',
  styleUrls: ['./product-tax-code-select.component.scss'],
})
export class ProductTaxCodeSelect {
  @Input() productForm: UntypedFormGroup
  @Input() superHSProductEditable: SuperHSProduct
  @Input() taxCodes: TaxCategorization[]
  @Output() handleTaxCodeSelection = new EventEmitter<any>()
  @Output() handleTaxCodesSearched = new EventEmitter<string>()
  @Input() readonly = false
  @Input() isRequired: boolean
  @Input() isCreatingNew: boolean
  faTimesCircle = faTimesCircle
  faCheckCircle = faCheckCircle
  faAsterisk = faAsterisk

  handleSelectTaxCode(taxCodeSelection: TaxCategorization): void {
    const event = {
      target: {
        value: taxCodeSelection,
      },
    }
    this.handleTaxCodeSelection.emit(event)
  }

  onTaxCodesSearched(searchTerm: string): void {
    this.handleTaxCodesSearched.emit(searchTerm)
  }

  taxSelectionsValid(): boolean {
    if (!this.isRequired) {
      return true
    }
    return this.isCreatingNew && this.productForm.controls['TaxCode'].valid
  }
}
