import { HttpClient, HttpHeaders } from '@angular/common/http'
import { Inject, Injectable } from '@angular/core'
import { applicationConfiguration } from '@app-seller/config/app.config'
import { OcTokenService, OcSupplierService } from '@ordercloud/angular-sdk'
import { ResourceCrudService } from '../resource-crud/resource-crud.service'
import { Router, ActivatedRoute } from '@angular/router'
import { CurrentUserService } from '../current-user/current-user.service'
import {
  HSBuyer,
  ListPage,
  ReportTypeResource,
} from '@ordercloud/headstart-sdk'
import { AppConfig } from '@app-seller/models/environment.types'

export const REPORTS_SUB_RESOURCE_LIST = [
  { route: 'reports', display: 'Reports' },
  { route: 'templates', display: 'Templates' },
]

@Injectable({
  providedIn: 'root',
})
export class ReportsTypeService extends ResourceCrudService<ReportTypeResource> {
  constructor(
    router: Router,
    activatedRoute: ActivatedRoute,
    service: OcSupplierService,
    currentUserService: CurrentUserService,
    private ocTokenService: OcTokenService,
    private http: HttpClient,
    @Inject(applicationConfiguration) private appConfig: AppConfig
  ) {
    super(
      router,
      activatedRoute,
      service,
      currentUserService,
      '/reports',
      'reports',
      REPORTS_SUB_RESOURCE_LIST
    )
  }

  private buildHeaders(): HttpHeaders {
    return new HttpHeaders({
      'Content-Type': 'application/json',
      Authorization: `Bearer ${this.ocTokenService.GetAccess()}`,
    })
  }

  async list(): Promise<ListPage<ReportTypeResource>> {
    const url = `${this.appConfig.middlewareUrl}/reports/fetchAllReportTypes`
    return await this.http
      .get<ListPage<ReportTypeResource>>(url, { headers: this.buildHeaders() })
      .toPromise()
  }

  async getBuyerFilterValues(): Promise<HSBuyer[]> {
    const url = `${this.appConfig.middlewareUrl}/reports/filters/buyers`
    return await this.http
      .get<HSBuyer[]>(url, { headers: this.buildHeaders() })
      .toPromise()
  }

  public getParentIDParamName(): string {
    return 'ReportType'
  }

  async getResourceById(resourceID: string): Promise<any> {
    await this.listResources()
    return this.resourceSubject.value.Items.find(
      (r) => r.ID.toString() === resourceID
    )
  }
}
