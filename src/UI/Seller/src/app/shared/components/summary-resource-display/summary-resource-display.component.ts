import {
  Component,
  Input,
  OnChanges,
  SimpleChanges,
  Inject,
} from '@angular/core'
import { singular } from 'pluralize'
import { SUMMARY_RESOURCE_INFO_PATHS_DICTIONARY } from '@app-seller/shared/services/configuration/table-display'
import { OcCategoryService } from '@ordercloud/angular-sdk'
import {
  faChevronDown,
  faChevronRight,
  faPlus,
} from '@fortawesome/free-solid-svg-icons'
import { Router } from '@angular/router'
import { applicationConfiguration } from '@app-seller/config/app.config'
import { AppConfig } from '@app-seller/shared'
import { getProductSmallImageUrl, getSupplierLogoSmallUrl, PLACEHOLDER_URL, PRODUCT_IMAGE_PATH_STRATEGY, SUPPLIER_LOGO_PATH_STRATEGY } from '@app-seller/shared/services/assets/asset.helper'

@Component({
  selector: 'summary-resource-display-component',
  templateUrl: './summary-resource-display.component.html',
  styleUrls: ['./summary-resource-display.component.scss'],
})
export class SummaryResourceDisplay implements OnChanges {
  isLoading = false
  _primaryHeader = ''
  _secondaryHeader = ''
  _imgPath = ''
  _shouldShowImage = false
  _isNewPlaceHolder = false
  _isExpandable = false
  faChevronDown = faChevronDown
  faChevronRight = faChevronRight
  faPlus = faPlus
  _isResourceExpanded: boolean
  _resource: any
  _resourceList: any
  _parentResourceID: any = ''
  _depthCount: number
  _isCreatingSubResource: boolean

  @Input()
  set resourceList(value: any) {
    this._resourceList = value
  }
  @Input()
  resourceType: any

  @Input()
  set isNewPlaceHolder(value: boolean) {
    this._isNewPlaceHolder = value
  }

  @Input()
  set resource(value: any) {
    this._resource = value
  }

  @Input()
  set parentResourceID(value: any) {
    this._parentResourceID = value
  }

