// core services
import { NgModule } from '@angular/core'
import { RouterModule, Routes } from '@angular/router'
import { FacetTableComponent } from './components/facet-table/facet-table.component'

const routes: Routes = [
  { path: '', component: FacetTableComponent },
  { path: 'new', component: FacetTableComponent },
  { path: ':facetID', component: FacetTableComponent },
]
@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class FacetsRoutingModule {}
