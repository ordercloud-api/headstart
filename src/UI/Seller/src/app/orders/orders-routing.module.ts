// core services
import { NgModule } from '@angular/core'
import { RouterModule, Routes } from '@angular/router'
import { OrderTableComponent } from './components/order-table/order-table.component'
import { UploadShipmentsComponent } from './components/upload-shipments/upload-shipments.component'

const routes: Routes = [
  { path: '', component: OrderTableComponent },
  { path: 'uploadshipments', component: UploadShipmentsComponent },
  { path: ':orderID', component: OrderTableComponent },
  // { path: ':orderID/shipments', component: OrderTableComponent },
]

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class OrdersRoutingModule {}
