import { NgModule } from '@angular/core'
import { SharedModule } from '@app-seller/shared'

import { PerfectScrollbarModule } from 'ngx-perfect-scrollbar'
import { FacetsRoutingModule } from './facets-routing.module'
import { FacetTableComponent } from './components/facet-table/facet-table.component'
import { FacetEditComponent } from './components/facet-edit/facet-edit.component'

@NgModule({
  imports: [SharedModule, FacetsRoutingModule, PerfectScrollbarModule],
  declarations: [FacetTableComponent, FacetEditComponent],
})
export class FacetsModule {}
