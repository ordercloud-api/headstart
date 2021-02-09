// core services
import { NgModule } from '@angular/core'
import { RouterModule, Routes } from '@angular/router'
import { KitsTableComponent } from './components/kits-table/kits-table.component'

const routes: Routes = [
  { path: '', component: KitsTableComponent },
  { path: 'new', component: KitsTableComponent, pathMatch: 'full' },
  { path: ':kitproductID', component: KitsTableComponent, pathMatch: 'full' },
  {
    path: ':kitProductID/catalog-assignments',
    component: KitsTableComponent,
    pathMatch: 'full',
  },
]
@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class KitsRoutingModule {}
