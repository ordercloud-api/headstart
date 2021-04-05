import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core'
import {
  HSCatalog,
  HSCatalogAssignmentRequest,
} from '@ordercloud/headstart-sdk'
import { Router } from '@angular/router'
import { CatalogsTempService } from '@app-seller/shared/services/middleware-api/catalogs-temp.service'

@Component({
  selector: 'app-buyer-location-catalogs',
  templateUrl: './buyer-location-catalogs.component.html',
  styleUrls: ['./buyer-location-catalogs.component.scss'],
})
export class BuyerLocationCatalogs implements OnInit {
  buyerID: string
  locationID = ''

  @Input()
  set locationUserGroup(locationUserGroup: any) {
    if (locationUserGroup && Object.keys(locationUserGroup)) {
      this.locationID = locationUserGroup?.ID
      this.resetAssignments(locationUserGroup.xp.CatalogAssignments || [])
    }
  }
  @Input()
  catalogs: HSCatalog[] = []
  @Input()
  isCreatingNew: boolean
  @Output()
  assignmentsToAdd = new EventEmitter<HSCatalogAssignmentRequest>()

  locationCatalogAssignmentsEditable: string[] = []
  locationCatalogAssignmentsStatic: string[] = []
  addLocationCatalogAssignments: string[] = []
  delLocationCatalogAssignments: string[] = []
  catalogAssignments: HSCatalogAssignmentRequest = { CatalogIDs: [] }
  areChanges = false
  dataIsSaving = false

  constructor(
    private router: Router,
    private hsCatalogService: CatalogsTempService
  ) {}

  ngOnInit() {
    var url = this.router?.routerState?.snapshot?.url
    if(url && url.split('/').length) {
      this.buyerID = url.split('/')[2]
    }
  }

  resetAssignments(assignments: string[]): void {
    this.locationCatalogAssignmentsEditable = assignments
    this.locationCatalogAssignmentsStatic = assignments
    this.checkForChanges()
  }

  checkForChanges(): void {
    this.addLocationCatalogAssignments = this.locationCatalogAssignmentsEditable.filter(
      (l) => !this.locationCatalogAssignmentsStatic.includes(l)
    )
    this.delLocationCatalogAssignments = this.locationCatalogAssignmentsStatic.filter(
      (l) => !this.locationCatalogAssignmentsEditable.includes(l)
    )
    this.catalogAssignments.CatalogIDs = this.locationCatalogAssignmentsEditable
    if(this.addLocationCatalogAssignments.length) {
      this.assignmentsToAdd.emit(this.catalogAssignments)
    }
    this.areChanges =
      !!this.delLocationCatalogAssignments.length ||
      !!this.addLocationCatalogAssignments.length
  }

  isAssigned(catalog: HSCatalog): boolean {
    return this.locationCatalogAssignmentsEditable.includes(catalog.ID)
  }

  toggleAssignment(catalog: HSCatalog): void {
    if (this.isAssigned(catalog)) {
      this.locationCatalogAssignmentsEditable = this.locationCatalogAssignmentsEditable.filter(
        (c) => c !== catalog.ID
      )
    } else {
      this.locationCatalogAssignmentsEditable = [
        ...this.locationCatalogAssignmentsEditable,
        catalog.ID,
      ]
    }
    this.checkForChanges()
  }

  discardChanges(): void {
    this.resetAssignments(this.locationCatalogAssignmentsStatic)
  }

  async saveChanges(): Promise<void> {
    this.resetAssignments(this.locationCatalogAssignmentsEditable)
    await this.hsCatalogService.setLocationAssignments(
      this.buyerID,
      this.locationID,
      this.locationCatalogAssignmentsEditable
    )
  }
}
