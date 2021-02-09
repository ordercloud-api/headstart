import { NgModule } from '@angular/core'
import { SharedModule } from '@app-seller/shared'

import { PerfectScrollbarModule } from 'ngx-perfect-scrollbar'
import { PromotionsRoutingModule } from './promotions-routing.module'
import { PromotionTableComponent } from './components/promotion-table/promotion-table.component'
import { PromotionEditComponent } from './components/promotion-edit/promotion-edit.component'
import {
  NgbDatepickerModule,
  NgbPopoverModule,
} from '@ng-bootstrap/ng-bootstrap'

@NgModule({
  imports: [
    SharedModule,
    PromotionsRoutingModule,
    PerfectScrollbarModule,
    NgbDatepickerModule,
    NgbPopoverModule,
  ],
  declarations: [PromotionTableComponent, PromotionEditComponent],
})
export class PromotionsModule {}
