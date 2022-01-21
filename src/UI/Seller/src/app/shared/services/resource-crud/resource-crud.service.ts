import { BehaviorSubject } from 'rxjs'
import { Router, Params, ActivatedRoute } from '@angular/router'
import { REDIRECT_TO_FIRST_PARENT } from '@app-seller/layout/header/header.config'
import {
  Options,
  RequestStatus,
  SUCCESSFUL_WITH_ITEMS,
  ERROR,
  GETTING_NEW_ITEMS,
  FETCHING_SUBSEQUENT_PAGES,
  REFRESHING_ITEMS,
  SUCCESSFUL_NO_ITEMS_WITH_FILTERS,
  SUCCESSFUL_NO_ITEMS_NO_FILTERS,
} from '../../../models/resource-crud.types'
import { set as _set } from 'lodash'
import { CurrentUserService } from '../current-user/current-user.service'
import { BuyerAddress, ListPage } from 'ordercloud-javascript-sdk'
import { singular } from 'pluralize'
import { ResourceUpdate } from '@app-seller/models/shared.types'
import { ListArgs } from '@ordercloud/headstart-sdk'

export abstract class ResourceCrudService<ResourceType> {
  public resourceSubject: BehaviorSubject<ListPage<ResourceType>> =
    new BehaviorSubject<ListPage<ResourceType>>({
      Meta: {},
      Items: [],
    })
  public resourceRequestStatus: BehaviorSubject<RequestStatus> =
    new BehaviorSubject<RequestStatus>(GETTING_NEW_ITEMS)
  public optionsSubject: BehaviorSubject<Options> =
    new BehaviorSubject<Options>({})

  route = ''
  myRoute = ''
  primaryResourceLevel = ''
  // example: for supplier user service the primary is supplier and the secondary is users
  secondaryResourceLevel = ''
  subResourceList: any[]
  emptyResource: any = {}

  private itemsPerPage = 100

  constructor(
    protected router: Router,
    private activatedRoute: ActivatedRoute,
    public ocService: any,
    public currentUserService: CurrentUserService,
    route: string,
    primaryResourceLevel: string,
    subResourceList: any[] = [],
    secondaryResourceLevel = '',
    myRoute = ''
  ) {
    this.route = route
    this.myRoute = myRoute
    this.primaryResourceLevel = primaryResourceLevel
    this.secondaryResourceLevel = secondaryResourceLevel
    this.subResourceList = subResourceList

    this.activatedRoute.queryParams.subscribe((params) => {
      // this prevents service from reading from query params when not on the route related to the service
      if (this.isOnRelatedRoute()) {
        this.readFromUrlQueryParams(params)
      } else {
        this.optionsSubject.next({})
      }
    })
    this.optionsSubject.subscribe(async (options: Options) => {
      const parentResourceID = await this.getParentResourceID()
      if (parentResourceID !== REDIRECT_TO_FIRST_PARENT) {
        this.listResources()
      }
    })
  }

  // Can Override
  async list(args: any[]): Promise<ListPage<ResourceType>> {
    return await this.ocService.List(...args)
  }

  public async getMyResource(): Promise<any> {
    if (this.primaryResourceLevel === 'suppliers') {
      return this.currentUserService.getMySupplier()
    }
  }

  async listResources(pageNumber = 1, searchText = ''): Promise<void> {
    const shouldList = await this.shouldListResources()
    if (shouldList) {
      const { sortBy, search, searchType, filters, OrderDirection } =
        this.optionsSubject.value
      let options: Options = {
        page: pageNumber,
        // allows a list call to pass in a search term that will not appear in the query params
        search: searchText || search,
        sortBy,
        pageSize: this.itemsPerPage,
        filters,
        searchType,
      }
      options = this.addIntrinsicListArgs(options)
      const resourceResponse = await this.listWithStatusIndicator(
        options,
        OrderDirection
      )
      if (pageNumber === 1) {
        this.setNewResources(resourceResponse)
      } else {
        this.addResources(resourceResponse)
      }
    }
  }

  addIntrinsicListArgs(options: ListArgs): ListArgs {
    return options
  }

  getFetchStatus(options: Options): RequestStatus {
    const isSubsequentPage = options.page > 1
    const areCurrentlyItems = this.resourceSubject.value.Items.length
    // will not want to show a loading indicator in certain situations so this
    // differentiates between refreshes and new lists
    // when filters are applied REFRESHING_ITEMS will be returned
    if (!areCurrentlyItems && !isSubsequentPage) {
      return GETTING_NEW_ITEMS
    }
    if (!isSubsequentPage && areCurrentlyItems) {
      return REFRESHING_ITEMS
    }
    if (isSubsequentPage && areCurrentlyItems) {
      return FETCHING_SUBSEQUENT_PAGES
    }
    // return isSubsequentPage || !areCurrentlyItems ? GETTING_NEW_ITEMS : REFRESHING_ITEMS;
  }

