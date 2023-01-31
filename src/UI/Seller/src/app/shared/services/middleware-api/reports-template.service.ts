import { Inject, Injectable } from '@angular/core'
import { applicationConfiguration } from '@app-seller/config/app.config'
import { ResourceCrudService } from '../resource-crud/resource-crud.service'
import { Router, ActivatedRoute } from '@angular/router'
import { CurrentUserService } from '../current-user/current-user.service'
import {
  HeadStartSDK,
  ListArgs,
  ListPage,
  ReportTemplate,
} from '@ordercloud/headstart-sdk'
import { AppConfig } from '@app-seller/models/environment.types'

@Injectable({
  providedIn: 'root',
})
export class ReportsTemplateService extends ResourceCrudService<ReportTemplate> {
  emptyResource = {
    TemplateID: '',
    Name: '',
    Description: '',
    AvailableToSuppliers: false,
    Headers: [],
    Filters: {
      BuyerID: [],
      State: [],
      Country: [],
    },
  }
  args: ListArgs

  constructor(
    router: Router,
    activatedRoute: ActivatedRoute,
    currentUserService: CurrentUserService,
    @Inject(applicationConfiguration) private appConfig: AppConfig
  ) {
    super(
      router,
      activatedRoute,
      null,
      currentUserService,
      '/reports',
      'reports',
      null,
      'templates'
    )
    this.ocService = this
  }

  async getResourceById(resourceID: string): Promise<ReportTemplate> {
    return await HeadStartSDK.Reports.GetReportTemplate(resourceID)
  }

  async list(args: any[]): Promise<ListPage<ReportTemplate>> {
    const list = await this.listReportTemplatesByReportType(
      this.router.url.split('/')[2]
    )
    const listPage = {
      Meta: {
        Page: 1,
        PageSize: 100,
        TotalCount: list.length,
        TotalPages: 1,
      },
      Items: list,
    }
    return listPage
  }

  async deleteResource(templateID: string): Promise<void> {
    return await HeadStartSDK.Reports.DeleteReportTemplate(templateID)
  }

  async createNewResource(template: ReportTemplate): Promise<ReportTemplate> {
    const routeUrl = this.router.routerState.snapshot.url.split('/')
    const reportType = routeUrl[2] as
      | 'BuyerLocation'
      | 'SalesOrderDetail'
      | 'PurchaseOrderDetail'
      | 'LineItemDetail'
    return await HeadStartSDK.Reports.PostReportTemplate(reportType, template)
  }

  async updateResource(
    originalID: string,
    resource: any
  ): Promise<ReportTemplate> {
    originalID = resource.TemplateID
    return await HeadStartSDK.Reports.UpdateReportTemplate(originalID, resource)
  }

  async listReportTemplatesByReportType(
    reportType: string
  ): Promise<ReportTemplate[]> {
    return await HeadStartSDK.Reports.ListReportTemplatesByReportType(
      reportType as
        | 'BuyerLocation'
        | 'SalesOrderDetail'
        | 'PurchaseOrderDetail'
        | 'LineItemDetail'
    )
  }

  async previewReport(
    template: any,
    reportRequestBody: any
  ): Promise<object[]> {
    return await HeadStartSDK.Reports.PreviewReport(
      template.ReportType,
      template.TemplateID
    )
  }

  async downloadReport(template: any, reportRequestBody: any): Promise<void> {
    const fileName = await HeadStartSDK.Reports.DownloadReport(
      template.ReportType,
      template.TemplateID
    )
    const sharedAccessSignature =
      await HeadStartSDK.Reports.GetSharedAccessSignature(fileName)
    const uri = `${this.appConfig.blobStorageUrl}/downloads/${fileName}${sharedAccessSignature}`
    const link = document.createElement('a')
    link.download = fileName
    link.href = uri
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
  }

  public getParentOrSecondaryIDParamName(): string {
    return 'TemplateID'
  }

  public getResourceID(resource: any): string {
    return resource.TemplateID
  }

  public checkForResourceMatch(i: any, resourceID: string): boolean {
    return i.TemplateID === resourceID
  }

  public checkForNewResourceMatch(i: any, newResource: any): boolean {
    return i.TemplateID === newResource.TemplateID
  }
}
