import { NgModule } from '@angular/core'
import { SharedModule } from '@app-seller/shared'
import { SuppliersRoutingModule } from './suppliers-routing.module'
import { SupplierTableComponent } from './components/suppliers/supplier-table/supplier-table.component'
import { PerfectScrollbarModule } from 'ngx-perfect-scrollbar'
import { SupplierLocationTableComponent } from './components/locations/supplier-location-table/supplier-location-table.component'
import { SupplierLocationEditComponent } from './components/locations/supplier-location-edit/supplier-location-edit.component'
import { SupplierEditComponent } from './components/suppliers/supplier-edit/supplier-edit.component'
import { SupplierUserTableComponent } from './components/users/supplier-user-table/supplier-user-table.component'
import { SupplierUserEditComponent } from './components/users/supplier-user-edit/supplier-user-edit.component'
import { RouterModule } from '@angular/router'

@NgModule({
  imports: [SharedModule, SuppliersRoutingModule, PerfectScrollbarModule, RouterModule],
  declarations: [
    SupplierLocationTableComponent,
    SupplierLocationEditComponent,
    SupplierTableComponent,
    SupplierUserTableComponent,
    SupplierEditComponent,
    SupplierUserEditComponent,
  ],
})
export class SuppliersModule { }