  getSucessStatus(
    options: Options,
    resourceResponse: ListPage<ResourceType>
  ): RequestStatus {
    const areFilters = this.areFiltersOnOptions(options)
    const areItems = !!resourceResponse.Items.length
    if (areItems) return SUCCESSFUL_WITH_ITEMS
    return areFilters
      ? SUCCESSFUL_NO_ITEMS_WITH_FILTERS
      : SUCCESSFUL_NO_ITEMS_NO_FILTERS
  }

  async shouldListResources(): Promise<boolean> {
    if (!this.secondaryResourceLevel) {
      // for primary resources list if on the route
      return this.router.url.startsWith(this.route)
    } else {
      const parentResourceID = await this.getParentResourceID()
      // for secondary resources list there is a parent ID
      return (
        !!parentResourceID &&
        this.router.url.includes(this.secondaryResourceLevel)
      )
    }
  }

  async isSupplierUser(): Promise<boolean> {
    return await this.currentUserService.isSupplierUser()
  }

  async constructResourceURLs(resourceID = ''): Promise<string[]> {
    const newUrlPieces = []
    newUrlPieces.push(this.route)
    if (this.secondaryResourceLevel) {
      const parentResourceID = await this.getParentResourceID()
      newUrlPieces.push(`/${parentResourceID}`)
      newUrlPieces.push(`/${this.secondaryResourceLevel}`)
    }
    if (resourceID) {
      newUrlPieces.push(`/${resourceID}`)
    }
    return newUrlPieces
  }

  selectParentResource(resource: any): void {
    const newUrl = this.updateUrlForUpdatedParent(resource)
    this.router.navigateByUrl(newUrl)

    // this settimeout ensures that the new parent resource ID is in the url before the resources are listed
    // find a better way to update the resources on the parent resource ID change
    setTimeout(() => {
      this.listResources()
    })
  }

  updateUrlForUpdatedParent(resource: any): string {
    const queryParams = this.router.url.split('?')[1]
    let newUrl = `${this.primaryResourceLevel}/${resource.ID}/${this.secondaryResourceLevel}`
    if (queryParams) {
      newUrl += `?${queryParams}`
    }
    return newUrl
  }

  async constructNewRouteInformation(resourceID = ''): Promise<any[]> {
    let newUrl = ''
    const queryParams = this.activatedRoute.snapshot.queryParams
    if (this.secondaryResourceLevel) {
      const parentResourceID = await this.getParentResourceID()
      newUrl += this.router.url.startsWith('/my-')
        ? `${this.myRoute}/${this.secondaryResourceLevel}`
        : `${this.route}/${parentResourceID}/${this.secondaryResourceLevel}`
    } else {
      newUrl += `${this.route}`
    }
    if (resourceID) {
      newUrl += `/${resourceID}`
    }
    return [newUrl, queryParams]
  }

  private isMyResource(): boolean {
    return this.router.url.startsWith('/my-')
  }

  async getParentResourceID(): Promise<string> {
    if (this.isMyResource()) {
      const myResource = await this.getMyResource()
      return myResource.ID
    } else {
      const urlPieces = this.router.url.split('/')
      const indexOfParent = urlPieces.indexOf(`${this.primaryResourceLevel}`)
      return urlPieces[indexOfParent + 1]
    }
  }

  async getResourceById(resourceID: string): Promise<any> {
    const orderDirection = this.optionsSubject.value.OrderDirection
    const args = await this.createListArgs([resourceID], orderDirection)
    return this.ocService.Get(...args)
  }

  async createListArgs(options: any[], orderDirection = ''): Promise<any[]> {
    /* ordercloud services follow a patter where the paramters to a function (Save, Create, List)
      are the nearly the same for all resource. However, sub resources (supplier users, buyer payment methods, etc...)
      have the parent resource ID as the first paramter before the expected argument
    */
    if (this.primaryResourceLevel === 'orders') {
      // placeholder conditional for getting the supplier order list page running
      // will need to integrate this with the filter on the order list page as a seller
      // user and potentially refactor later
      return [orderDirection || 'Incoming', ...options]
    }
    if (this.secondaryResourceLevel || this.isMyResource()) {
      const parentResourceID = await this.getParentResourceID()
      return [parentResourceID, ...options]
    } else {
      return [...options]
    }
  }

