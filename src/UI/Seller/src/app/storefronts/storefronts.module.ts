import { NgModule } from '@angular/core'
import { SharedModule } from '@app-seller/shared'
import { StorefrontTableComponent } from './components/storefronts/storefronts-table/storefront-table.component'
import { StorefrontsRoutingModule } from './storefronts-routing.module'
import { StorefrontEditComponent } from './components/storefronts/storefronts-edit/storefront-edit.component'
import {
  NgbTooltipModule,
  NgbTypeaheadModule,
} from '@ng-bootstrap/ng-bootstrap'
import { RouterModule } from '@angular/router'

@NgModule({
  imports: [
    SharedModule,
    StorefrontsRoutingModule,
    NgbTooltipModule,
    NgbTypeaheadModule, RouterModule
  ],
  declarations: [
    StorefrontTableComponent,
    StorefrontEditComponent,
  ],
})
export class StorefrontsModule { }
