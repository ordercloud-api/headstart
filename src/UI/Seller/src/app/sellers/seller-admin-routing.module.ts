// core services
import { NgModule } from '@angular/core'
import { RouterModule, Routes } from '@angular/router'
import { SellerUserTableComponent } from './components/users/seller-user-table/seller-user-table.component'
import { SellerLocationTableComponent } from './components/locations/seller-location-table/seller-location-table.component'

const routes: Routes = [
  { path: '', component: SellerUserTableComponent },
  { path: 'users', component: SellerUserTableComponent },
  { path: 'users/new', component: SellerUserTableComponent },
  { path: 'users/:userID', component: SellerUserTableComponent },
  { path: 'locations', component: SellerLocationTableComponent },
  { path: 'locations/new', component: SellerLocationTableComponent },
  { path: 'locations/:locationID', component: SellerLocationTableComponent },
]

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class SellerUsersRoutingModule {}