  async findOrGetResourceByID(resourceID: string): Promise<any> {
    const resourceInList = this.resourceSubject.value.Items.find((i: any) =>
      this.checkForResourceMatch(i, resourceID)
    )
    if (resourceInList) {
      return resourceInList
    } else {
      if (resourceID !== REDIRECT_TO_FIRST_PARENT) {
        return await this.getResourceById(resourceID)
      }
    }
  }

  checkForResourceMatch(i: any, resourceID: string): boolean {
    return i.ID === resourceID
  }

  async updateResource(originalID: string, resource: any): Promise<any> {
    const args = await this.createListArgs([originalID, resource])
    const newResource = await this.ocService.Save(...args)
    this.updateResourceSubject(newResource)
    return newResource
  }

  updateResourceSubject(newResource: any): void {
    const resourceIndex = this.resourceSubject.value.Items.findIndex((i: any) =>
      this.checkForNewResourceMatch(i, newResource)
    )
    this.resourceSubject.value.Items[resourceIndex] = newResource
    this.resourceSubject.next(this.resourceSubject.value)
  }

  checkForNewResourceMatch(i: any, newResource: any): boolean {
    return i.ID === newResource.ID
  }

  async deleteResource(resourceID: string): Promise<null> {
    const args = await this.createListArgs([resourceID])
    await this.ocService.Delete(...args)
    this.resourceSubject.value.Items = this.resourceSubject.value.Items.filter(
      (i: any) => i.ID !== resourceID
    )
    this.resourceSubject.next(this.resourceSubject.value)
    return
  }

  async createNewResource(resource: any): Promise<any> {
    const args = await this.createListArgs([resource])
    const newResource = await this.ocService.Create(...args)
    this.resourceSubject.value.Items = [
      ...this.resourceSubject.value.Items,
      newResource,
    ]
    this.resourceSubject.next(this.resourceSubject.value)
    return newResource
  }

  setNewResources(resourceResponse: ListPage<ResourceType>): void {
    this.resourceSubject.next(resourceResponse)
  }

  addResources(resourceResponse: ListPage<ResourceType>): void {
    this.resourceSubject.next({
      Meta: resourceResponse.Meta,
      Items: [...this.resourceSubject.value.Items, ...resourceResponse.Items],
    })
  }

  getNextPage(): void {
    if (
      this.resourceSubject.value.Meta &&
      this.resourceSubject.value.Meta.Page
    ) {
      this.listResources(this.resourceSubject.value.Meta.Page + 1)
    }
  }

  patchFilterState(patch: Options): void {
    const activeOptions = { ...this.optionsSubject.value, ...patch }
    const queryParams = this.mapToUrlQueryParams(activeOptions)
    this.router.navigate([], { queryParams }) // update url, which will call readFromUrlQueryParams()
  }

  toPage(pageNumber: number): void {
    this.patchFilterState({ page: pageNumber || undefined })
  }

  sortBy(field: string): void {
    // temporarily as any until changed to latest oc sdk
    this.patchFilterState({ sortBy: field || undefined } as any)
  }

  searchBy(searchTerm: string): void {
    this.patchFilterState({
      search: searchTerm || undefined,
      searchType: searchTerm ? 'ExactPhrasePrefix' : undefined,
    })
  }

  addFilters(newFilters: ListArgs): void {
    const newFilterDictionary = {
      ...this.optionsSubject.value.filters,
      ...newFilters,
    }
    this.patchFilterState({ filters: newFilterDictionary })
  }

  removeFilters(filtersToRemove: string[]): void {
    const newFilterDictionary = { ...this.optionsSubject.value.filters }
    filtersToRemove.forEach((filter) => {
      if (newFilterDictionary[filter]) {
        delete newFilterDictionary[filter]
      }
    })
    this.patchFilterState({ filters: newFilterDictionary })
  }

  clearSort(): void {
    this.sortBy(undefined)
  }

  clearSearch(): void {
    this.searchBy(undefined)
  }

  clearAllFilters(): void {
    this.patchFilterState({ filters: {} })
  }

  clearResources(): void {
    this.resourceSubject.next({ Meta: {}, Items: [] })
  }

  getRouteFromResourceName(resourceName: string): string {
    return `/${resourceName}`
  }

  hasFilters(): boolean {
    const filters = this.optionsSubject.value
    return Object.entries(filters).some(([key, value]) => {
      return !!value
    })
  }

  areFiltersOnOptions(options: Options): boolean {
    return (
      !!options.search ||
      (options.filters &&
        Object.entries(options.filters).some(([key, value]) => {
          return !!value
        }))
    )
  }

  // Used to update the URL
  mapToUrlQueryParams(options: Options): Params {
    const { sortBy, search, searchType, filters, OrderDirection } = options
    return { sortBy, search, searchType, ...filters, OrderDirection }
  }

