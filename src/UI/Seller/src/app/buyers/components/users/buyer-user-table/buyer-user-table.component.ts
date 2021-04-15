import { Component, ChangeDetectorRef, NgZone } from '@angular/core'
import { ResourceCrudComponent } from '@app-seller/shared/components/resource-crud/resource-crud.component'
import { User, UserGroupAssignment } from '@ordercloud/angular-sdk'
import { Router, ActivatedRoute } from '@angular/router'
import { BuyerUserService } from '../buyer-user.service'
import { BuyerService } from '../../buyers/buyer.service'
import { UserGroupTypes } from '@app-seller/shared/components/user-group-assignments/user-group-assignments.constants'
@Component({
  selector: 'app-buyer-user-table',
  templateUrl: './buyer-user-table.component.html',
  styleUrls: ['./buyer-user-table.component.scss'],
})
export class BuyerUserTableComponent extends ResourceCrudComponent<User> {
  permissionUserGroupAssignments: UserGroupAssignment[] = []
  locationUserGroupAssignments: UserGroupAssignment[] = []

  constructor(
    private buyerUserService: BuyerUserService,
    private buyerService: BuyerService, //used in <resource-table-component/>
    changeDetectorRef: ChangeDetectorRef,
    router: Router,
    activatedroute: ActivatedRoute,
    ngZone: NgZone
  ) {
    super(changeDetectorRef, buyerUserService, router, activatedroute, ngZone)
  }



  async createNewResource() {
    try {
      this.dataIsSaving = true
      const user = await this.buyerUserService.createNewResource(
        this.updatedResource
      )
      await this.executeSupplierUserSecurityProfileAssignmentRequests(user)
      this.selectResource(user)
      this.dataIsSaving = false
    } catch (ex) {
      this.dataIsSaving = false
      throw ex
    }
  }

  async executeSupplierUserSecurityProfileAssignmentRequests(
    user: User
  ): Promise<void> {
    let assignmentsToMake = [
      ...this.permissionUserGroupAssignments,
      ...this.locationUserGroupAssignments,
    ]
    assignmentsToMake = assignmentsToMake.map((a) => {
      a.UserID = user.ID
      return a
    })
    const buyerID = await this.buyerUserService.getParentResourceID()
    await this.buyerUserService.updateUserUserGroupAssignments(
      buyerID,
      assignmentsToMake,
      [],
      assignmentsToMake.length > 0
    )
  }

  updateResource($event: any): void {
    const allValues = $event.getRawValue() 
    this.locationUserGroupAssignments = allValues.BuyerGroupAssignments || []
    this.permissionUserGroupAssignments = allValues.PermissionGroupAssignments || []
    const buyerUserForm = {
      ...allValues,
      ID: this.updatedResource.ID,
      xp: { Country: allValues.Country },
    } 
    this.resourceForm = $event
    this.updatedResource = buyerUserForm
  }
}
