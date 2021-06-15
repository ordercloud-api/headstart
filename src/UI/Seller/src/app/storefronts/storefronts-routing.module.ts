// core services
import { NgModule } from '@angular/core'
import { RouterModule, Routes } from '@angular/router'
import { StorefrontTableComponent } from './components/storefronts/storefronts-table/storefront-table.component'

const routes: Routes = [
  { path: '', component: StorefrontTableComponent },
  { path: 'new', component: StorefrontTableComponent },
  { path: ':storefrontID', component: StorefrontTableComponent },
]

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class StorefrontsRoutingModule { }
