import { NgModule } from '@angular/core'
import { SharedModule } from '@app-seller/shared'
import { PerfectScrollbarModule } from 'ngx-perfect-scrollbar'
import {
  NgbDatepickerModule,
  NgbPopoverModule,
} from '@ng-bootstrap/ng-bootstrap'
import { NgbModule } from '@ng-bootstrap/ng-bootstrap'
import { OrderReturnTableComponent } from './components/order-return-table/order-return-table.component'
import { OrderReturnEditComponent } from './components/order-return-edit/order-return-edit.component'
import { OrderReturnSummaryComponent } from './components/order-return-summary/order-return-summary.component'
import { OrderReturnRoutingModule } from './order-returns-routing.module'
import { OrderReturnLogsComponent } from './components/order-return-logs/order-return-logs.component'
import { OrderReturnStatusComponent } from './components/order-return-status/order-return-status.component'
import { OrderReturnOverviewComponent } from './components/order-return-overview/order-return-overview.component'
import { OrderReturnLineItemDetailComponent } from './components/order-return-lineitem-detail/order-return-lineitem-detail.component'
import { OrderReturnCommentsComponent } from './components/order-return-comments/order-return-comments.component';
import { OrderReturnRefundInputComponent } from './components/order-return-refund-input/order-return-refund-input.component';
@NgModule({
  imports: [
    SharedModule,
    OrderReturnRoutingModule,
    PerfectScrollbarModule,
    NgbDatepickerModule,
    NgbPopoverModule,
    NgbModule,
  ],
  declarations: [
    OrderReturnTableComponent,
    OrderReturnEditComponent,
    OrderReturnSummaryComponent,
    OrderReturnLogsComponent,
    OrderReturnStatusComponent,
    OrderReturnOverviewComponent,
    OrderReturnLineItemDetailComponent,
    OrderReturnCommentsComponent,
    OrderReturnRefundInputComponent,
  ],
})
export class OrderReturnModule {}
