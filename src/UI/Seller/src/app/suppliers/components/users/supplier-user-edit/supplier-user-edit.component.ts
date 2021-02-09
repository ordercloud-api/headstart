import { Component, Input, Output, EventEmitter } from '@angular/core'
import { get as _get } from 'lodash'
import { FormGroup, FormControl, Validators } from '@angular/forms'
import { Router } from '@angular/router'
import { UserGroupAssignment, User } from '@ordercloud/angular-sdk'
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
  resourceForm: FormGroup
  constructor(public supplierUserService: SupplierUserService) {
    this.isCreatingNew = this.supplierUserService.checkIfCreatingNew()
  }
  createSupplierUserForm(user: User) {
    this.resourceForm = new FormGroup({
      Username: new FormControl(user.Username, Validators.required),
      FirstName: new FormControl(user.FirstName, Validators.required),
      LastName: new FormControl(user.LastName, Validators.required),
      Email: new FormControl(user.Email, [Validators.required, ValidateEmail]),
      Active: new FormControl(user.Active),
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
