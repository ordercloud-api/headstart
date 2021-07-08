import { NgModule } from '@angular/core'
import { SharedModule } from '@app-seller/shared'
import { PerfectScrollbarModule } from 'ngx-perfect-scrollbar'
import {
  NgbDatepickerModule,
  NgbPopoverModule,
} from '@ng-bootstrap/ng-bootstrap'
import { NgbModule } from '@ng-bootstrap/ng-bootstrap'
import { RMATableComponent } from './components/rmas-table/rmas-table.component'
import { RMAEditComponent } from './components/rmas-edit/rmas-edit.component'
import { RMASummaryComponent } from './components/rmas-summary/rmas-summary.component'
import { RMARoutingModule } from './rmas-routing.module'
import { RMALogsComponent } from './components/rmas-logs/rmas-logs.component'
import { RMAStatusComponent } from './components/rmas-status/rmas-status.component'
import { RMAFormProcessingComponent } from './components/rmas-form-processing/rmas-form-processing.component'
import { RMAOverviewComponent } from './components/rmas-overview/rmas-overview.component'
import { RMALineItemDetailComponent } from './components/rmas-line-item-detail/rmas-line-item-detail.component'
import { RMAModalContent } from './components/rmas-modal-content/rmas-modal-content.component'
@NgModule({
  imports: [
    SharedModule,
    RMARoutingModule,
    PerfectScrollbarModule,
    NgbDatepickerModule,
    NgbPopoverModule,
    NgbModule,
  ],
  declarations: [
    RMATableComponent,
    RMAEditComponent,
    RMASummaryComponent,
    RMAFormProcessingComponent,
    RMALogsComponent,
    RMAStatusComponent,
    RMAOverviewComponent,
    RMALineItemDetailComponent,
    RMAModalContent,
  ],
})
export class RMAModule {}
