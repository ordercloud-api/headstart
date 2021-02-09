import {
  Component,
  Input,
  Output,
  EventEmitter,
  ViewChild,
  OnChanges,
  SimpleChanges,
} from '@angular/core'
import { NgbPopover } from '@ng-bootstrap/ng-bootstrap'
import {
  SuperHSProduct,
  ListPage,
  TaxProperties,
} from '@ordercloud/headstart-sdk'
import { faTimesCircle, faCheckCircle } from '@fortawesome/free-solid-svg-icons'
import { FormGroup } from '@angular/forms'

@Component({
  selector: 'product-tax-code-select-dropdown',
  templateUrl: './product-tax-code-select-dropdown.component.html',
  styleUrls: ['./product-tax-code-select-dropdown.component.scss'],
})
export class ProductTaxCodeSelectDropdown implements OnChanges {
  @Input()
  productForm: FormGroup
  @Input()
  taxCodes: ListPage<TaxProperties>
  @Input()
  superHSProductEditable: SuperHSProduct
  @Input()
  readonly: boolean
  @Input()
  isRequired: boolean
  @Input()
  isCreatingNew: boolean

  @Output()
  taxCodesSearched = new EventEmitter<any>()
  @Output()
  onScrollEnd = new EventEmitter<string>()
  @Output()
  onSelectTaxCode = new EventEmitter<TaxProperties>()

  faTimesCircle = faTimesCircle
  faCheckCircle = faCheckCircle
  @ViewChild('popover', { static: false })
  public popover: NgbPopover
  searchTerm = ''
  productTaxCodeSelectDropdownHeight = 250

  ngOnChanges(changes: SimpleChanges) {
    if (
      changes?.superHSProductEditable?.previousValue &&
      changes.superHSProductEditable.previousValue.Product.ID !==
        changes.superHSProductEditable.currentValue.Product.ID
    ) {
      this.searchTerm = ''
    }
  }

  searchedTaxCodes(searchText: any) {
    this.searchTerm = searchText
    this.taxCodesSearched.emit(this.searchTerm)
  }

  selectTaxCode(taxCode: TaxProperties) {
    // To clear the tax code search term when a selection is made - to refresh the list back to the starting state.
    if (this.searchTerm !== '') {
      this.searchTerm = ''
      this.taxCodesSearched.emit(this.searchTerm)
    }
    const { Category, ...rest } = taxCode
    const Tax = {
      ...rest,
      Category: this.superHSProductEditable.Product.xp?.Tax.Category,
    }
    this.onSelectTaxCode.emit(Tax)
  }

  handleScrollEnd(event) {
    if (event.target.classList.value.includes('active')) {
      this.onScrollEnd.emit(this.searchTerm)
    }
  }
}
