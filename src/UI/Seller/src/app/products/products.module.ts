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
  NgbDateAdapter,
  NgbDateNativeAdapter,
  NgbModule,
  NgbTooltipModule,
} from '@ng-bootstrap/ng-bootstrap'
import { PriceDisplayComponent } from './components/price-display/price-display.component'
import { ProductBundleEditComponent } from './components/product-bundle-edit/product-bundle-edit.component';
import { ProductImageUploadComponent } from './components/product-image-upload/product-image-upload.component';
import { ProductDocumentUploadComponent } from './components/product-document-upload/product-document-upload.component'

@NgModule({
  imports: [
    SharedModule,
    ProductsRoutingModule,
    PerfectScrollbarModule,
    NgbModule,
    NgbTooltipModule,
  ],
  providers: [{ provide: NgbDateAdapter, useClass: NgbDateNativeAdapter }],
  declarations: [
    ProductTableComponent,
    ProductEditComponent,
    ProductPricingComponent,
    PriceBreakEditor,
    ProductTaxCodeSelect,
    ProductTaxCodeSelectDropdown,
    ProductVariations,
    ProductFilters,
    PriceDisplayComponent,
    ProductBundleEditComponent,
    ProductImageUploadComponent,
    ProductDocumentUploadComponent,
  ],
})
export class ProductsModule {}
