// core services
import { NgModule } from '@angular/core'
import { RouterModule, Routes } from '@angular/router'
import { ReportsTableComponent } from './components/reports-main/reports-table/reports-table.component'
import { TemplateTableComponent } from './components/reports-template/template-table/template-table.component'

const routes: Routes = [
  { path: 'reports', component: ReportsTableComponent },
  { path: ':ReportType/templates', component: TemplateTableComponent },
  {
    path: ':ReportType/templates/:TemplateID',
    component: TemplateTableComponent,
  },
  { path: ':ReportType/templates/new', component: TemplateTableComponent },
]

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ReportsRoutingModule {}