  // TODO - move to some other file. Not related to resource crud
  getSuggestedAddresses = (ex): ListPage<BuyerAddress<any>> => {
    if (ex?.Message === 'Address not valid') {
      return ex?.Data?.SuggestedAddresses
    }
    for (const err of ex?.error?.Errors) {
      if (err.ErrorCode === 'blocked by web hook') {
        return err.Data?.Body?.SuggestedAddresses
      }
    }
  }

  // Handle URL updates
  private readFromUrlQueryParams(params: Params): void {
    const { sortBy, search, searchType, OrderDirection, ...filters } = params
    this.optionsSubject.next({
      sortBy,
      search,
      searchType,
      filters,
      OrderDirection,
    })
  }

  private async listWithStatusIndicator(
    options: Options,
    orderDirection = ''
  ): Promise<ListPage<ResourceType>> {
    let isRetry = false
    try {
      this.resourceRequestStatus.next(this.getFetchStatus(options))
      const args = await this.createListArgs([options], orderDirection)
      const resourceResponse = await this.list(args)
      const successStatus = this.getSucessStatus(options, resourceResponse)
      if (
        !isRetry &&
        this.primaryResourceLevel === 'products' &&
        successStatus === 'SUCCESSFUL_NO_ITEMS_WITH_FILTERS'
      ) {
        isRetry = true
        const retryOptions: Options = { ...options, searchType: 'AnyTerm' }
        const retryResourceResponse = await this.list([retryOptions])
        this.resourceRequestStatus.next(
          this.getSucessStatus(retryOptions, retryResourceResponse)
        )
        return retryResourceResponse
      }
      this.resourceRequestStatus.next(successStatus)
      return resourceResponse
    } catch (error) {
      this.resourceRequestStatus.next(ERROR)
      throw error
    }
  }

  private isOnRelatedRoute(): boolean {
    const isOnSubResource =
      this.subResourceList &&
      this.subResourceList.some((subResource) => {
        return this.router.url.includes(`/${subResource.route}`)
      })
    const isOnBaseRoute = this.router.url.includes(this.route)
    const isOnRelatedSubResource = this.router.url.includes(
      `/${this.secondaryResourceLevel}`
    )
    if (!isOnBaseRoute) {
      return false
    } else if (
      isOnSubResource &&
      this.secondaryResourceLevel &&
      isOnRelatedSubResource
    ) {
      return true
    } else if (!isOnSubResource && !this.secondaryResourceLevel) {
      return true
    } else {
      return false
    }
  }

  getUpdatedEditableResource<T>(
    resourceUpdate: ResourceUpdate,
    resourceToUpdate: T
  ): T {
    const updatedResourceCopy: any = this.copyResource(resourceToUpdate)
    const update = _set(
      updatedResourceCopy,
      resourceUpdate.field,
      resourceUpdate.value
    )
    if (
      resourceUpdate.field === 'Product.Inventory.Enabled' &&
      resourceUpdate.value === false
    ) {
      update.Product.Inventory.QuantityAvailable = null
      update.Product.Inventory.OrderCanExceed = false
    }
    return update
  }

  copyResource<T>(resource: T): T {
    return JSON.parse(JSON.stringify(resource))
  }

  // TODO: Refactor to remove duplicate function (function exists in resource-table.component.ts)
  checkIfCreatingNew(): boolean {
    const routeUrl = this.router.routerState.snapshot.url
    const splitUrl = routeUrl.split('/')
    const endUrl =
      this.primaryResourceLevel === 'products'
        ? splitUrl[splitUrl.length - 2]
        : splitUrl[splitUrl.length - 1]
    return endUrl === 'new' || endUrl.startsWith('new?')
  }

  checkForChanges<T>(resourceEditable: T, resourceStatic: T): boolean {
    return JSON.stringify(resourceEditable) !== JSON.stringify(resourceStatic)
  }

  getSaveBtnText(
    dataIsSaving: boolean,
    isCreatingNew: boolean,
    isError?: boolean
  ): string {
    if (isError) return 'Re-submit'
    if (dataIsSaving) return 'Saving...'
    if (isCreatingNew) return 'Create'
    if (!isCreatingNew) return 'Save Changes'
  }

  getParentIDParamName(): string {
    return `${singular(this.primaryResourceLevel)}ID`
  }

  getParentOrSecondaryIDParamName(): string {
    return `${singular(
      this.secondaryResourceLevel || this.primaryResourceLevel
    )}ID`
  }

  getResourceID(resource: any): string {
    return resource?.ID
  }
}
