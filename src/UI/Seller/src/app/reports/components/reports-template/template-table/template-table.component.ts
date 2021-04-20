import { Component, ChangeDetectorRef, NgZone } from '@angular/core'
import { ResourceCrudComponent } from '@app-seller/shared/components/resource-crud/resource-crud.component'
import { ReportsTemplateService } from '@app-seller/shared/services/middleware-api/reports-template.service'
import { Router, ActivatedRoute } from '@angular/router'
import { ReportsTypeService } from '@app-seller/shared/services/middleware-api/reports-type.service'
import { FormGroup, FormControl, Validators } from '@angular/forms'
import { ValidateRichTextDescription } from '@app-seller/validators/validators'
import { ReportTemplate } from '@ordercloud/headstart-sdk'

function createTemplateForm(template: ReportTemplate): FormGroup {
  const resourceForm = new FormGroup({
    ReportType: new FormControl(template?.ReportType),
    Name: new FormControl(template?.Name, Validators.required),
    Description: new FormControl(
      template?.Description,
      ValidateRichTextDescription
    ),
    AvailableToSuppliers: new FormControl(template?.AvailableToSuppliers),
    Headers: new FormControl(template?.Headers || [], Validators.required),
    Filters: new FormControl(template?.Filters || {}),
  })
  return resourceForm
}

@Component({
  selector: 'app-template-table',
  templateUrl: './template-table.component.html',
  styleUrls: ['./template-table.component.scss'],
})
export class TemplateTableComponent extends ResourceCrudComponent<ReportTemplate> {
  reportType: string

  constructor(
    private reportsTemplateService: ReportsTemplateService,
    private reportsTypeService: ReportsTypeService,
    changeDetectorRef: ChangeDetectorRef,
    router: Router,
    activatedRoute: ActivatedRoute,
    ngZone: NgZone
  ) {
    super(
      changeDetectorRef,
      reportsTemplateService,
      router,
      activatedRoute,
      ngZone,
      createTemplateForm
    )
    this.router = router
    const routeUrl = this.router.routerState.snapshot.url.split('/')
    this.reportType = routeUrl[2]
  }
}
