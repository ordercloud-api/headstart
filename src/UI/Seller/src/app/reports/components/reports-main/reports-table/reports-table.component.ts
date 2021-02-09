import { Component, ChangeDetectorRef, NgZone } from '@angular/core'
import { ResourceCrudComponent } from '@app-seller/shared/components/resource-crud/resource-crud.component'
import { ReportsTemplateService } from '@app-seller/shared/services/middleware-api/reports-template.service'
import { Router, ActivatedRoute } from '@angular/router'
import { ReportTemplate } from '@ordercloud/headstart-sdk'

@Component({
  selector: 'app-reports-table',
  templateUrl: './reports-table.component.html',
  styleUrls: ['./reports-table.component.scss'],
})
export class ReportsTableComponent extends ResourceCrudComponent<ReportTemplate> {
  constructor(
    private reportsTemplateService: ReportsTemplateService,
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
      ngZone
    )
    this.router = router
  }
}
