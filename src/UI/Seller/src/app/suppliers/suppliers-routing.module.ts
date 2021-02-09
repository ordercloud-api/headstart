// core services
import { NgModule } from '@angular/core'
import { RouterModule, Routes } from '@angular/router'
import { SupplierTableComponent } from './components/suppliers/supplier-table/supplier-table.component'
import { SupplierLocationTableComponent } from './components/locations/supplier-location-table/supplier-location-table.component'
import { SupplierUserTableComponent } from './components/users/supplier-user-table/supplier-user-table.component'

const routes: Routes = [
  { path: '', component: SupplierTableComponent },
  { path: 'new', component: SupplierTableComponent },
  { path: 'locations', component: SupplierLocationTableComponent },
  { path: 'users', component: SupplierUserTableComponent },
  { path: 'locations/new', component: SupplierLocationTableComponent },
  { path: 'users/new', component: SupplierUserTableComponent },
  { path: 'users/:userID', component: SupplierUserTableComponent },
  { path: 'locations/:locationID', component: SupplierLocationTableComponent },
  { path: ':supplierID', component: SupplierTableComponent },
  { path: ':supplierID/users', component: SupplierUserTableComponent },
  { path: ':supplierID/users/new', component: SupplierUserTableComponent },
  { path: ':supplierID/users/:userID', component: SupplierUserTableComponent },
  { path: ':supplierID/locations', component: SupplierLocationTableComponent },
  {
    path: ':supplierID/locations/new',
    component: SupplierLocationTableComponent,
  },
  {
    path: ':supplierID/locations/:locationID',
    component: SupplierLocationTableComponent,
  },
]

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class SuppliersRoutingModule {}
