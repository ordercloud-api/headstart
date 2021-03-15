import { NgModule } from '@angular/core'
import { SharedModule } from '@app-seller/shared'
import { ReportsRoutingModule } from './reports-routing.module'
import { ReportsTableComponent } from './components/reports-main/reports-table/reports-table.component'
import { ReportsComponent } from './components/reports-main/reports/reports.component'
import { ReportsSelectionComponent } from './components/reports-main/reports-selection/reports-selection.component'
import { ReportsProcessingComponent } from './components/reports-main/reports-processing/reports-processing.component'
import { ReportsPreviewComponent } from './components/reports-main/reports-preview/reports-preview.component'
import { TemplateTableComponent } from './components/reports-template/template-table/template-table.component'
import { TemplateEditComponent } from './components/reports-template/template-edit/template-edit.component'

@NgModule({
  imports: [SharedModule, ReportsRoutingModule],
  declarations: [
    ReportsTableComponent,
    ReportsComponent,
    ReportsSelectionComponent,
    ReportsProcessingComponent,
    ReportsPreviewComponent,
    TemplateTableComponent,
    TemplateEditComponent,
  ],
})
export class ReportsModule {}
