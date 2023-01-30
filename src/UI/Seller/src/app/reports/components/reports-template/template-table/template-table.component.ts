import { Component, ChangeDetectorRef, NgZone } from '@angular/core'
import { ResourceCrudComponent } from '@app-seller/shared/components/resource-crud/resource-crud.component'
import { ReportsTemplateService } from '@app-seller/shared/services/middleware-api/reports-template.service'
import { Router, ActivatedRoute } from '@angular/router'
import { ReportsTypeService } from '@app-seller/shared/services/middleware-api/reports-type.service'
import { UntypedFormGroup, UntypedFormControl, Validators } from '@angular/forms'
import { ValidateRichTextDescription } from '@app-seller/validators/validators'
import { ReportTemplate } from '@ordercloud/headstart-sdk'

function createTemplateForm(template: ReportTemplate): UntypedFormGroup {
  const resourceForm = new UntypedFormGroup({
    ReportType: new UntypedFormControl(template?.ReportType),
    Name: new UntypedFormControl(template?.Name, Validators.required),
    Description: new UntypedFormControl(
      template?.Description,
      ValidateRichTextDescription
    ),
    AvailableToSuppliers: new UntypedFormControl(template?.AvailableToSuppliers),
    Headers: new UntypedFormControl(template?.Headers || [], Validators.required),
    Filters: new UntypedFormControl(template?.Filters || {}),
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
