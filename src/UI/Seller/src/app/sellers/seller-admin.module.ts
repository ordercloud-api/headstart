import { NgModule } from '@angular/core'
import { SharedModule } from '@app-seller/shared'

import { SellerUserTableComponent } from './components/users/seller-user-table/seller-user-table.component'
import { SellerUsersRoutingModule } from './seller-admin-routing.module'
import { SellerUserEditComponent } from './components/users/seller-user-edit/seller-user-edit.component'
import { SellerLocationEditComponent } from './components/locations/seller-location-edit/seller-location-edit.component'
import { SellerLocationTableComponent } from './components/locations/seller-location-table/seller-location-table.component'

@NgModule({
  imports: [SharedModule, SellerUsersRoutingModule],
  declarations: [
    SellerUserTableComponent,
    SellerUserEditComponent,
    SellerLocationEditComponent,
    SellerLocationTableComponent,
  ],
})
export class SellerUsersModule {}
