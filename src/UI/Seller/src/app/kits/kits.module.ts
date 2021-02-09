import { NgModule } from '@angular/core'
import { SharedModule } from '@app-seller/shared'

import { PerfectScrollbarModule } from 'ngx-perfect-scrollbar'
import { KitsRoutingModule } from './kits-routing.module'
import { KitsTableComponent } from './components/kits-table/kits-table.component'
import { KitsEditComponent } from './components/kits-edit/kits-edit.component'
import {
  NgbDatepickerModule,
  NgbPopoverModule,
} from '@ng-bootstrap/ng-bootstrap'
import { NgbModule } from '@ng-bootstrap/ng-bootstrap'
@NgModule({
  imports: [
    SharedModule,
    KitsRoutingModule,
    PerfectScrollbarModule,
    NgbDatepickerModule,
    NgbPopoverModule,
    NgbModule,
  ],
  declarations: [KitsTableComponent, KitsEditComponent],
})
export class KitsModule {}
