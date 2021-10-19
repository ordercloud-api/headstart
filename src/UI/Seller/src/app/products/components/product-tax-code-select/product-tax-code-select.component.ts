import { Component, Input, Output, EventEmitter } from '@angular/core'
import { FormGroup } from '@angular/forms'

import {
  SuperHSProduct,
  ListPage,
  TaxCategorization,
} from '@ordercloud/headstart-sdk'
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
  @Input()
  productForm: FormGroup
  @Input()
  superHSProductEditable: SuperHSProduct
  @Input()
  taxCodes: TaxCategorization[]
  @Output()
  handleTaxCodeSelection = new EventEmitter<any>()
  @Output()
  handleTaxCodesSearched = new EventEmitter<string>()
  @Output()
  handleIsResale = new EventEmitter<boolean>()
  @Input()
  readonly = false
  @Input()
  isRequired: boolean
  @Input()
  isCreatingNew: boolean
  faTimesCircle = faTimesCircle
  faCheckCircle = faCheckCircle
  faAsterisk = faAsterisk

  handleIsResaleInput(event: boolean): void {
    return this.handleIsResale.emit(event)
  }

  handleSelectTaxCode(taxCodeSelection: TaxCategorization): void {
    const event = {
      target: {
        value: taxCodeSelection,
      },
    }
    this.handleTaxCodeSelection.emit(event)
  }

  onTaxCodesSearched(searchTerm: string) {
    this.handleTaxCodesSearched.emit(searchTerm)
  }

  taxSelectionsValid(): boolean {
    return (
      this.isCreatingNew &&
      this.isRequired &&
      this.productForm.controls['TaxCode'].valid
    )
  }
}
