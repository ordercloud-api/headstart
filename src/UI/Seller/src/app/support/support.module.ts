import { NgModule } from '@angular/core'

import { SupportRoutingModule } from './support-routing.module'
import { CaseSubmissionComponent } from './case-submission/case-submission.component'
import { SharedModule } from '@app-seller/shared'

@NgModule({
  declarations: [CaseSubmissionComponent],
  imports: [
    SharedModule,
    SupportRoutingModule,
  ]
})
export class SupportModule { }
