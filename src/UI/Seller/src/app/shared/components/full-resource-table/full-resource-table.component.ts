import { Component, Input, Output, EventEmitter, Inject } from '@angular/core'
import { FULL_TABLE_RESOURCE_DICTIONARY } from '@app-seller/shared/services/configuration/table-display'
import { RequestStatus } from '@app-seller/models/resource-crud.types'
import {
  faCopy,
  faSort,
  faSortUp,
  faSortDown,
} from '@fortawesome/free-solid-svg-icons'
import { ToastrService } from 'ngx-toastr'
import { ResourceCrudService } from '@app-seller/shared/services/resource-crud/resource-crud.service'
import { SortDirection } from './sort-direction.enum'
import { Router, ActivatedRoute } from '@angular/router'
import { ImpersonationService } from '@app-seller/shared/services/impersonation/impersonation.service'
import { applicationConfiguration } from '@app-seller/config/app.config'
import { AppConfig, ResourceRow } from '@app-seller/shared'
import { getProductSmallImageUrl, getSupplierLogoSmallUrl } from '@app-seller/shared/services/assets/asset.helper'

@Component({
  selector: 'full-resource-table-component',
  templateUrl: './full-resource-table.component.html',
  styleUrls: ['./full-resource-table.component.scss'],
})
export class FullResourceTableComponent {
  headers = []
  rows = []
  numberOfColumns = 1
  faCopy = faCopy
  faSort = faSort
  faSortUp = faSortUp
  faSortDown = faSortDown
  sortDirection: SortDirection = SortDirection.None
  activeSort: string
  objectPreviewText: string
  routeUrl: string
  _resourceList = { Meta: {}, Items: [] }
  defaultSortList: string[] = ['NAME']

  @Input()
  resourceType: any
  @Input()
  requestStatus: RequestStatus
  @Input()
  set resourceList(value: any) {
    this.routeUrl = this.router.routerState.snapshot.url
    this._resourceList = value
    this.setDisplayValuesForResource(value.Items)
  }
  @Input()
  ocService: ResourceCrudService<any>
  @Output()
  resourceSelected = new EventEmitter()

  constructor(
    private router: Router,
    private toastrService: ToastrService,
    private activatedRoute: ActivatedRoute,
    @Inject(applicationConfiguration) private appConfig: AppConfig,
    private impersonationService: ImpersonationService
  ) {}

  setDisplayValuesForResource(resources: any[] = []) {
    this.headers = this.getHeaders()
    this.rows = this.getRows(resources)
    this.numberOfColumns = this.getNumberOfColumns(this.resourceType)
  }

  getHeaders(): object[] {
    return FULL_TABLE_RESOURCE_DICTIONARY[
      this.resourceType
    ].fields.filter((r) => this.isValidForDisplay(r))
  }

  isValidForDisplay(field: any): boolean {
    return !(
      field?.queryRestriction &&
      !this.routeUrl.includes(field?.queryRestriction)
    )
  }

  getRows(resources: any[]): ResourceRow[] {
    return resources.map((resource) => {
      return this.createResourceRow(resource)
    })
  }

  getNumberOfColumns(resourceType: string): number {
    return FULL_TABLE_RESOURCE_DICTIONARY[resourceType].fields.length
  }

  createResourceRow(resource: any): ResourceRow {
    const resourceConfiguration =
      FULL_TABLE_RESOURCE_DICTIONARY[this.resourceType]
    const fields = resourceConfiguration.fields.filter((r) =>
      this.isValidForDisplay(r)
    )
    const resourceCells = fields.map((fieldConfiguration) => {
      return {
        type: fieldConfiguration.type,
        value: this.getValueOnExistingResource(
          resource,
          fieldConfiguration.path
        ),
      }
    })
    return {
      resource,
      cells: resourceCells,
      imgPath: resourceConfiguration.imgPath ? this.getImage(resource) : '',
    }
  }

  copyObject(resource: any) {
    this.toastrService.success(null, 'Copied to clipboard!', {
      disableTimeOut: false,
      closeButton: true,
      tapToDismiss: true,
    })
    const copy = document.createElement('textarea')
    document.body.appendChild(copy)
    copy.value = JSON.stringify(resource)
    copy.select()
    document.execCommand('copy')
    document.body.removeChild(copy)
  }

  previewObject(resource: any) {
    this.objectPreviewText = JSON.stringify(resource)
  }

  getImage(resource: any): string {
    if(this.resourceType === 'products') {
      return getProductSmallImageUrl(resource)
    } else if(this.resourceType === 'suppliers') {
      return getSupplierLogoSmallUrl(resource)
    } else return ''
  }

  selectResource(value: any) {
    this.resourceSelected.emit(value)
  }

  getValueOnExistingResource(value: any, path: string) {
    const piecesOfPath = path.split('.')
    if (path) {
      let currentObject = value
      piecesOfPath.forEach((piece) => {
        currentObject = currentObject && currentObject[piece]
      })
      return currentObject
    } else {
      return ''
    }
  }

  handleSort(header: string) {
    this.activeSort = header
    this.sortDirection = (this.sortDirection + 1) % 3
    if (this.sortDirection === SortDirection.None) {
      this.activeSort = ''
    }
    const sortInverse = this.sortDirection === SortDirection.Desc ? '!' : ''
    this.ocService.sortBy(sortInverse + this.activeSort)
  }

  getSortArrowDirection(header: string) {
    if (
      this.activeSort === header &&
      this.sortDirection === SortDirection.Asc
    ) {
      return faSortUp
    } else if (
      this.activeSort === header &&
      this.sortDirection === SortDirection.Desc
    ) {
      return faSortDown
    } else {
      return faSort
    }
  }

  async impersonateUser(resource: any) {
    event.stopPropagation()
    const buyerID = this.activatedRoute.snapshot.params?.buyerID
    await this.impersonationService.impersonateUser(buyerID, resource)
  }
}
