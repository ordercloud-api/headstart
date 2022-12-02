import { NgModule } from '@angular/core'
import { RouterModule, Routes } from '@angular/router'
import { OrderReturnTableComponent } from './components/order-return-table/order-return-table.component'

const routes: Routes = [
  { path: '', component: OrderReturnTableComponent },
  { path: 'new', component: OrderReturnTableComponent, pathMatch: 'full' },
  {
    path: ':orderreturnID',
    component: OrderReturnTableComponent,
    pathMatch: 'full',
  },
]
@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class OrderReturnRoutingModule {}
