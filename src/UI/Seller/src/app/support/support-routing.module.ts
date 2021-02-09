// core services
import { NgModule } from '@angular/core'
import { RouterModule, Routes } from '@angular/router'

import { CaseSubmissionComponent } from './case-submission/case-submission.component'

const routes: Routes = [
  { path: '', component: CaseSubmissionComponent },
]

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class SupportRoutingModule {}