  @Input()
  set isCreatingSubResource(value: any) {
    this._isCreatingSubResource = value
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.resourceType?.firstChange) {
      this.setDisplayValuesForPlaceholder()
    }
    if (changes.resource?.firstChange) {
      this.setDisplayValuesForResource(changes.resource.currentValue)
    }
  }

  constructor(
    private ocCategoryService: OcCategoryService,
    private router: Router,
    @Inject(applicationConfiguration) private appConfig: AppConfig
  ) {}

  setDisplayValuesForResource(resource: any) {
    if (resource.ProductAssignments) resource = resource.Product
    this._primaryHeader = this.getValueOnExistingResource(
      resource,
      'toPrimaryHeader'
    )
    this._secondaryHeader = this.getValueOnExistingResource(
      resource,
      'toSecondaryHeader'
    )
    this._shouldShowImage = !!SUMMARY_RESOURCE_INFO_PATHS_DICTIONARY[
      this.resourceType
    ].toImage
    this._imgPath =
      this.getValueOnExistingResource(resource, 'toImage') || PLACEHOLDER_URL
    this._isExpandable = resource.ChildCount > 0
  }

  getValueOnExistingResource(value: any, valueType: string) {
    const pathToValue =
      SUMMARY_RESOURCE_INFO_PATHS_DICTIONARY[this.resourceType][valueType]
    const piecesOfPath = pathToValue.split('.')
    if (pathToValue) {
      if (pathToValue === PRODUCT_IMAGE_PATH_STRATEGY) {
        return getProductSmallImageUrl(value)
      } else if (pathToValue === SUPPLIER_LOGO_PATH_STRATEGY) {
        return getSupplierLogoSmallUrl(value)
      } else {
        let currentObject = value
        piecesOfPath.forEach((piece) => {
          currentObject = currentObject && currentObject[piece]
        })
        return currentObject
      }
    } else {
      return ''
    }
  }

  setDisplayValuesForPlaceholder() {
    this._primaryHeader = this.getValueOnPlaceHolderResource('toPrimaryHeader')
    this._secondaryHeader = this.getValueOnPlaceHolderResource(
      'toSecondaryHeader'
    )
    this._imgPath = this.getValueOnPlaceHolderResource('toImage')
  }

  getValueOnPlaceHolderResource(valueType: string) {
    switch (valueType) {
      case 'toPrimaryHeader':
        return this._isCreatingSubResource
          ? `Your new sub-${singular(this.resourceType)}`
          : `Your new ${singular(this.resourceType)}`
      default:
        return ''
    }
  }

  // Further indent display of subsequent categorical tiers
  getIndent() {
    const depthCount = -1
    return !this._resource?.ParentID
      ? 0
      : this.getResourceDepth(this._resource, depthCount)
  }

  getResourceDepth(resource, depthCount) {
    depthCount++
    this._depthCount = depthCount
    const parentResource = this._resourceList.find(
      (item) => item.ID === resource?.ParentID
    )
    return resource?.ParentID
      ? this.getResourceDepth(parentResource, depthCount)
      : depthCount * 15
  }

  async toggleNestedResources() {
    let index = this._resourceList.indexOf(this._resource)
    if (!this._isResourceExpanded) {
      // Prevent another API call if it's already been made.
      if (this._resource.children) {
        this._resource.children.forEach((item) =>
          this._resourceList.splice(++index, 0, item)
        )
      } else {
        this.isLoading = true
        const options = { filters: { ParentID: this._resource.ID } }
        const categoryResponse = await this.ocCategoryService
          .List(this._parentResourceID, options)
          .toPromise()
        categoryResponse.Items.forEach((item) =>
          this._resourceList.splice(++index, 0, item)
        )
        // Attach category response to new children property
        this._resource.children = categoryResponse.Items
      }
      this._isResourceExpanded = true
      this.isLoading = false
    } else {
      // Retrieve children that will no longer be expanded, to prevent repeat API calls.
      const allChildren = this.getAllChildren(this._resource)
      const collapseBreakPointObject = this._resourceList.find(
        (item, i) => i > index && !allChildren.includes(item)
      )
      const collapseBreakPointIndex = this._resourceList.findIndex(
        (item) => item === collapseBreakPointObject
      )
      const numberToCollapse =
        collapseBreakPointIndex !== -1
          ? collapseBreakPointIndex - index - 1
          : this._resourceList.length - 1
      this._resourceList.splice(++index, numberToCollapse)
      this._isResourceExpanded = false
    }
  }

  // Retrieve all children, grandchild, etc. that will need to be collapsed.
  getAllChildren(resource) {
    const children = resource.children
    if (children) {
      const nextChildren = []
      children.forEach((child) =>
        this.getAllChildren(child).forEach((nextChild) =>
          nextChildren.push(nextChild)
        )
      )
      const allChildren = children.concat(nextChildren)
      return allChildren
    } else {
      return []
    }
  }

  addNestedResource(resource) {
    event.stopPropagation()
    const routeUrl = this.router.routerState.snapshot.url
    const splitUrl = routeUrl.split('/')
    this.router.navigate([`${splitUrl[1]}/${splitUrl[2]}/${splitUrl[3]}/new`], {
      queryParams: { ParentCategory: resource },
    })
  }

  // Nested resources cannot be added beyond a third tier
  isAtMaximumDepth() {
    const parentOfResource = this._resource?.ParentID
    let parentOfParentOfResource
    if (parentOfResource) {
      parentOfParentOfResource = this._resourceList.find(
        (resource) => resource.ID === parentOfResource
      )
    }
    parentOfParentOfResource = parentOfParentOfResource?.ParentID
    return parentOfResource && parentOfParentOfResource
  }
}
