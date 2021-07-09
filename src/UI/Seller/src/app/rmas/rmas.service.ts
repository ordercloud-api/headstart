import { Inject, Injectable } from '@angular/core'
import { Router, ActivatedRoute } from '@angular/router'
import { ResourceCrudService } from '@app-seller/shared/services/resource-crud/resource-crud.service'
import { CurrentUserService } from '@app-seller/shared/services/current-user/current-user.service'
import { CosmosListOptions, HeadStartSDK, RMA } from '@ordercloud/headstart-sdk'
import { CosmosListPage } from '@ordercloud/headstart-sdk/dist/models/CosmosListPage'
import { AppConfig, Options } from '@app-seller/shared'
import { applicationConfiguration } from '@app-seller/config/app.config'
import { HttpClient, HttpHeaders } from '@angular/common/http'
import { OcTokenService } from '@ordercloud/angular-sdk'

@Injectable({
  providedIn: 'root',
})
export class RMAService extends ResourceCrudService<RMA> {
  constructor(
    router: Router,
    activatedRoute: ActivatedRoute,
    currentUserService: CurrentUserService,
    private http: HttpClient,
    private ocTokenService: OcTokenService,
    @Inject(applicationConfiguration) private appConfig: AppConfig
  ) {
    super(
      router,

      activatedRoute,
      HeadStartSDK.RmAs,
      currentUserService,
      '/rmas',
      'rmas'
    )
  }

  private buildHeaders(): HttpHeaders {
    return new HttpHeaders({
      'Content-Type': 'application/json',
      Authorization: `Bearer ${this.ocTokenService.GetAccess()}`,
    })
  }

  public async updateResource(originalID: string, resource: RMA): Promise<RMA> {
    originalID = resource.RMANumber
    const processedRMA = await HeadStartSDK.RmAs.ProcessRMA(resource)
    this.updateResourceSubject(processedRMA)
    return processedRMA
  }

  public async processRefund(rmaNumber: string): Promise<RMA> {
    const rmaAfterRefund = await HeadStartSDK.RmAs.ProcessRefund(rmaNumber)
    this.updateResourceSubject(rmaAfterRefund)
    return rmaAfterRefund
  }

  public async listRMAsByOrderID(
    orderID: string
  ): Promise<CosmosListPage<RMA>> {
    const rmasByOrderID = await HeadStartSDK.RmAs.ListRMAsByOrderID(orderID)
    return rmasByOrderID
  }

  public async list(args: any[]): Promise<CosmosListPage<RMA>> {
    let cosmosFilterOptions: CosmosListOptions = { Filters: null }
    if (args?.length && args[0].Filters != undefined) {
      cosmosFilterOptions = this.updateCosmosFilter(args)
    } else {
      cosmosFilterOptions = this.updateCosmosFilter([
        this.optionsSubject?.getValue().filters,
      ])
    }
    let tokenArg = null
    if (args?.length && !args[0].Filters?.length) {
      tokenArg = args?.find((arg) => arg?.ContinuationToken)
    }
    const cosmosListOptions: any = {
      PageSize: 100,
      ContinuationToken: tokenArg?.ContinuationToken,
      Filters: cosmosFilterOptions.Filters,
      Sort: 'DateCreated',
      SortDirection: 'DESC',
      Search: args[0].search,
      SearchOn: 'RMANumber',
    }
    const url = `${this.appConfig.middlewareUrl}/rma/list`
    const listResponse = await this.http
      .post<CosmosListPage<RMA>>(url, cosmosListOptions, {
        headers: this.buildHeaders(),
      })
      .toPromise()
    if (cosmosListOptions.ContinuationToken) {
      this.addResources(listResponse)
    }
    return listResponse
  }

  updateCosmosFilter(args: any[]): CosmosListOptions {
    const activeOptions = { ...this.optionsSubject.value, ...args }
    const queryParams = this.mapToUrlQueryParams(activeOptions)
    const cosmosFilterOptions = {
      Filters: this.buildCosmosFilterOptions(activeOptions),
    }
    return cosmosFilterOptions
  }

  buildCosmosFilterOptions(options: Options): any {
    const cosmosFilters = []
    if (options?.filters == null) {
      return
    }
    const filters = Object.entries(options?.filters)

    filters.forEach((filter) => {
      const cosmosFilterName = this.getCosmosFilterName(filter[0])
      const cosmosFilterTerm = this.getCosmosFilterTerm(filter)
      const cosmosOperator = this.getCosmosOperator(filter[0])
      const filterExpression = cosmosOperator + cosmosFilterTerm
      if (
        !cosmosFilters.some(
          (cosmosFilter) =>
            cosmosFilter?.PropertyName === cosmosFilterName &&
            cosmosFilter?.FilterExpression === filterExpression
        )
      ) {
        cosmosFilters.push({
          PropertyName: cosmosFilterName,
          FilterExpression: filterExpression,
        })
      }
    })
    return cosmosFilters
  }

  getCosmosFilterTerm(filter: [string, any]): string {
    if (filter[0] === 'to') {
      const date = new Date(filter[1]).toISOString()
      const formattedDate = date.split('T')
      const dateToUse = formattedDate[0] + 'T23:59:59.999Z' // End of day
      return dateToUse
    }
    return filter[1]
  }

  getCosmosFilterName(filterName: string): string {
    if (filterName === 'from' || filterName === 'to') {
      return 'DateCreated'
    }
    return filterName
  }

  getCosmosOperator(filterName: string): string {
    switch (filterName) {
      case 'from':
        return '>='
      case 'to':
        return '<='
      default:
        return '='
    }
  }

  public getParentOrSecondaryIDParamName(): string {
    return 'RMANumber'
  }

  public getResourceID(resource: RMA): string {
    return resource.RMANumber
  }

  public checkForResourceMatch(i: RMA, resourceID: string): boolean {
    return i.RMANumber === resourceID
  }

  public checkForNewResourceMatch(i: RMA, newResource: RMA): boolean {
    return i.RMANumber === newResource.RMANumber
  }
}
