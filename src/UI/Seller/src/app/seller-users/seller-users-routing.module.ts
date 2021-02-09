// core services
import { NgModule } from '@angular/core'
import { RouterModule, Routes } from '@angular/router'
import { SellerUserTableComponent } from './components/seller-user-table/seller-user-table.component'

const routes: Routes = [
  { path: '', component: SellerUserTableComponent },
  { path: 'new', component: SellerUserTableComponent },
  { path: ':userID', component: SellerUserTableComponent },
]

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class SellerUsersRoutingModule {}
