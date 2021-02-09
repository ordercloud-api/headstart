import { NgModule } from '@angular/core'
import { SharedModule } from '@app-seller/shared'
import { StorefrontTableComponent } from './components/storefronts/storefronts-table/storefront-table.component'
import { StorefrontsRoutingModule } from './storefronts-routing.module'
import { StorefrontEditComponent } from './components/storefronts/storefronts-edit/storefront-edit.component'
import { StorefrontPageTableComponent } from './components/pages/storefront-page-table/storefront-page-table.component'

@NgModule({
  imports: [SharedModule, StorefrontsRoutingModule],
  declarations: [
    StorefrontTableComponent,
    StorefrontEditComponent,
    StorefrontPageTableComponent,
  ],
})
export class StorefrontsModule {}
