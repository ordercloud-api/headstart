import { NgModule } from '@angular/core'
import { SharedModule } from '@app-seller/shared'

import { ProductsRoutingModule } from '@app-seller/products/products-routing.module'
import { PerfectScrollbarModule } from 'ngx-perfect-scrollbar'
import { ProductTableComponent } from './components/product-table/product-table.component'
import { ProductEditComponent } from './components/product-edit/product-edit.component'
import { ProductTaxCodeSelect } from './components/product-tax-code-select/product-tax-code-select.component'
import { ProductTaxCodeSelectDropdown } from './components/product-tax-code-select-dropdown/product-tax-code-select-dropdown.component'
import { ProductVariations } from './components/product-variations/product-variations.component'
import { ProductFilters } from './components/product-filters/product-filters.component'
import { ProductPricingComponent } from './components/product-pricing/product-pricing.component'
import { PriceBreakEditor } from './components/price-break-editor/price-break-editor.component'
import {
  NgbModule,
  NgbTooltipModule,
} from '@ng-bootstrap/ng-bootstrap'

@NgModule({
  imports: [
    SharedModule,
    ProductsRoutingModule,
    PerfectScrollbarModule,
    NgbModule,
    NgbTooltipModule,
  ],
  declarations: [
    ProductTableComponent,
    ProductEditComponent,
    ProductPricingComponent,
    PriceBreakEditor,
    ProductTaxCodeSelect,
    ProductTaxCodeSelectDropdown,
    ProductVariations,
    ProductFilters,
  ],
})
export class ProductsModule { }
