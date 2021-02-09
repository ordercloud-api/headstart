import { NgModule } from '@angular/core'
import { SharedModule } from '@app-seller/shared'

import { SellerUserTableComponent } from './components/seller-user-table/seller-user-table.component'
import { SellerUsersRoutingModule } from './seller-users-routing.module'
import { SellerUserEditComponent } from './components/seller-user-edit/seller-user-edit.component'

@NgModule({
  imports: [SharedModule, SellerUsersRoutingModule],
  declarations: [SellerUserTableComponent, SellerUserEditComponent],
})
export class SellerUsersModule {}
