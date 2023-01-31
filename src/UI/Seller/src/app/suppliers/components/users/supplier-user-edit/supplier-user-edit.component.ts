import { Component, Input, Output, EventEmitter } from '@angular/core'
import { UntypedFormGroup, UntypedFormControl, Validators } from '@angular/forms'
import { UserGroupAssignment, User } from 'ordercloud-javascript-sdk'
import { SupplierUserService } from '../supplier-user.service'
import { ValidateEmail } from '@app-seller/validators/validators'
@Component({
  selector: 'app-supplier-user-edit',
  templateUrl: './supplier-user-edit.component.html',
  styleUrls: ['./supplier-user-edit.component.scss'],
})
export class SupplierUserEditComponent {
  @Input()
  filterConfig
  @Input()
  set resourceInSelection(sellerUser: User) {
    this.selectedResource = sellerUser
    this.createSupplierUserForm(sellerUser)
  }
  @Output()
  updateResource = new EventEmitter<any>()
  @Output()
  userGroupAssignments = new EventEmitter<UserGroupAssignment[]>()
  isCreatingNew: boolean
  selectedResource: User
  resourceForm: UntypedFormGroup
  constructor(public supplierUserService: SupplierUserService) {
    this.isCreatingNew = this.supplierUserService.checkIfCreatingNew()
  }
  createSupplierUserForm(user: User) {
    this.resourceForm = new UntypedFormGroup({
      Username: new UntypedFormControl(user.Username, Validators.required),
      FirstName: new UntypedFormControl(user.FirstName, Validators.required),
      LastName: new UntypedFormControl(user.LastName, Validators.required),
      Email: new UntypedFormControl(user.Email, [Validators.required, ValidateEmail]),
      Active: new UntypedFormControl(user.Active),
    })
  }
  updateResourceFromEvent(event: any, field: string): void {
    field === 'Active'
      ? this.updateResource.emit({ value: event.target.checked, field })
      : this.updateResource.emit({ value: event.target.value, field })
  }
  addUserGroupAssignments(event): void {
    this.userGroupAssignments.emit(event)
  }
}
