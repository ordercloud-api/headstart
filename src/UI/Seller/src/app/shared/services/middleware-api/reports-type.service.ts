import { Injectable } from '@angular/core'
import { Suppliers } from 'ordercloud-javascript-sdk'
import { ResourceCrudService } from '../resource-crud/resource-crud.service'
import { Router, ActivatedRoute } from '@angular/router'
import { CurrentUserService } from '../current-user/current-user.service'
import {
  HeadStartSDK,
  HSBuyer,
  ListPage,
  ReportTypeResource,
} from '@ordercloud/headstart-sdk'

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
    currentUserService: CurrentUserService
  ) {
    super(
      router,
      activatedRoute,
      Suppliers,
      currentUserService,
      '/reports',
      'reports',
      REPORTS_SUB_RESOURCE_LIST
    )
  }

  async list(): Promise<ListPage<ReportTypeResource>> {
    return await HeadStartSDK.Reports.FetchAllReportTypes()
  }

  async getBuyerFilterValues(): Promise<HSBuyer[]> {
    return await HeadStartSDK.Reports.GetBuyerFilterValues()
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
