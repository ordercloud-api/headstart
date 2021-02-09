import {
  Component,
  Input,
  ViewChild,
  OnInit,
  ChangeDetectorRef,
  OnDestroy,
  AfterViewChecked,
} from '@angular/core'
import { ResourceCrudService } from '@app-seller/shared/services/resource-crud/resource-crud.service'
import { faFilter, faHome } from '@fortawesome/free-solid-svg-icons'
import { NgbPopover } from '@ng-bootstrap/ng-bootstrap'
import { Router, ActivatedRoute } from '@angular/router'
import { takeWhile } from 'rxjs/operators'
import { REDIRECT_TO_FIRST_PARENT } from '@app-seller/layout/header/header.config'
import { getPsHeight } from '@app-seller/shared/services/dom.helper'
import { ListPage } from '@ordercloud/headstart-sdk'

@Component({
  selector: 'resource-select-dropdown-component',
  templateUrl: './resource-select-dropdown.component.html',
  styleUrls: ['./resource-select-dropdown.component.scss'],
})
export class ResourceSelectDropdown
  implements OnInit, OnDestroy, AfterViewChecked {
  @ViewChild('popover', { static: false })
  public popover: NgbPopover
  faFilter = faFilter
  faHome = faHome
  searchTerm = ''
  selectedParentResourceName = 'Fetching Data'
  alive = true
  resourceSelectDropdownHeight = 450

  @Input()
  ocService: ResourceCrudService<any>
  @Input()
  parentService: ResourceCrudService<any>
  parentResourceList: ListPage<any> = { Meta: {}, Items: [] }

  constructor(
    private router: Router,
    private activatedRoute: ActivatedRoute,
    private changeDetectorRef: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.setParentResourceSubscription()
    this.setParentResourceSelectionSubscription()
  }

  ngAfterViewChecked() {
    // TODO: Magic number ... the 'search' element doesn't exist in the DOM at time of instantiation
    this.resourceSelectDropdownHeight =
      getPsHeight('additional-item-resource-select-dropdown') - 75
    this.changeDetectorRef.detectChanges()
  }
  private setParentResourceSubscription() {
    this.parentService.resourceSubject
      .pipe(takeWhile(() => this.alive))
      .subscribe((resourceList) => {
        this.parentResourceList = resourceList
        this.changeDetectorRef.detectChanges()
      })
  }

  setParentResourceSelectionSubscription() {
    this.activatedRoute.params
      .pipe(takeWhile(() => this.alive))
      .subscribe(async (params) => {
        const parentResourceID = await this.parentService.getParentResourceID()
        if (parentResourceID !== REDIRECT_TO_FIRST_PARENT) {
          const parentIDParamName = this.parentService.getParentIDParamName()
          const resourceID = params[parentIDParamName]
          if (params && resourceID) {
            const resource = await this.parentService.findOrGetResourceByID(
              resourceID
            )
            if (resource) {
              this.selectedParentResourceName =
                resource.Name || resource.AppName
            }
          }
        }
      })
  }

  selectParentResource(resource: any) {
    this.ocService.selectParentResource(resource)

    // reset the search form when selecting resource
    this.parentService.listResources()
    this.searchTerm = ''
  }

  searchedResources(searchText: any) {
    this.parentService.listResources(1, searchText)
    this.searchTerm = searchText
  }

  handleScrollEnd() {
    const totalPages = this.parentResourceList.Meta.TotalPages
    const nextPageNumber = this.parentResourceList.Meta.Page + 1
    if (totalPages >= nextPageNumber)
      this.parentService.listResources(nextPageNumber, this.searchTerm)
  }

  ngOnDestroy() {
    this.alive = false
  }
}
