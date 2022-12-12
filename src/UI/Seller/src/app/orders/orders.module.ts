import { NgModule } from '@angular/core'
import { SharedModule } from '@app-seller/shared'
import { OrdersRoutingModule } from './orders-routing.module'
import { OrderTableComponent } from './components/order-table/order-table.component'
import { OrderDetailsComponent } from './components/order-details/order-details.component'
import { OrderShipmentsComponent } from './components/order-shipments/order-shipments.component'
import { LineItemTableComponent } from './components/line-item-table/line-item-table.component'
import { UploadShipmentsComponent } from './components/upload-shipments/upload-shipments.component'
import { PerfectScrollbarModule } from 'ngx-perfect-scrollbar'
import { NgxSpinnerModule } from 'ngx-spinner'
import { OrderReturnRequestTable } from './components/order-return-create-modal/order-return-request-table/order-return-request-table.component'
import { OrderReturnCreateModal } from './components/order-return-create-modal/order-return-create-modal.component'
import { NgbPopoverModule } from '@ng-bootstrap/ng-bootstrap'

@NgModule({
  imports: [
    SharedModule,
    OrdersRoutingModule,
    PerfectScrollbarModule,
    NgxSpinnerModule,
    NgbPopoverModule,
  ],
  declarations: [
    OrderTableComponent,
    OrderDetailsComponent,
    OrderShipmentsComponent,
    LineItemTableComponent,
    UploadShipmentsComponent,
    OrderReturnCreateModal,
    OrderReturnRequestTable,
  ],
})
export class OrdersModule {}
