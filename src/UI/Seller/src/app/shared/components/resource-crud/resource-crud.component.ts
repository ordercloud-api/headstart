import { OnInit, OnDestroy, ChangeDetectorRef, NgZone, Directive } from '@angular/core'
import { takeWhile } from 'rxjs/operators'
import { ResourceCrudService } from '@app-seller/shared/services/resource-crud/resource-crud.service'
import { FormGroup } from '@angular/forms'
import { Router, ActivatedRoute } from '@angular/router'
import { REDIRECT_TO_FIRST_PARENT } from '@app-seller/layout/header/header.config'
import { ListPage } from '@ordercloud/headstart-sdk'
import { BehaviorSubject } from 'rxjs'
import { ResourceUpdate } from '@app-seller/models/shared.types'
import OrderCloudError from 'ordercloud-javascript-sdk/dist/utils/OrderCloudError'

@Directive()
export abstract class ResourceCrudComponent<ResourceType>
  implements OnInit, OnDestroy {
  alive = true
  resourceList: ListPage<ResourceType> = { Meta: {}, Items: [] }

  // empty string if no resource is selected
  selectedResourceID = ''
  updatedResource = {} as ResourceType
  resourceInSelection = {} as ResourceType
  resourceForm: FormGroup
  isMyResource = false
  isSupplierUser = false
  parentResourceID: string
  parentResourceIDSubject = new BehaviorSubject<string>(undefined)
  submitError: OrderCloudError

  // form setting defined in component implementing this component
  createForm: (resource: any) => FormGroup

  ocService: ResourceCrudService<ResourceType>
  filterConfig: any = {}
  router: Router
  isCreatingNew: boolean
  dataIsSaving = false

  constructor(
    private changeDetectorRef: ChangeDetectorRef,
    ocService: any,
    router: Router,
    private activatedRoute: ActivatedRoute,
    private ngZone: NgZone,
    createForm?: (resource: any) => FormGroup
  ) {
    this.ocService = ocService
    this.router = router
    this.createForm = createForm
  }

  public navigate(url: string, options: any): void {
    /*
     * Had a bug where clicking on a resource on the second page of resources was triggering an error
     * navigation trigger outside of Angular zone. Might be caused by inheritance or using
     * changeDetector.detectChange, but couldn't resolve any other way
     * Please remove the need for this if you can
     * https://github.com/angular/angular/issues/25837
     */
    if (Object.keys(options)) {
      this.ngZone.run(() => this.router.navigate([url], options)).then()
    } else {
      this.ngZone.run(() => this.router.navigate([url])).then()
    }
  }

  async ngOnInit(): Promise<void> {
    await this.determineViewingContext()
    this.subscribeToResources()
    await this.subscribeToResourceSelection()
    this.setForm(this.updatedResource)
  }

  subscribeToResources(): void {
    this.ocService.resourceSubject
      .pipe(takeWhile(() => this.alive))
      .subscribe((resourceList) => {
        this.resourceList = resourceList
        this.changeDetectorRef.detectChanges()
      })
  }

  async determineViewingContext(): Promise<void> {
    this.isMyResource = this.router.url.startsWith('/my-')
    this.isSupplierUser = await this.ocService.isSupplierUser()
    if (this.isMyResource) {
      const myResource = await this.ocService.getMyResource()
      const shouldDisplayList =
        this.router.url.includes('locations') ||
        this.router.url.includes('users')
      if (!shouldDisplayList) this.setResourceSelectionFromResource(myResource)
    }
  }

  subscribeToResourceSelection(): void {
    // eslint-disable-next-line @typescript-eslint/no-misused-promises
    this.activatedRoute.params.subscribe(async (params) => {
      this.parentResourceID = await this.ocService.getParentResourceID()
      this.parentResourceIDSubject.next(this.parentResourceID)
      if (this.parentResourceID !== REDIRECT_TO_FIRST_PARENT) {
        this.setIsCreatingNew()
        const resourceIDSelected =
          params[this.ocService.getParentOrSecondaryIDParamName()] // Example - Reports uses a different prefix to ID
        if (this.isCreatingNew) {
          this.setResoureObjectsForCreatingNew()
        } else if (resourceIDSelected) {
          this.setResourceSelectionFromID(resourceIDSelected)
        }
      }
    })
  }

  setForm(resource: any): void {
    if (this.createForm) {
      this.resourceForm = this.createForm(resource)
      this.changeDetectorRef.detectChanges()
    }
  }

  resetForm(resource: any): void {
    if (this.createForm) {
      this.resourceForm.reset(this.createForm(resource))
      this.changeDetectorRef.detectChanges()
    }
  }

  handleScrollEnd(): void {
    if (this.resourceList.Meta.TotalPages > this.resourceList.Meta.Page) {
      this.ocService.getNextPage()
    }
  }

  searchResources(searchStr: string): void {
    this.ocService.searchBy(searchStr)
  }

  async setResourceSelectionFromID(resourceID: string): Promise<void> {
    this.selectedResourceID = resourceID || ''
    const resource = await this.ocService.findOrGetResourceByID(resourceID)
    this.resourceInSelection = this.ocService.copyResource(resource)
    this.setUpdatedResourceAndResourceForm(resource)
  }

  setResourceSelectionFromResource(resource: any): void {
    this.selectedResourceID = (resource && resource.ID) || ''

    this.resourceInSelection = this.ocService.copyResource(resource)
    this.setUpdatedResourceAndResourceForm(resource)
  }

  setResoureObjectsForCreatingNew(): void {
    this.resourceInSelection = this.ocService.emptyResource
    this.setUpdatedResourceAndResourceForm(this.ocService.emptyResource)
  }

  async selectResource(resource: any): Promise<void> {
    const [
      newURL,
      queryParams,
    ] = await this.ocService.constructNewRouteInformation(
      this.ocService.getResourceID(resource) || ''
    )
    this.navigate(newURL, { queryParams })
  }

  updateResource(resourceUpdate: ResourceUpdate): void {
    this.updatedResource = this.ocService.getUpdatedEditableResource(resourceUpdate as ResourceUpdate, this.updatedResource)
    if (resourceUpdate.form) {
      this.resourceForm = resourceUpdate.form
    }
    this.changeDetectorRef.detectChanges()
  }

  handleUpdateResource(event: any, field: string): void {
    const resourceUpdate = {
      field,
      value: field === 'Active' ? event.target.checked : event.target.value,
      form: this.resourceForm,
    }
    this.updateResource(resourceUpdate)
  }

  saveUpdates(): void {
    if (this.isCreatingNew) {
      this.createNewResource()
    } else {
      this.updateExistingResource()
    }
  }

  async deleteResource(): Promise<void> {
    await this.ocService.deleteResource(this.selectedResourceID)
    this.selectResource({})
  }

  discardChanges(): void {
    this.setUpdatedResourceAndResourceForm(this.resourceInSelection)
  }

  async updateExistingResource(): Promise<void> {
    try {
      this.dataIsSaving = true // disables submit button while loading
      const updatedResource = await this.ocService.updateResource(
        (this.resourceInSelection as any).ID,
        this.updatedResource
      )
      this.resourceInSelection = this.ocService.copyResource(updatedResource)
      this.setUpdatedResourceAndResourceForm(updatedResource)
      this.dataIsSaving = false
    } catch (ex) {
      this.dataIsSaving = false
      this.submitError = ex
      throw ex
    }
  }

  setUpdatedResourceAndResourceForm(updatedResource: any): void {
    this.updatedResource = this.ocService.copyResource(updatedResource)
    const originalResource = this.ocService.copyResource(updatedResource)
    this.setForm(originalResource)
    this.changeDetectorRef.detectChanges()
  }

  async createNewResource(): Promise<void> {
    // dataIsSaving indicator is used in the resource table to conditionally tell the
    // submit button to disable
    try {
      this.dataIsSaving = true
      const newResource = await this.ocService.createNewResource(
        this.updatedResource
      )
      this.selectResource(newResource)
      this.dataIsSaving = false
    } catch (ex) {
      this.dataIsSaving = false
      this.submitError = ex
      throw ex
    }
  }

  ngOnDestroy(): void {
    this.alive = false
  }

  private setIsCreatingNew(): void {
    const routeUrl = this.router.routerState.snapshot.url
    const splitUrl = routeUrl.split('/')
    const endUrl = splitUrl[splitUrl.length - 1]
    /* Reduce possibility of errors: all IDs with the word new must equal it exactly,
    or begin with the word new and have a question mark following it for query params. */
    this.isCreatingNew = endUrl === 'new' || endUrl.startsWith('new?')
  }
}
